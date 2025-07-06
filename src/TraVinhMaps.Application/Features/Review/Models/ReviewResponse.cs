// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.Review.Models;
public class ReviewResponse
{
    public string Id { get; set; }
    public int Rating { get; set; }
    public List<string>? Images { get; set; }
    public string? Comment { get; set; }
    public string UserId { get; set; }
    public string? UserName { get; set; }
    public string? Avatar { get; set; }
    public string DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Reply>? Reply { get; set; }
}
