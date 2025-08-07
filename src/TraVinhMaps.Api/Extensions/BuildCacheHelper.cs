// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TraVinhMaps.Application.Common.Dtos;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Api.Extensions;

public static class BuildCacheHelper
{
    public static string BuildCacheKey(TouristDestinationSpecParams parameters)
    {
        var keyParts = new List<string>
    {
        CacheKey.Destination_Paging,
        $"page:{parameters.PageIndex}",
        $"size:{parameters.PageSize}"
    };
        if (!string.IsNullOrEmpty(parameters.Sort))
        {
            keyParts.Add($"sort:{parameters.Sort}");
        }
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            keyParts.Add($"search:{parameters.Search}");
        }
        if (parameters.PageIndex <= 0 || parameters.PageSize <= 0)
        {
            throw new ArgumentException("PageIndex and PageSize must be greater than zero.");
        }
        // Ensure that the key parts are sorted to maintain consistency
        return string.Join(":", keyParts);
    }
    public static TimeSpan GetCacheTtl(int pageIndex)
    {
        return pageIndex switch
        {
            1 => TimeSpan.FromMinutes(10),      // First page - cache longer
            <= 3 => TimeSpan.FromMinutes(7),   // First 3 pages - medium cache
            <= 10 => TimeSpan.FromMinutes(5),  // Pages 4-10 - standard cache
            _ => TimeSpan.FromMinutes(3)       // Higher pages - shorter cache
        };
    }

    public static string BuildCacheKeyForViewport(double north, double south, double east, double west, double? zoomLevel, string? locationTypeId, string? tagId, int limit)
    {
        var keyParts = new List<string>
        {
            CacheKey.Destination_Viewport,
            $"north:{north}",
            $"south:{south}",
            $"east:{east}",
            $"west:{west}",
            $"zoomLevel:{zoomLevel}",
            $"locationTypeId:{locationTypeId}",
            $"tagId:{tagId}",
            $"limit:{limit}"
        };
        return string.Join(":", keyParts);
    }

    public static string BuildCacheKeyForEventAndFestival(EventAndFestivalSpecParams parameters)
    {
        var keyParts = new List<string>
        {
            CacheKey.EventAndFestival_Paging,
            $"page:{parameters.PageIndex}",
            $"size:{parameters.PageSize}"
        };
        if (!string.IsNullOrEmpty(parameters.Sort))
        {
            keyParts.Add($"sort:{parameters.Sort}");
        }
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            keyParts.Add($"search:{parameters.Search}");
        }
        if (parameters.PageIndex <= 0 || parameters.PageSize <= 0)
        {
            throw new ArgumentException("PageIndex and PageSize must be greater than zero.");
        }
        // Ensure that the key parts are sorted to maintain consistency
        return string.Join(":", keyParts);
    }
    public static string BuildCacheKeyForOcopProduct(OcopProductSpecParams parameters)
    {
        var keyParts = new List<string>
        {
            CacheKey.OcopProduct_Paging,
            $"page:{parameters.PageIndex}",
            $"size:{parameters.PageSize}"
        };
        if (!string.IsNullOrEmpty(parameters.Sort))
        {
            keyParts.Add($"sort:{parameters.Sort}");
        }
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            keyParts.Add($"search:{parameters.Search}");
        }
        if (parameters.PageIndex <= 0 || parameters.PageSize <= 0)
        {
            throw new ArgumentException("PageIndex and PageSize must be greater than zero.");
        }
        // Ensure that the key parts are sorted to maintain consistency
        return string.Join(":", keyParts);
    }

    public static string BuildCacheKeyForLocalSpecialties(LocalSpecialtiesSpecParams parameters)
    {
        var keyParts = new List<string>
        {
            CacheKey.LocalSpecialties_Paging,
            $"page:{parameters.PageIndex}",
            $"size:{parameters.PageSize}"
        };
        if (!string.IsNullOrEmpty(parameters.Sort))
        {
            keyParts.Add($"sort:{parameters.Sort}");
        }
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            keyParts.Add($"search:{parameters.Search}");
        }
        if (parameters.PageIndex <= 0 || parameters.PageSize <= 0)
        {
            throw new ArgumentException("PageIndex and PageSize must be greater than zero.");
        }
        // Ensure that the key parts are sorted to maintain consistency
        return string.Join(":", keyParts);
    }
}
