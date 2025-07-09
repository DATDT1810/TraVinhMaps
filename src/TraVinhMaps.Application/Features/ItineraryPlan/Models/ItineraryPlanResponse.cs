// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.ItineraryPlan.Models;
public class ItineraryPlanResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Duration { get; set; }
    public List<string>? Locations { get; set; }
    public string? EstimatedCost { get; set; }
    public List<TouristDestination> touristDestinations { get; set; }
}
