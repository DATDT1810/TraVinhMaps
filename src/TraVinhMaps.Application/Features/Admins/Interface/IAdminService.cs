// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Common.Extensions;
using TraVinhMaps.Application.Features.Admins.Models;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.Admins.Interface;
public interface IAdminService
{
    Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> ListAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);
    Task<User> AddAsync(AdminRequest entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(User entity, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<User, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAdmin(string id, CancellationToken cancellationToken = default);
    Task<bool> RestoreAdmin(string id, CancellationToken cancellationToken = default);

    // step 1  :  fill the otp for this request
    Task<string> RequestOtpForUpdate(string identifier, string authen, RequestOtpUpdateType? requestOtpUpdate, CancellationToken cancellationToken = default);
    Task<bool> ConfirmOtpUpdate(string otp, string context, CancellationToken cancellationToken = default);
    Task<bool> UpdateSetting(UpdateAdminSettingRequest request, string authen, CancellationToken cancellationToken = default);

    Task<bool> UpdatePassword(UpdateAdminPasswordRequest request, string authen, CancellationToken cancellationToken = default);
    Task<string> ResendOtpForUpdate(string identifier, string context, CancellationToken cancellationToken = default);
}
