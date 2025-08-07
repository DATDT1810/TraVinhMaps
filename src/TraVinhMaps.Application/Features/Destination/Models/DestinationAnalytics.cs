// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Application.Features.Destination.Models;
public class DestinationAnalytics
{
    public string Id { get; set; }
    public string LocationName { get; set; }
    public long ViewCount { get; set; }
    public long InteractionCount { get; set; }
    public long FavoriteCount { get; set; }
}
