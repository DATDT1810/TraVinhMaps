// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraVinhMaps.Application.Features.InteractionLogs.Models;
public class UpdateInteractionLogsRequest
{
    public required string Id { get; set; }
    public required string ItemId { get; set; }
    public string? ItemType { get; set; }
    public int? Duration { get; set; }
}
