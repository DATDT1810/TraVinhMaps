using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using TraVinhMaps.Application.Features.Translate.Interface;
using TraVinhMaps.Application.Features.Translate.Models;

public sealed class TranslationService : ITranslationService, IDisposable
{
    private const int MaxBatchSize = 40;
    private const int MaxRetries = 3;
    private const string ModelId = "gemini-2.5-flash";
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan MinCallInterval = TimeSpan.FromSeconds(1.1);

    private readonly HttpClient _http;
    private readonly TranslationCache _cache;
    private readonly ILogger<TranslationService> _log;

    private static readonly SemaphoreSlim _gate = new(2, 2);
    private static readonly object _rateLock = new();
    private static DateTime _lastCallUtc = DateTime.MinValue;

    private readonly Timer _flushTimer;
    private static readonly TimeSpan FlushEvery = TimeSpan.FromSeconds(30);

    public TranslationService(IConfiguration cfg, IHttpClientFactory factory, ILogger<TranslationService> log)
    {
        _log = log;

        var key = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? cfg["Gemini:ApiKey"]
                  ?? throw new ArgumentNullException("Gemini API key missing");

        _cache = new TranslationCache(Path.Combine("Cache", "translations.json"), log);
        Directory.CreateDirectory(Path.GetDirectoryName(_cache.FilePath)!);

        _http = factory.CreateClient();
        _http.Timeout = HttpTimeout;
        _http.DefaultRequestHeaders.Add("x-goog-api-key", key);
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");

        _flushTimer = new Timer(_ => _cache.FlushIfDirty(), null, FlushEvery, FlushEvery);
    }

    public void Dispose()
    {
        _flushTimer?.Dispose();
        _cache.FlushIfDirty();
        _http.Dispose();
    }

    public async Task<TranslationResult> TranslateAsync(string text, string sourceLang = "en", string targetLang = "vi", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text required", nameof(text));

        if (_cache.TryGet(text, sourceLang, targetLang, out var cached))
            return new TranslationResult { TranslatedText = cached!, FromCache = true, ModelUsed = "cache", Cost = 0 };

        var dict = await TranslateBatchAsync(new[] { text }, sourceLang, targetLang, cancellationToken);
        return new TranslationResult { TranslatedText = dict[text], FromCache = false, ModelUsed = ModelId };
    }

    public async Task<Dictionary<string, string>> TranslateBatchAsync(IEnumerable<string> texts, string sourceLang, string targetLang, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var input = texts?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList()
                    ?? throw new ArgumentNullException(nameof(texts));

        var result = new Dictionary<string, string>(input.Count);
        var need = new List<string>();

        foreach (var t in input)
        {
            if (_cache.TryGet(t, sourceLang, targetLang, out var hit))
                result[t] = hit!;
            else
                need.Add(t);
        }

        if (need.Count == 0) return result;

        foreach (var batch in Chunk(need, MaxBatchSize))
        {
            var batchSw = System.Diagnostics.Stopwatch.StartNew();
            var dict = await CallGeminiAsync(batch, sourceLang, targetLang, ct);
            batchSw.Stop();

            if (dict.Count < batch.Count)
                _log.LogWarning($"Batch fallback: requested {batch.Count}, got {dict.Count}, fallback {batch.Count - dict.Count} sentences.");

            foreach (var kv in dict)
                result[kv.Key] = kv.Value;

            _log.LogInformation($"Batch size {batch.Count} done in {batchSw.Elapsed.TotalSeconds:F2}s");
        }

        sw.Stop();
        _log.LogInformation($"Total batch of {input.Count} sentences translated in {sw.Elapsed.TotalSeconds:F2}s");
        return result;
    }

    private async Task<Dictionary<string, string>> CallGeminiAsync(List<string> batch, string src, string tgt, CancellationToken ct)
    {
        _log.LogInformation($"Start CallGeminiAsync: {batch.Count} sentences, src={src}, tgt={tgt}");
        var prompt = BuildPrompt(batch, src, tgt);

        var payload = JsonSerializer.Serialize(new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        });

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            await _gate.WaitAsync(ct);
            try
            {
                EnforceRate();

                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent", content, ct);
                var json = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    var msg = TryExtractError(json) ?? resp.ReasonPhrase;
                    if (resp.StatusCode == (HttpStatusCode)429 && attempt < MaxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                        continue;
                    }
                    throw new Exception($"Gemini {(int)resp.StatusCode}: {msg}");
                }

                if (!TryParseJsonArray(json, batch.Count, out var translations))
                {
                    _log.LogWarning("Gemini returned mismatched count. Falling back per-sentence.");
                    return await FallbackOneByOne(batch, src, tgt, ct);
                }

                var dict = new Dictionary<string, string>(batch.Count);
                for (int i = 0; i < batch.Count; i++)
                {
                    var original = batch[i];
                    var translated = translations[i];

                    if (!string.IsNullOrWhiteSpace(translated) && Normalize(translated) != Normalize(original))
                    {
                        dict[original] = translated;
                        _cache.Set(original, src, tgt, translated);
                    }
                }

