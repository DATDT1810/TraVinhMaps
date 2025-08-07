// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TraVinhMaps.Application.Features.Feedback.Models;
public class FeedbackRequest
{
    [Required(ErrorMessage = "Content is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 1000 characters.")]
    public string Content { get; set; } = default!;
    public List<IFormFile>? Images { get; set; }
    //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
