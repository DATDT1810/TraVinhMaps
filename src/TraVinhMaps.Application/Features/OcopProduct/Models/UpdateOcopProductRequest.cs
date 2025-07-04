// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.OcopProduct.Models;
public class UpdateOcopProductRequest
{
    public required string Id { get; set; }
    public required string ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public string? ProductPrice { get; set; }
    public required string OcopTypeId { get; set; }
    public DateTime? UpdateAt { get; set; } = DateTime.UtcNow;
    public required string CompanyId { get; set; }
    public required int OcopPoint { get; set; }
    public required int OcopYearRelease { get; set; }
    public required string TagId { get; set; }
}