                _cache.FlushIfDirty();
                return dict;
            }
            finally
            {
                _gate.Release();
            }
        }

        return await FallbackOneByOne(batch, src, tgt, ct);
    }

    private static string BuildPrompt(IEnumerable<string> texts, string src, string tgt)
    {
        var sb = new StringBuilder();
        sb.AppendLine(src.Equals("auto", StringComparison.OrdinalIgnoreCase)
            ? $"Translate the list below to {tgt}."
            : $"Translate the list below from {src} to {tgt}.");
        sb.AppendLine("Return ONLY a JSON array of the translated sentences in the same order. No keys, no markdown, no extra commentary.");
        sb.AppendLine("Example: [\"Xin chào\", \"Tạm biệt\"]");
        sb.AppendLine("### LIST ###");
        foreach (var t in texts) sb.AppendLine(t);
        sb.AppendLine("### END ###");
        return sb.ToString();
    }

    private static bool TryParseJsonArray(string geminiJson, int expectedCount, out List<string> translations)
    {
        translations = new List<string>();
        using var doc = JsonDocument.Parse(geminiJson);
        var raw = doc.RootElement.GetProperty("candidates")[0]
                             .GetProperty("content").GetProperty("parts")[0]
                             .GetProperty("text").GetString() ?? string.Empty;

        raw = raw.Trim();
        if (raw.Contains("[") && raw.Contains("]"))
        {
            int start = raw.IndexOf('[');
            int end = raw.LastIndexOf(']') + 1;
            raw = raw.Substring(start, end - start).Trim('`', '\n', '\r', ' ');
        }

        try
        {
            var arr = JsonSerializer.Deserialize<List<string>>(raw);
            if (arr != null && arr.Count == expectedCount)
            {
                translations = arr;
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Dictionary<string, string>> FallbackOneByOne(IEnumerable<string> texts, string src, string tgt, CancellationToken ct)
    {
        var dict = new Dictionary<string, string>();
        foreach (var batch in Chunk(texts, 5))
        {
            try
            {
                var single = await CallGeminiAsync(batch, src, tgt, ct);
                foreach (var kv in single)
                    if (Normalize(kv.Value) != Normalize(kv.Key))
                        dict[kv.Key] = kv.Value;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Fallback batch failed: {ex.Message}");
                foreach (var t in batch)
                    dict[t] = t;
            }
        }
        return dict;
    }

    private static string? TryExtractError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("error", out var err) &&
                   err.TryGetProperty("message", out var msg)
                ? msg.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static void EnforceRate()
    {
        lock (_rateLock)
        {
            var now = DateTime.UtcNow;
            var diff = now - _lastCallUtc;
            if (diff < MinCallInterval)
                Thread.Sleep(MinCallInterval - diff);
            _lastCallUtc = DateTime.UtcNow;
        }
    }

    private static IEnumerable<List<T>> Chunk<T>(IEnumerable<T> src, int size)
    {
        var bucket = new List<T>(size);
        foreach (var item in src)
        {
            bucket.Add(item);
            if (bucket.Count == size)
            {
                yield return bucket;
                bucket = new List<T>(size);
            }
        }
        if (bucket.Count > 0) yield return bucket;
    }

    private static string Normalize(string s) =>
        s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormC);

    // Updated TranslationCache with multi-language mapping
    private sealed class TranslationCache
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, Dictionary<string, string>> _bag;
        private bool _dirty;
        private readonly JsonSerializerOptions _opt = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        private readonly ILogger _log;
        public string FilePath { get; }

        public TranslationCache(string file, ILogger log)
        {
            FilePath = file;
            _log = log;
            _bag = Load();
        }

        public bool TryGet(string txt, string src, string tgt, out string? translated)
        {
            var key = Key(src, txt);
            lock (_lock)
            {
                if (_bag.TryGetValue(key, out var map) && map.TryGetValue(tgt, out var v))
                {
                    translated = v;
                    return true;
                }
                translated = null;
                return false;
            }
        }

        public void Set(string txt, string src, string tgt, string trans)
        {
            var keyForward = Key(src, txt);
            var keyReverse = Key(tgt, trans);

            lock (_lock)
            {
                if (Normalize(txt) == Normalize(trans)) return;

                if (!_bag.TryGetValue(keyForward, out var forwardMap))
                    _bag[keyForward] = forwardMap = new();
                forwardMap[tgt] = trans;

                if (!_bag.TryGetValue(keyReverse, out var reverseMap))
                    _bag[keyReverse] = reverseMap = new();
                reverseMap[src] = txt;

                _dirty = true;
            }
        }

        public void FlushIfDirty()
        {
            lock (_lock)
            {
                if (!_dirty) return;
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_bag, _opt));
                _dirty = false;
            }
        }

        private Dictionary<string, Dictionary<string, string>> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json, _opt) ?? new();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to load translation cache. Creating new.");
                return new();
            }
        }

        private static string Key(string lang, string txt) => $"{lang}:{Normalize(txt)}";
    }
}
