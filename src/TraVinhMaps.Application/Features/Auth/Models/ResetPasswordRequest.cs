// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Application.Features.Auth.Models;

public class ResetPasswordRequest
{
    public string Identifier { get; set; }
    public string NewPassword { get; set; }
}
