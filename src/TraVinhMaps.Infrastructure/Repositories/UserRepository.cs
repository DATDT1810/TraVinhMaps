using MongoDB.Bson;
using MongoDB.Driver;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Infrastructure.Db;
using Microsoft.Extensions.Logging;
using DnsClient.Internal;
using TraVinhMaps.Application.Common.Exceptions;

namespace TraVinhMaps.Infrastructure.CustomRepositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbContext context, ILogger<UserRepository> logger) : base(context)
    {
        _database = context.Database;
        _logger = logger;
    }

    public async Task<bool> addItemToFavoriteList(string id, Favorite favorite, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var user = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with id {id} not found.");
        }

        if (user.Favorites == null)
        {
            var setfavoriteUpdate = Builders<User>.Update.Set(p => p.Favorites, new List<Favorite>());
            await _collection.UpdateOneAsync(filter, setfavoriteUpdate);
        }
        var pushFavoriteUpdate = Builders<User>.Update.Push(p => p.Favorites, favorite);
        var updateResult = await _collection.UpdateOneAsync(filter, pushFavoriteUpdate, cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUser(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var update = Builders<User>.Update
            .Set(u => u.IsForbidden, true)
            .Set(u => u.Status, false)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetPerformanceByTagAsync(
    IEnumerable<string>? tagNames,
    bool includeOcop,
    bool includeDestination,
    bool includeLocalSpecialty,
    bool includeTips,
    bool includeFestivals,
    string timeRange = "month",
    DateTime? startDate = null,
    DateTime? endDate = null,
    CancellationToken cancellationToken = default)
    {
        try
        {
            startDate ??= DateTime.UtcNow.AddHours(7).AddDays(-30).Date;
            endDate ??= DateTime.UtcNow.AddHours(7).Date.AddDays(1);

            var tagCol = _database.GetCollection<Tags>("Tags");
            var tagFilter = tagNames == null || !tagNames.Any()
                ? FilterDefinition<Tags>.Empty
                : Builders<Tags>.Filter.In(t => t.Name, tagNames);
            var tagDocs = await tagCol.Find(tagFilter).ToListAsync(cancellationToken);

            var tagIdToName = tagDocs.ToDictionary(
                t => ObjectId.Parse(t.Id),
                t => t.Name);

            if (!tagDocs.Any() && tagNames != null && tagNames.Any())
            {
                tagIdToName = tagNames.ToDictionary(t => ObjectId.GenerateNewId(), t => t);
                _logger.LogWarning("No tags found in the database, using provided tagNames.");
            }
            else if (!tagDocs.Any())
            {
                _logger.LogWarning("No tags found, returning empty result.");
                return new Dictionary<string, Dictionary<string, int>>();
            }

            _logger.LogDebug("⮞ Processing {Count} tag(s)", tagIdToName.Count);

            var config = new[]
            {
            (includeOcop, "OcopProduct"),
            (includeDestination, "TouristDestination"),
            (includeLocalSpecialty, "LocalSpecialties"),
            (includeTips, "Tips"),
            (includeFestivals, "EventAndFestival")
        };

            var itemMap = new Dictionary<ObjectId, string>();

            foreach (var (isOn, colName) in config)
            {
                if (!isOn) continue;

                var col = _database.GetCollection<BsonDocument>(colName);
                var tagIdArray = new BsonArray(tagIdToName.Keys.Select(id => (BsonValue)id));
                var tagIdStrArray = new BsonArray(tagIdToName.Keys.Select(id => (BsonValue)id.ToString()));

                var filter = Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.In("tagId", tagIdArray),
                    Builders<BsonDocument>.Filter.In("tagId", tagIdStrArray),
                    Builders<BsonDocument>.Filter.AnyIn("tags", tagIdArray),
                    Builders<BsonDocument>.Filter.AnyIn("tags", tagIdStrArray)
                );

                var logDocs = await col.Find(filter)
                    .Project(Builders<BsonDocument>.Projection.Include("_id").Include("tagId").Include("tags"))
                    .ToListAsync(cancellationToken);

                _logger.LogDebug("▶ {Collection}: {Count} items matched", colName, logDocs.Count);

                foreach (var doc in logDocs)
                {
                    var itemId = doc["_id"].AsObjectId;
                    ObjectId? matchedTagId = null;

                    if (doc.TryGetValue("tagId", out var tIdVal))
                    {
                        if (tIdVal.IsObjectId && tagIdToName.ContainsKey(tIdVal.AsObjectId))
                            matchedTagId = tIdVal.AsObjectId;
                        else if (tIdVal.IsString && ObjectId.TryParse(tIdVal.AsString, out var oid) && tagIdToName.ContainsKey(oid))
                            matchedTagId = oid;
                    }
                    else if (doc.TryGetValue("tags", out var tagsVal) && tagsVal.IsBsonArray)
                    {
                        foreach (var tagVal in tagsVal.AsBsonArray)
                        {
                            if (tagVal.IsObjectId && tagIdToName.ContainsKey(tagVal.AsObjectId))
                            {
                                matchedTagId = tagVal.AsObjectId;
                                break;
                            }
                            if (tagVal.IsString && ObjectId.TryParse(tagVal.AsString, out var oid) && tagIdToName.ContainsKey(oid))
                            {
                                matchedTagId = oid;
                                break;
                            }
                        }
                    }

                    if (matchedTagId.HasValue)
                    {
                        var tagName = tagIdToName[matchedTagId.Value];
                        itemMap[itemId] = tagName;
                    }
                }
            }

            var logs = _database.GetCollection<BsonDocument>("InteractionLogs");

            var match = new BsonDocument("$match", new BsonDocument
        {
            { "itemId", new BsonDocument("$in", new BsonArray(itemMap.Keys)) },
            { "createdAt", new BsonDocument {
                { "$gte", new BsonDateTime(startDate.Value) },
                { "$lte", new BsonDateTime(endDate.Value) }
            }}
        });

            string format = timeRange.ToLower() switch
            {
                "week" => "%G-%V",
                "month" => "%Y-%m",
                "year" => "%Y",
                _ => "%Y-%m-%d"
            };

            var project = new BsonDocument("$project", new BsonDocument
        {
            { "itemId", 1 },
            { "period", new BsonDocument("$dateToString", new BsonDocument
                {
                    { "format", format },
                    { "date", "$createdAt" },
                    { "timezone", "Asia/Ho_Chi_Minh" }
                })
            }
        });

            var docs = await logs.Aggregate<BsonDocument>(new[] { match, project }).ToListAsync(cancellationToken);

            _logger.LogDebug("⮞ InteractionLogs matched: {Count}", docs.Count);

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var doc in docs)
            {
                var itemId = doc["itemId"].AsObjectId;
                var period = doc["period"].AsString;

                if (!itemMap.TryGetValue(itemId, out var tagName)) continue;

                if (!result.ContainsKey(tagName)) result[tagName] = new();
                if (!result[tagName].ContainsKey(period)) result[tagName][period] = 0;

                result[tagName][period] += 1;
            }

            var targetTags = tagNames != null && tagNames.Any() ? tagNames : tagIdToName.Values;

            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(startDate.Value, tz);
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endDate.Value, tz);

            var expectedPeriods = new List<string>();

            switch (timeRange.ToLower())
            {
                case "week":
                    for (var dt = localStart.Date; dt <= localEnd.Date; dt = dt.AddDays(7))
                    {
                        var isoWeek = System.Globalization.ISOWeek.GetWeekOfYear(dt);
                        var year = System.Globalization.ISOWeek.GetYear(dt);
                        expectedPeriods.Add($"{year}-{isoWeek:D2}");
                    }
                    break;
                case "month":
                    for (var dt = new DateTime(localStart.Year, localStart.Month, 1); dt <= localEnd; dt = dt.AddMonths(1))
                    {
                        expectedPeriods.Add(dt.ToString("yyyy-MM"));
                    }
                    break;
                case "year":
                    for (var y = localStart.Year; y <= localEnd.Year; y++)
                    {
                        expectedPeriods.Add(y.ToString());
                    }
                    break;
                case "day":
                default:
                    expectedPeriods.Add(localStart.ToString("yyyy-MM-dd"));
                    break;
            }

            foreach (var tag in targetTags)
            {
                if (!result.ContainsKey(tag)) result[tag] = new();
                foreach (var period in expectedPeriods)
                {
                    if (!result[tag].ContainsKey(period)) result[tag][period] = 0;
                }
            }

            _logger.LogInformation("GetPerformanceByTagsAsync completed – returning {TagCount} tags", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPerformanceByTagsAsync failed");
            throw;
        }
    }

    public async Task<List<Favorite>> getFavoriteUserList(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var user = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with id {id} not found.");
        }
        return user.Favorites ?? new List<Favorite>();
    }

    public Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Dictionary<string, object>> GetUserStatisticsAsync(string groupBy, string timeRange, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, object>();

        try
        {
            _logger.LogInformation("▶️ Start GetUserStatisticsAsync - groupBy={GroupBy}, timeRange={TimeRange}", groupBy, timeRange);

            var validGroupBy = new[] { "all", "age", "hometown", "gender", "status", "time" };
            if (!validGroupBy.Contains(groupBy?.ToLower()))
            {
                _logger.LogWarning("Invalid groupBy: {GroupBy}", groupBy);
                return result;
            }

            var now = DateTime.UtcNow;
            DateTime startDate, endDate;

            switch (timeRange?.ToLowerInvariant())
            {
                case "day":
                    startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddDays(1);
                    break;
                case "week":
                    int diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    startDate = now.Date.AddDays(-diff);
                    endDate = startDate.AddDays(7);
                    break;
                case "month":
                    startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddMonths(1);
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddYears(1);
                    break;
                default:
                    _logger.LogWarning("❌ Invalid timeRange: {TimeRange}", timeRange);
                    return result;
            }

            _logger.LogInformation("Time range (UTC): {Start} - {End}", startDate, endDate);

            var userRoleId = "68357026034a7216407e93d1";
            var baseMatch = new BsonDocument
        {
            { "roleId", new ObjectId(userRoleId) },
            { "username", new BsonDocument("$ne", BsonNull.Value) },
            { "createdAt", new BsonDocument
                {
                    { "$gte", new BsonDateTime(startDate) },
                    { "$lt", new BsonDateTime(endDate) }
                }
            }
        };

            var matchedCount = await _collection.CountDocumentsAsync(baseMatch, cancellationToken: cancellationToken);
            if (matchedCount == 0)
            {
                _logger.LogWarning("No users matched the filter.");
                return result;
            }

            if (groupBy == "all")
                result["total"] = matchedCount;

            if (groupBy == "all" || groupBy == "age")
            {
                var nowVN = now.AddHours(7);
                var ageGroups = new[] { 0, 18, 30, 50, 120 };
                var ageLabels = new[] { "0-18", "18-30", "30-50", "50+" };
                var ageStats = new Dictionary<string, int>();

                for (int i = 0; i < ageLabels.Length; i++)
                {
                    var minAge = ageGroups[i];
                    var maxAge = ageGroups[i + 1];
                    var latestDob = nowVN.AddYears(-minAge);
                    var earliestDob = nowVN.AddYears(-maxAge);

                    var pipeline = new List<BsonDocument>
                {
                    new BsonDocument("$match", baseMatch),
                    new BsonDocument("$addFields", new BsonDocument("parsedDob",
                        new BsonDocument("$convert", new BsonDocument
                        {
                            { "input", "$profile.dateOfBirth" },
                            { "to", "date" },
                            { "onError", BsonNull.Value },
                            { "onNull", BsonNull.Value }
                        })
                    )),
                    new BsonDocument("$match", new BsonDocument("parsedDob", new BsonDocument
                    {
                        { "$ne", BsonNull.Value },
                        { "$gte", new BsonDateTime(earliestDob) },
                        { "$lt", new BsonDateTime(latestDob) }
                    })),
                    new BsonDocument("$count", "count")
                };

                    var docs = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
                    ageStats[ageLabels[i]] = docs.FirstOrDefault()?["count"]?.AsInt32 ?? 0;
                }

                var unknownDobFilter = baseMatch.DeepClone().AsBsonDocument;
                unknownDobFilter.Add("$or", new BsonArray
            {
                new BsonDocument("profile.dateOfBirth", new BsonDocument("$exists", false)),
                new BsonDocument("profile.dateOfBirth", BsonNull.Value)
            });
                ageStats["Unknown"] = (int)await _collection.CountDocumentsAsync(unknownDobFilter, cancellationToken: cancellationToken);

                result["age"] = ageStats;
            }

            if (groupBy == "all" || groupBy == "gender")
            {
                var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", baseMatch),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$ifNull", new BsonArray { "$profile.gender", "Unknown" }) },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

                var docs = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
                result["gender"] = docs.ToDictionary(x => x["_id"].AsString, x => x["count"].AsInt32);
            }

            if (groupBy == "all" || groupBy == "status")
            {
                var active = baseMatch.DeepClone().AsBsonDocument;
                active.Add("status", true);
                active.Add("isForbidden", false);

                var inactive = baseMatch.DeepClone().AsBsonDocument;
                inactive.Add("status", false);
                inactive.Add("isForbidden", false);

                var forbidden = baseMatch.DeepClone().AsBsonDocument;
                forbidden.Add("isForbidden", true);

                result["status"] = new Dictionary<string, long>
            {
                { "Active", await _collection.CountDocumentsAsync(active, cancellationToken:cancellationToken) },
                { "Inactive", await _collection.CountDocumentsAsync(inactive, cancellationToken:cancellationToken) },
                { "Forbidden", await _collection.CountDocumentsAsync(forbidden, cancellationToken:cancellationToken) }
            };
            }

            if (groupBy == "all" || groupBy == "hometown")
            {
                // Sửa lỗi bằng cách thay thế hàm Javascript bằng logic đơn giản và đáng tin cậy hơn.
                var pipeline = new List<BsonDocument>
                {
                    new BsonDocument("$match", baseMatch),
                    new BsonDocument("$addFields", new BsonDocument("normalizedAddress",
                        new BsonDocument("$function", new BsonDocument
                        {
                            { "body", @"
                                function(addr) {
                                    // 1. Luôn kiểm tra đầu vào để đảm bảo an toàn
                                    if (!addr || typeof addr !== 'string' || addr.trim() === '') {
                                        return 'Unknown';
                                    }
                        
                                    // 2. Tách địa chỉ bằng dấu phẩy, cắt bỏ khoảng trắng thừa và loại bỏ các phần tử rỗng
                                    const parts = addr.split(',').map(p => p.trim()).filter(p => p);

                                    // 3. Áp dụng quy tắc xử lý:
                                    if (parts.length >= 2) {
                                        // Nếu có 2 phần trở lên, lấy 2 phần cuối cùng.
                                        // Đây là trường hợp phổ biến nhất (Quận/Huyện, Tỉnh/Thành phố).
                                        // Ví dụ: 'Q. Ô Môn, TP. Cần Thơ'
                                        return `${parts[parts.length - 2]}, ${parts[parts.length - 1]}`;
                                    } 
                        
                                    if (parts.length === 1) {
                                        // Nếu chỉ có 1 phần, trả về chính nó (thường là chỉ có Tỉnh/Thành phố).
                                        // Ví dụ: 'Cần Thơ'
                                        return parts[0];
                                    } 
                        
                                    // Nếu địa chỉ rỗng hoặc chỉ chứa dấu phẩy, trả về 'Unknown'.
                                    return 'Unknown';
                                }
                            " },
                            { "args", new BsonArray { "$profile.address" } },
                            { "lang", "js" }
                        })
                    )),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", "$normalizedAddress" },
                        { "count", new BsonDocument("$sum", 1) }
                    }),
                    new BsonDocument("$sort", new BsonDocument("count", -1))
                };

                var docs = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);

                // Chuyển đổi kết quả sang Dictionary, đồng thời xử lý các kết quả 'Unknown' từ pipeline
                var hometownStats = docs.ToDictionary(x => x["_id"].AsString, x => x["count"].AsInt32);

                result["hometown"] = hometownStats;
            }

            if (groupBy == "all" || groupBy == "time")
            {
                var dateFormat = timeRange switch
                {
                    "day" => "%Y-%m-%d %H:00",
                    "week" or "month" => "%Y-%m-%d",
                    "year" => "%Y-%m",
                    _ => "%Y-%m-%d"
                };

                var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", baseMatch),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", dateFormat },
                            { "date", "$createdAt" },
                            { "timezone", "Asia/Ho_Chi_Minh" }
                        }) },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };

                var docs = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
                result["time"] = docs.ToDictionary(x => x["_id"].AsString, x => x["count"].AsInt32);
            }

            _logger.LogInformation("Done. Statistics keys: {Keys}", string.Join(", ", result.Keys));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserStatisticsAsync: groupBy={GroupBy}, timeRange={TimeRange}", groupBy, timeRange);
            return result;
        }
    }

    public async Task<bool> removeItemToFavoriteList(string id, string itemId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var user = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with id {id} not found.");
        }
        var update = Builders<User>.Update.PullFilter(u => u.Favorites,
        Builders<Favorite>.Filter.Eq(f => f.ItemId, itemId));

        var updateResult = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> RestoreUser(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var update = Builders<User>.Update
            .Set(u => u.IsForbidden, false)
            .Set(u => u.Status, true)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

}
