// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.Company.Models;
public class CreateCompanyRequest
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required List<Location> Locations { get; set; }
    public Contact? Contact { get; set; }
}
