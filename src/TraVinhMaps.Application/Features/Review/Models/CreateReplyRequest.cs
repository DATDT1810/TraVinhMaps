// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson.Serialization.Attributes;

namespace TraVinhMaps.Application.Features.Review.Models;
public class CreateReplyRequest
{
    public string Id { get; set; }
    public string? Content { get; set; }
    public List<IFormFile>? Images { get; set; }
}
