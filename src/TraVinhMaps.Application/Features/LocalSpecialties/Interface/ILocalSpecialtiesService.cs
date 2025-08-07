// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.LocalSpecialties.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.LocalSpecialties.Interface;
public interface ILocalSpecialtiesService
{
    Task<Pagination<Domain.Entities.LocalSpecialties>> GetLocalSpecialtiesPaging(LocalSpecialtiesSpecParams specParams);
    Task<Domain.Entities.LocalSpecialties> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.LocalSpecialties>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.LocalSpecialties>> ListAsync(Expression<Func<Domain.Entities.LocalSpecialties, bool>> predicate, CancellationToken cancellationToken = default);
    Task<Domain.Entities.LocalSpecialties> AddAsync(CreateLocalSpecialtiesRequest entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.LocalSpecialties>> AddRangeAsync(IEnumerable<Domain.Entities.LocalSpecialties> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateLocalSpecialtiesRequest entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.LocalSpecialties entity, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.LocalSpecialties, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<Domain.Entities.LocalSpecialties> GetAsyns(Expression<Func<Domain.Entities.LocalSpecialties, bool>> predicate, CancellationToken cancellationToken = default);
    Task<string> AddLocalSpecialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<string> DeleteLocalSpecialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<bool> RestoreLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.LocalSpecialties>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default);

    // (Sell Location)
    Task<LocalSpecialtyLocation> AddSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default);
    Task<bool> RemoveSellLocationAsync(string id, string sellLocationId, CancellationToken cancellationToken = default);
    Task<LocalSpecialtyLocation> UpdateSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.LocalSpecialties>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
}
