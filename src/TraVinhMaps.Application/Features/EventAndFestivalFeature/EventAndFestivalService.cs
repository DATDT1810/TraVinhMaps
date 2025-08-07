// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Application.Features.EventAndFestivalFeature.Interface;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.EventAndFestivalFeature;
public class EventAndFestivalService : IEventAndFestivalService
{
    private readonly IEventAndFestivalRepository _repository;

    public EventAndFestivalService(IEventAndFestivalRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventAndFestival> AddAsync(EventAndFestival entity, CancellationToken cancellationToken = default)
    {
        return await _repository.AddAsync(entity, cancellationToken);
    }

    public async Task<string> AddEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _repository.AddEventAndFestivalImage(id, imageUrl, cancellationToken);
    }

    public async Task<IEnumerable<EventAndFestival>> AddRangeAsync(IEnumerable<EventAndFestival> entities, CancellationToken cancellationToken = default)
    {
        return await _repository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<EventAndFestival, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return await _repository.CountAsync(predicate, cancellationToken);
    }

    public Task DeleteAsync(EventAndFestival entity, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<string> DeleteEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteEventAndFestivalImage(id, imageUrl, cancellationToken);
    }

    public async Task<EventAndFestival> GetAsyns(Expression<Func<EventAndFestival, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _repository.GetAsyns(predicate, cancellationToken);
    }

    public async Task<EventAndFestival> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<EventAndFestival>> GetTopUpcomingEvents(CancellationToken cancellationToken = default)
    {
        var allEvents = await _repository.ListAllAsync(cancellationToken);
        return allEvents
       .OrderByDescending(e => e.StartDate)
       .Take(3);
    }

    public async Task<IEnumerable<EventAndFestival>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.ListAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventAndFestival>> ListAsync(Expression<Func<EventAndFestival, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _repository.ListAsync(predicate, cancellationToken);
    }

    public Task UpdateAsync(EventAndFestival entity, CancellationToken cancellationToken = default)
    {
        return  _repository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<Pagination<EventAndFestival>> GetEventAndFestivalPaging(EventAndFestivalSpecParams specParams, CancellationToken cancellationToken = default)
    {
        return await _repository.GetEventAndFestivalPaging(specParams, cancellationToken);
    }
}
