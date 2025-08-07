// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using MongoDB.Driver;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.UnitOfWorks;

/// <summary>
///   <br The base of Repository Interface /> 
/// </summary>
/// <typeparam name="T" is the interface class></typeparam>
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ListAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists the asynchronous with the conditional
    /// </summary>
    /// <param name="predicate">The predicate is the conditional when you list (ex : a => a.id == 1)</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the range asynchronous with conditional
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Counts the asynchronous.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<long> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<T> GetAsyns(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> ListAsync(
   FilterDefinition<T> filter,
   SortDefinition<T>? sort = null,
   int? limit = null,
   CancellationToken cancellationToken = default);
}
