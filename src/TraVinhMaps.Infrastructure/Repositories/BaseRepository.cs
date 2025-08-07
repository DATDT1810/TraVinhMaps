// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using MongoDB.Driver;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Infrastructure.Db;

namespace TraVinhMaps.Infrastructure.CustomRepositories;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    private readonly IDbContext _context;
    protected readonly IMongoCollection<T> _collection;

    public BaseRepository(IDbContext context)
    {
        _context = context;
        _collection = _context.GetCollection<T>();
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, null, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _collection.InsertManyAsync(entities, null, cancellationToken);
        return entities;
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty, null, cancellationToken)
            : await _collection.CountDocumentsAsync(predicate, null, cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
        //var filter = Builders<T>.Filter.Eq("_id", entity.Id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, id);
        //var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(FilterDefinition<T>.Empty).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(predicate).ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
        // var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
        //var filter = Builders<T>.Filter.Eq("_id", entity.Id); // Hoặc e => e.Id nếu dùng biểu thức

        var updateDefinition = BuildUpdateDefinition(entity);

        await _collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
    }

    public async Task<T> GetAsyns(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    private UpdateDefinition<T> BuildUpdateDefinition(T entity)
    {
        var updates = new List<UpdateDefinition<T>>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.Name == "Id" || prop.Name == "_id") continue; // Bỏ qua _id vì không update được

            var value = prop.GetValue(entity);

            // Optional: Bỏ qua null nếu không muốn update null
            if (value != null)
            {
                updates.Add(Builders<T>.Update.Set(prop.Name, value));
            }
        }

        return updates.Count > 0
            ? Builders<T>.Update.Combine(updates)
            : null;
    }

    public async Task<IEnumerable<T>> ListAsync(FilterDefinition<T> filter, SortDefinition<T>? sort = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = _collection.Find(filter);
        if (sort != null)
            query = query.Sort(sort);

        if (limit.HasValue)
            query = query.Limit(limit.Value);

        return await query.ToListAsync(cancellationToken);
    }
}
