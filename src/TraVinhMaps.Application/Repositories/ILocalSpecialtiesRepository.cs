// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.UnitOfWorks;
public interface ILocalSpecialtiesRepository : IBaseRepository<LocalSpecialties>
{
    Task<string> AddLocalSpeacialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<string> DeleteLocalSpeacialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<bool> RestoreLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default);
    Task<LocalSpecialtyLocation> AddSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default);
    Task<bool> RemoveSellLocationAsync(string id, string sellLocationId, CancellationToken cancellationToken = default);
    Task<LocalSpecialtyLocation> UpdateSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default);
    Task<IEnumerable<LocalSpecialties>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default);


}
