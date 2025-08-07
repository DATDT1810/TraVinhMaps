// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Api.Extensions;

public static class GetPartitionKeyExtension
{

    public static string GetPartitionKey(HttpContext context)
    {
        // Priority order:
        // 1. Authenticated user
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            return $"user:{context.User.Identity.Name}";
        }
        // 2. API Key from header
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return $"api:{apiKey.FirstOrDefault()}";
        }

        // 3. Client IP
        var clientIP = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(clientIP))
        {
            return $"ip:{clientIP}";
        }

        // 4. Fallback
        return "anonymous";
    }
}
