// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TraVinhMaps.Application.Common.Extensions;
public enum RequestOtpUpdateType
{
    ChangeCurrentIdentifier, // Yêu cầu OTP để được đổi identifier hiện tại
    UpdateToNewIdentifier    // Yêu cầu OTP để cập nhật identifier mới
}
