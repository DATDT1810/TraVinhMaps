// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.Users.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.Users.Interface;
public interface IUserService
{
    Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> ListAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists the asynchronous with the conditional
    /// </summary>
    /// <param name="predicate">The predicate is the conditional when you list (ex : a => a.id == 1)</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<User>> ListAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the range asynchronous with conditional
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(User entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(User entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Counts the asynchronous.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<long> CountAsync(Expression<Func<User, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteUser(string id, CancellationToken cancellationToken = default);
    Task<bool> RestoreUser(string id, CancellationToken cancellationToken = default);
    Task<Pagination<User>> GetUsersAsync(UserSpecParams userSpecParams, CancellationToken cancellationToken = default);
    Task<AdminProfileResponse> GetProfileAdmin(string id, CancellationToken cancellationToken = default);
    Task UpdateProfileAdmin(UpdateProfileAdminRequest request, CancellationToken cancellationToken = default);

    // chart statistics
    Task<Dictionary<string, object>> GetUserStatisticsAsync(string groupBy, string timeRange, CancellationToken cancellationToken = default);
    // chart performance
    Task<Dictionary<string, Dictionary<string, int>>> GetPerformanceByTagAsync(IEnumerable<string>? tagNames, bool includeOcop, bool includeDestination, bool includeLocalSpecialty, bool includeTips, bool includeFestivals, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> GetUserProfile(string id, CancellationToken cancellationToken = default);

    Task<List<Favorite>> getFavoriteUserList(CancellationToken cancellationToken = default);
    Task<bool> addItemToFavoriteList(FavoriteRequest favoriteRequest, CancellationToken cancellationToken = default);
    Task<bool> removeItemToFavoriteList(string itemId, CancellationToken cancellationToken = default);
}
