// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Application.External;

public interface IEmailSender
{
    Task SendEmailAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default);
}
