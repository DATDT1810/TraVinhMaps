// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson.Serialization.Attributes;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.Review.Models;
public class CreateReviewRequest
{
    public int Rating { get; set; }
    public List<IFormFile>? Images { get; set; }
    public string? Comment { get; set; }
    public required string DestinationId { get; set; }
}
