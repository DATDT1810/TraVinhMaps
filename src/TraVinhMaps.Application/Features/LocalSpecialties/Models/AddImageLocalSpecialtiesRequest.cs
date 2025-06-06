// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TraVinhMaps.Application.Features.LocalSpecialties.Models;
public class AddImageLocalSpecialtiesRequest
{
    public string Id { get; set; }
    public List<IFormFile> ImageFile { get; set; }
}
