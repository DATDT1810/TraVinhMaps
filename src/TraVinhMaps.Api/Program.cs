// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.RateLimiting;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using TraVinhMaps.Api.AuthenticationHandlers;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Api.Middlewares;
using TraVinhMaps.Application;
using TraVinhMaps.Application.External.Models;
using TraVinhMaps.Infrastructure;
using TraVinhMaps.Infrastructure.Db.Data;

var root = Directory.GetCurrentDirectory();
var dotnetEnv = Path.Combine(root, ".env");

if (File.Exists(dotnetEnv))
{
    DotNetEnv.Env.Load(dotnetEnv);
}
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables();

builder.Services.Configure<MongoDbSetting>(options =>
{
    builder.Configuration.GetSection("MongoDb").Bind(options);

    var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    var envDatabaseName = Environment.GetEnvironmentVariable("DbName");

    if (!string.IsNullOrEmpty(envConnectionString))
    {
        options.ConnectionString = envConnectionString;
    }

    if (!string.IsNullOrEmpty(envDatabaseName))
    {
        options.DatabaseName = envDatabaseName;
    }

    Console.WriteLine(options.DatabaseName + "/n" + options.ConnectionString);
});

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5280")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// layer di
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

// Register custom authorization handler
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddleware>();

builder.Services.AddAuthentication("SessionAuth")
    .AddScheme<SessionAuthenticationSchemeOptions, SessionAuthenticationHandler>("SessionAuth", options =>
    {
        options.ValidateExpiration = true;
        //  options.HeaderName = "X-Session-Token";
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//config Cloudinary
builder.Services.Configure<CloudinarySetting>(options =>
{
    builder.Configuration.GetSection("CloudinarySettings").Bind(options);

    var envCloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
    var envApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
    var envApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

    if (!string.IsNullOrEmpty(envCloudName))
    {
        options.CloudName = envCloudName;
    }

    if (!string.IsNullOrEmpty(envApiKey))
    {
        options.ApiKey = envApiKey;
    }

    if (!string.IsNullOrEmpty(envApiSecret))
    {
        options.ApiSecret = envApiSecret;
    }
});

// Email configuration
builder.Services.Configure<EmailConfiguration>(options =>
{
    builder.Configuration.GetSection("EMAIL_CONFIGURATION").Bind(options);

    var envHost = Environment.GetEnvironmentVariable("EMAIL_HOST");
    var envPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    var envPort = Environment.GetEnvironmentVariable("EMAIL_PORT");
    var envEmail = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");

    if (!string.IsNullOrEmpty(envHost))
    {
        options.Host = envHost;
    }

    if (!string.IsNullOrEmpty(envPassword))
    {
        options.Password = envPassword;
    }

    if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out int port))
    {
        options.Port = port;
    }

    if (!string.IsNullOrEmpty(envEmail))
    {
        options.Email = envEmail;
    }
});


// SpeedSms configuration
builder.Services.Configure<SpeedSmsSetting>(options =>
{
    builder.Configuration.GetSection("SpeedSmsSettings").Bind(options);

    var envAccessToken = Environment.GetEnvironmentVariable("SPEEDSMS_ACCESS_TOKEN");
    var envDeviceId = Environment.GetEnvironmentVariable("SPEEDSMS_DEVICE_ID");
    var envBaseUrl = Environment.GetEnvironmentVariable("SPEEDSMS_BASE_URL");

    if (!string.IsNullOrEmpty(envAccessToken))
    {
        options.AccessToken = envAccessToken;
    }

    if (!string.IsNullOrEmpty(envDeviceId))
    {
        options.DeviceId = envDeviceId;
    }

    if (!string.IsNullOrEmpty(envBaseUrl))
    {
        options.BaseUrl = envBaseUrl;
    }
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var connectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    var instanceName = builder.Configuration.GetValue<string>("Redis:InstanceName");

    var envConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

    if (!string.IsNullOrEmpty(envConnectionString))
    {
        connectionString = envConnectionString;
    }
    options.Configuration = connectionString;
});

// config for the maximum request body size for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
});

// config for push notifications using Firebase
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile("travinhgo-ba688-firebase-adminsdk-fbsvc-5ffd0fa4a9.json"),
    });
}



// rate limiter configuration
builder.Services.AddRateLimiter(rateLimiterOption =>
{
    rateLimiterOption.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetPartitionKeyExtension.GetPartitionKey(context),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10  // Queue 10 requests khi limit exceeded
            }));

    // Custom rejection handler
    rateLimiterOption.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded",
            message = $"Too many requests for partition: {GetPartitionKeyExtension.GetPartitionKey(context.HttpContext)}",
            retryAfter = 60
        }, cancellationToken: token);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Register global exception handling middleware first
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (builder.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var serviceScope = scope.ServiceProvider;
        await DataSeeding.SeedData(serviceScope);
    }
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
// Enable CORS for all origins
app.UseCors("AllowAll");
app.UseAuthentication();
// Add our custom authentication response handler
app.UseMiddleware<CustomAuthenticationMiddleware>();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
// SignalR
app.MapHub<DashboardHub>("/dashboardHub")
   .RequireCors("AllowSpecificOrigin");

app.Run();
