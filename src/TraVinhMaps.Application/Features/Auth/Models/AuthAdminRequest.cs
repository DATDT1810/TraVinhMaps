// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Application.Features.Auth.Models;

public class AuthAdminRequest
{
    public required string Identifier { get; set; }
    public required string Password { get; set; }
    public required bool RememberMe { get; set; }
}
