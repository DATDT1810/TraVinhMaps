// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoDB.Bson;
using MongoDB.Driver;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;
using TraVinhMaps.Infrastructure.CustomRepositories;
using TraVinhMaps.Infrastructure.Db;

namespace TraVinhMaps.Infrastructure.Repositories;
public class EventAndFestivalRepository : BaseRepository<EventAndFestival>, IEventAndFestivalRepository
{
    public EventAndFestivalRepository(IDbContext context) : base(context)
    {
    }

    public async Task<string> AddEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EventAndFestival>.Filter.Eq(p => p.Id, id);

        var eventAndFestinal = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (eventAndFestinal == null) return null;
        if (eventAndFestinal.Images == null)
        {
            var setImagesUpdate = Builders<EventAndFestival>.Update.Set(p => p.Images, new List<string>());
            await _collection.UpdateOneAsync(filter, setImagesUpdate);
        }
        var pushImageUpdate = Builders<EventAndFestival>.Update.Push(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pushImageUpdate, cancellationToken: cancellationToken);
        return imageUrl;
    }

    public async Task<string> DeleteEventAndFestivalImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EventAndFestival>.Filter.Eq(p => p.Id, id);

        var eventAndFestinal = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (eventAndFestinal == null || eventAndFestinal.Images == null || !eventAndFestinal.Images.Contains(imageUrl))
        {
            return null;
        }

        var pullImageUpdate = Builders<EventAndFestival>.Update.Pull(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pullImageUpdate, cancellationToken: cancellationToken);
        return "Image deleted successfully";
    }

    public async Task<Pagination<EventAndFestival>> GetEventAndFestivalPaging(EventAndFestivalSpecParams specParams, CancellationToken cancellationToken = default)
    {
        var builder = Builders<EventAndFestival>.Filter;
        var filter = builder.Eq(x => x.Status, true) & builder.Gte(x => x.EndDate, DateTime.Now);
        if (!string.IsNullOrEmpty(specParams.Search))
        {
            var searchFilter = builder.Regex(x => x.NameEvent, new BsonRegularExpression(specParams.Search, "i"));
            filter &= searchFilter;
        }

        if (!string.IsNullOrEmpty(specParams.Sort))
        {
            return new Pagination<EventAndFestival>
            {
                PageSize = specParams.PageSize,
                PageIndex = specParams.PageIndex,
                Data = await DataFilter(specParams, filter, cancellationToken),
                Count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken)
            };
        }
        return new Pagination<EventAndFestival>
        {
            PageSize = specParams.PageSize,
            PageIndex = specParams.PageIndex,
            Data = await _collection
                    .Find(filter)
                    .Sort(Builders<EventAndFestival>.Sort.Descending("CreatedAt"))
                    .Skip(specParams.PageSize * (specParams.PageIndex - 1))
                    .Limit(specParams.PageSize)
                    .ToListAsync(cancellationToken),
            Count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken)
        };
    }

    private async Task<IReadOnlyList<EventAndFestival>> DataFilter(EventAndFestivalSpecParams specParams, FilterDefinition<EventAndFestival> filter, CancellationToken cancellationToken = default)
    {
        switch (specParams.Sort)
        {
            case "nameAsc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<EventAndFestival>.Sort.Ascending("NameEvent"))
                    .Skip(specParams.PageSize * (specParams.PageIndex - 1))
                    .Limit(specParams.PageSize)
                    .ToListAsync(cancellationToken);
            case "nameDesc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<EventAndFestival>.Sort.Descending("NameEvent"))
                    .Skip(specParams.PageSize * (specParams.PageIndex - 1))
                    .Limit(specParams.PageSize)
                    .ToListAsync(cancellationToken);
            case "dateAsc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<EventAndFestival>.Sort.Ascending("CreatedAt"))
                    .Skip(specParams.PageSize * (specParams.PageIndex - 1))
                    .Limit(specParams.PageSize)
                    .ToListAsync(cancellationToken);
            case "dateDesc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<EventAndFestival>.Sort.Descending("CreatedAt"))
                    .Skip(specParams.PageSize * (specParams.PageIndex - 1))
                    .Limit(specParams.PageSize)
                    .ToListAsync(cancellationToken);
            default:
                return await _collection
                    .Find(filter)
                    .Sort(Builders<EventAndFestival>.Sort.Descending("CreatedAt"))
                    .Skip(specParams.PageSize * (specParams.PageIndex - 1))
                    .Limit(specParams.PageSize)
                    .ToListAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<EventAndFestival>> SearchEventAndFestivalByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var builder = Builders<EventAndFestival>.Filter;
        var filter = builder.And(
            builder.Regex(x => x.NameEvent, new BsonRegularExpression(name, "i")),
            builder.Gte(x => x.EndDate, DateTime.Now),
            builder.Eq(x => x.Status, true)
        );
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
}
