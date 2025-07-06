// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.EventAndFestivalFeature.Interface;
public interface IEventAndFestivalService
{
    Task<EventAndFestival> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventAndFestival>> ListAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists the asynchronous with the conditional
    /// </summary>
    /// <param name="predicate">The predicate is the conditional when you list (ex : a => a.id == 1)</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<EventAndFestival>> ListAsync(Expression<Func<EventAndFestival, bool>> predicate, CancellationToken cancellationToken = default);
    Task<EventAndFestival> AddAsync(EventAndFestival entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the range asynchronous with conditional
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<EventAndFestival>> AddRangeAsync(IEnumerable<EventAndFestival> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(EventAndFestival entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(EventAndFestival entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Counts the asynchronous.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<long> CountAsync(Expression<Func<EventAndFestival, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<EventAndFestival> GetAsyns(Expression<Func<EventAndFestival, bool>> predicate, CancellationToken cancellationToken = default);
    Task<string> AddEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<string> DeleteEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventAndFestival>> GetTopUpcomingEvents(CancellationToken cancellationToken = default);
}
