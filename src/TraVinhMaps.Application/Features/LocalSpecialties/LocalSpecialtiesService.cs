// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.LocalSpecialties.Interface;
using TraVinhMaps.Application.Features.LocalSpecialties.Mapper;
using TraVinhMaps.Application.Features.LocalSpecialties.Models;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.LocalSpecialties;
public class LocalSpecialtiesService : ILocalSpecialtiesService
{
    private readonly ILocalSpecialtiesRepository _localSpecialtiesRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMarkerRepository _markerRepository;

    public LocalSpecialtiesService(ILocalSpecialtiesRepository localSpecialtiesRepository, ITagRepository tagRepository, IMarkerRepository markerRepository)
    {
        _localSpecialtiesRepository = localSpecialtiesRepository;
        _tagRepository = tagRepository;
        _markerRepository = markerRepository;
    }

    public async Task<Domain.Entities.LocalSpecialties> AddAsync(CreateLocalSpecialtiesRequest entity, CancellationToken cancellationToken = default)
    {
        var tagId = await _tagRepository.GetTagIdByNameAsync("Local specialty", cancellationToken);
        if (string.IsNullOrEmpty(tagId))
        {
            throw new InvalidOperationException("Required tag 'Local Specialty' not found.");
        }
        var request = LocalSpecialtiesMapper.Mapper.Map<CreateLocalSpecialtiesRequest, Domain.Entities.LocalSpecialties>(entity);
        request.TagId = tagId;
        request.Status = true;
        return await _localSpecialtiesRepository.AddAsync(request, cancellationToken);
    }

    public async Task<string> AddLocalSpecialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.AddLocalSpeacialtiesImage(id, imageUrl, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.LocalSpecialties>> AddRangeAsync(IEnumerable<Domain.Entities.LocalSpecialties> entities, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<LocalSpecialtyLocation> AddSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default)
    {
        var sellLocationMaker = await _markerRepository.GetAsyns(m => m.Name == "Sell Location", cancellationToken);
        if (sellLocationMaker == null)
        {
            throw new InvalidOperationException("Marker 'Sell Location' not found.");
        }
        request.MarkerId = sellLocationMaker.Id;
        return await _localSpecialtiesRepository.AddSellLocationAsync(id, request, cancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<Domain.Entities.LocalSpecialties, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.CountAsync(predicate, cancellationToken);
    }

    public async Task DeleteAsync(Domain.Entities.LocalSpecialties entity, CancellationToken cancellationToken = default)
    {
        entity.Status = false;
        await _localSpecialtiesRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.DeleteLocalSpecialtiesAsync(id, cancellationToken);
    }

    public async Task<string> DeleteLocalSpecialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.DeleteLocalSpeacialtiesImage(id, imageUrl, cancellationToken);
    }

    public Task<Domain.Entities.LocalSpecialties> GetAsyns(Expression<Func<Domain.Entities.LocalSpecialties, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _localSpecialtiesRepository.GetAsyns(predicate, cancellationToken);
    }

    public async Task<Domain.Entities.LocalSpecialties> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.LocalSpecialties>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.GetDestinationsByIds(idList, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.LocalSpecialties>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.ListAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.LocalSpecialties>> ListAsync(Expression<Func<Domain.Entities.LocalSpecialties, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.ListAsync(predicate, cancellationToken);
    }

    public async Task<bool> RemoveSellLocationAsync(string id, string sellLocationId, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.RemoveSellLocationAsync(id, sellLocationId, cancellationToken);
    }

    public async Task<bool> RestoreLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.RestoreLocalSpecialtiesAsync(id, cancellationToken);
    }

    public async Task UpdateAsync(UpdateLocalSpecialtiesRequest entity, CancellationToken cancellationToken = default)
    {
        var existingSpecialty = await _localSpecialtiesRepository.GetByIdAsync(entity.Id, cancellationToken);
        if (existingSpecialty == null)
        {
            throw new KeyNotFoundException("Local specialty not found.");
        }

        var originalCreatedAt = existingSpecialty.CreatedAt;

        // Map the request to the entity
        var updatedSpecialty = LocalSpecialtiesMapper.Mapper.Map<UpdateLocalSpecialtiesRequest, Domain.Entities.LocalSpecialties>(entity);

        // Restore the original CreatedAt
        updatedSpecialty.CreatedAt = originalCreatedAt;

        // Ensure UpdateAt is set to the current time
        updatedSpecialty.UpdateAt = DateTime.UtcNow;

        // Update the entity in the repository
        await _localSpecialtiesRepository.UpdateAsync(updatedSpecialty, cancellationToken);
    }

    public async Task<LocalSpecialtyLocation> UpdateSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default)
    {
        // fetch marker "Sell Location"
        var sellLocationMarker = await _markerRepository.GetAsyns(m => m.Name == "Sell Location", cancellationToken);
        if (sellLocationMarker == null)
        {
            throw new InvalidOperationException("Marker 'Sell Location' not found.");
        }

        request.MarkerId = sellLocationMarker.Id;
        return await _localSpecialtiesRepository.UpdateSellLocationAsync(id, request, cancellationToken);
    }



    public async Task<Pagination<Domain.Entities.LocalSpecialties>> GetLocalSpecialtiesPaging(LocalSpecialtiesSpecParams specParams)
    {
        var result = await _localSpecialtiesRepository.GetLocalSpecialtiesPaging(specParams);
        return new Pagination<Domain.Entities.LocalSpecialties>(specParams.PageIndex, specParams.PageSize, (int)result.Count, result.Data);
    }

    public async Task<IEnumerable<Domain.Entities.LocalSpecialties>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _localSpecialtiesRepository.ListAsync(l => l.FoodName.ToLower().Contains(name.ToLower()), cancellationToken);
    }
}
