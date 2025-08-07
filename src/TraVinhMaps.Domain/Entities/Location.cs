// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoDB.Bson.Serialization.Attributes;

namespace TraVinhMaps.Domain.Entities;
/// <summary>
/// Location document object
/// </summary>
public class Location
{
    [BsonElement("type")]
    public string? Type { get; set; }
    [BsonElement("coordinates")]
    public List<double>? Coordinates { get; set; }

    [BsonIgnore]
    public double Longitude => Coordinates?.Count >= 2 ? Coordinates[0] : 0;

    [BsonIgnore]
    public double Latitude => Coordinates?.Count >= 2 ? Coordinates[1] : 0;
}
