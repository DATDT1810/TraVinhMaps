// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Domain.Specs;
public class OcopProductSpecParams
{
    private const int MaxPageSize = 15;
    public int PageIndex { get; set; } = 1;
    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    public string? TypeId { get; set; }
    public string? Sort { get; set; }
    public string? Search { get; set; }
}
