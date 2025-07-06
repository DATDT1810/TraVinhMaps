// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.OcopProduct.Models;
public class OcopProductResponse
{
    public required string Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public List<string>? ProductImage { get; set; }
    public string? ProductPrice { get; set; }
    public required string OcopTypeId { get; set; }
    public required bool Status { get; set; }
    public DateTime? UpdateAt { get; set; }
    public List<SellLocation>? Sellocations { get; set; }
    public required string CompanyId { get; set; }
    public required int OcopPoint { get; set; }
    public required int OcopYearRelease { get; set; }
    public required string TagId { get; set; }
    public required CompanyDto company { get; set; }
    public OcopTypeDto? ocopType { get; set; }
}
