// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoDB.Driver;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;
using TraVinhMaps.Infrastructure.CustomRepositories;
using TraVinhMaps.Infrastructure.Db;
namespace TraVinhMaps.Infrastructure.Repositories;
public class LocalSpecialtiesRepository : BaseRepository<LocalSpecialties>, ILocalSpecialtiesRepository
{
    public LocalSpecialtiesRepository(IDbContext context) : base(context)
    {
    }

    public async Task<string> AddLocalSpeacialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);

        var localSpecialties = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (localSpecialties == null) return null;
        if (localSpecialties.Images == null)
        {
            var setImagesUpdate = Builders<LocalSpecialties>.Update.Set(p => p.Images, new List<String>());
            await _collection.UpdateOneAsync(filter, setImagesUpdate);
        }
        var pushImageUpdate = Builders<LocalSpecialties>.Update.Push(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pushImageUpdate, cancellationToken: cancellationToken);
        return imageUrl;
    }

    public async Task<LocalSpecialtyLocation> AddSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);

        // Ensure the location list exists
        var localSpecialties = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (cancellationToken == null)
            return null;

        if (localSpecialties.Locations == null)
        {
            var setLocationsUpdate = Builders<LocalSpecialties>.Update.Set(p => p.Locations, new List<LocalSpecialtyLocation>());
            await _collection.UpdateOneAsync(filter, setLocationsUpdate, cancellationToken: cancellationToken);
        }

        var pushLocationUpdate = Builders<LocalSpecialties>.Update.Push(p => p.Locations, request);
        await _collection.UpdateOneAsync(filter, pushLocationUpdate, cancellationToken: cancellationToken);

        return request;
    }

    public async Task<string> DeleteLocalSpeacialtiesImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);

        var localSpecialties = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (localSpecialties == null || localSpecialties.Images == null || !localSpecialties.Images.Contains(imageUrl))
        {
            return null;
        }

        var pullImageUpdate = Builders<LocalSpecialties>.Update.Pull(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pullImageUpdate, cancellationToken: cancellationToken);
        return "Image deleted successfully";
    }

    public async Task<bool> DeleteLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);
        var update = Builders<LocalSpecialties>.Update.Set(p => p.Status, false);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<LocalSpecialties>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default)
    {
        if (idList == null || idList.Count == 0)
            return Enumerable.Empty<LocalSpecialties>();

        var filter = Builders<LocalSpecialties>.Filter.In(d => d.Id, idList);
        var localSpecialties = await _collection.Find(filter).ToListAsync(cancellationToken);
        return localSpecialties;
    }

    public async Task<Pagination<LocalSpecialties>> GetLocalSpecialtiesPaging(LocalSpecialtiesSpecParams specParams)
    {
        var filterBuilder = Builders<LocalSpecialties>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(specParams.Search))
        {
            filter &= filterBuilder.Regex(x => x.FoodName, new MongoDB.Bson.BsonRegularExpression(specParams.Search, "i"));
        }

        var query = _collection.Find(filter);

        if (!string.IsNullOrEmpty(specParams.Sort))
        {
            var sortBuilder = Builders<LocalSpecialties>.Sort;
            switch (specParams.Sort)
            {
                case "nameAsc":
                    query = query.Sort(sortBuilder.Ascending(p => p.CreatedAt));
                    break;
                case "nameDesc":
                    query = query.Sort(sortBuilder.Descending(p => p.CreatedAt));
                    break;
                default:
                    query = query.Sort(sortBuilder.Ascending(n => n.CreatedAt));
                    break;
            }
        }

        var count = await query.CountDocumentsAsync();
        var localSpecialties = await query
            .Skip(specParams.PageSize * (specParams.PageIndex - 1))
            .Limit(specParams.PageSize)
            .ToListAsync();

        return new Pagination<LocalSpecialties>(specParams.PageIndex, specParams.PageSize, (int)count, localSpecialties);
    }

    public async Task<bool> RemoveSellLocationAsync(string id, string locationId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);
        var update = Builders<LocalSpecialties>.Update.PullFilter(
    p => p.Locations,
    loc => loc.LocationId == locationId
);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RestoreLocalSpecialtiesAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);
        var update = Builders<LocalSpecialties>.Update.Set(p => p.Status, true);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<LocalSpecialtyLocation> UpdateSellLocationAsync(string id, LocalSpecialtyLocation request, CancellationToken cancellationToken = default)
    {
        var filter = Builders<LocalSpecialties>.Filter.Eq(p => p.Id, id);

        var specialty = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (specialty == null || specialty.Locations == null)
            return null;

        var existingLocation = specialty.Locations.FirstOrDefault(l => l.LocationId == request.LocationId);
        if (existingLocation == null)
            return null;

        var pullUpdate = Builders<LocalSpecialties>.Update.PullFilter(p => p.Locations, l => l.LocationId == request.LocationId);
        await _collection.UpdateOneAsync(filter, pullUpdate, cancellationToken: cancellationToken);

        var pushUpdate = Builders<LocalSpecialties>.Update.Push(p => p.Locations, request);
        await _collection.UpdateOneAsync(filter, pushUpdate, cancellationToken: cancellationToken);

        return request;
    }

}
