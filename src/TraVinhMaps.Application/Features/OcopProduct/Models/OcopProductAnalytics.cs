// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraVinhMaps.Application.Features.OcopProduct.Models;
public class OcopProductAnalytics
{
    public string Id { get; set; }
    public string ProductName { get; set; }
    public long ViewCount { get; set; }
    public long InteractionCount { get; set; }
    public long FavoriteCount { get; set; }
}
