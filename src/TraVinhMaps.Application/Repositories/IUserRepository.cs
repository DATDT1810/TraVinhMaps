// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq.Expressions;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.UnitOfWorks;

public interface IUserRepository : IBaseRepository<User>
{
    Task<long> CountAsync(Expression<Func<User, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteUser(string id, CancellationToken cancellationToken = default);
    Task<bool> RestoreUser(string id, CancellationToken cancellationToken = default);
    Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetUserStatisticsAsync(string groupBy, string timeRange, CancellationToken cancellationToken = default);
    // chart performance
    Task<Dictionary<string, Dictionary<string, int>>> GetPerformanceByTagAsync(IEnumerable<string>? tagNames, bool includeOcop, bool includeDestination, bool includeLocalSpecialty, bool includeTips, bool includeFestivals, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<List<Favorite>> getFavoriteUserList(string id, CancellationToken cancellationToken = default);
    Task<bool> addItemToFavoriteList(string id, Favorite favorite, CancellationToken cancellationToken = default);
    Task<bool> removeItemToFavoriteList(string id, string itemId, CancellationToken cancellationToken = default);
}
