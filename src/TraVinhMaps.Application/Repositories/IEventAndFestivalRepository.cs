// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Repositories;
public interface IEventAndFestivalRepository : IBaseRepository<EventAndFestival>
{
    Task<string> AddEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<string> DeleteEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<Pagination<EventAndFestival>> GetEventAndFestivalPaging(EventAndFestivalSpecParams specParams, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventAndFestival>> SearchEventAndFestivalByNameAsync(string name, CancellationToken cancellationToken = default);
}
