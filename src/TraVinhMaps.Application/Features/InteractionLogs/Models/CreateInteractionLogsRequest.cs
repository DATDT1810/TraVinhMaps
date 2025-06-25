// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace TraVinhMaps.Application.Features.InteractionLogs.Models;
public class CreateInteractionLogsRequest
{
    public required string UserId { get; set; }
    public required string ItemId { get; set; }
    public string? ItemType { get; set; }
    public int? Duration { get; set; }
}
