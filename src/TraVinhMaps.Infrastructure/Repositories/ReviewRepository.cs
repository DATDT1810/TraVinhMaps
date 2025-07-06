// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using TraVinhMaps.Application.Features.Review.Models;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Infrastructure.CustomRepositories;
using TraVinhMaps.Infrastructure.Db;

namespace TraVinhMaps.Infrastructure.Repositories;
public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(IDbContext dbContext) : base(dbContext) { }
    public async Task<IEnumerable<ReviewResponse>> FilterReviewsAsync(string? destinationId, int? rating, DateTime? startAt, DateTime? endAt, CancellationToken cancellationToken = default)
    {
        var filters = new List<FilterDefinition<Review>>();

        if (!string.IsNullOrEmpty(destinationId))
            filters.Add(Builders<Review>.Filter.Eq(r => r.DestinationId, destinationId));

        if (rating.HasValue)
            filters.Add(Builders<Review>.Filter.Eq(r => r.Rating, rating.Value));

        if (startAt.HasValue && endAt.HasValue)
        {
            var startDate = DateTime.SpecifyKind(startAt.Value.Date, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(endAt.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            filters.Add(Builders<Review>.Filter.Gte(r => r.CreatedAt, startDate));
            filters.Add(Builders<Review>.Filter.Lte(r => r.CreatedAt, endDate));
        }

        else if (startAt.HasValue)
        {
            var startDate = startAt.Value.Date;
            filters.Add(Builders<Review>.Filter.Gte(r => r.CreatedAt, startDate));
        }
        else if (endAt.HasValue)
        {
            var endDate = endAt.Value.Date.AddDays(1).AddTicks(-1);
            filters.Add(Builders<Review>.Filter.Lte(r => r.CreatedAt, endDate));
        }

        var filter = filters.Any() ? Builders<Review>.Filter.And(filters) : Builders<Review>.Filter.Empty;

        var reviews = await _collection.Find(filter).ToListAsync(cancellationToken);

        var responses = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            Rating = r.Rating,
            Images = r.Images,
            Comment = r.Comment,
            UserId = r.UserId,
            DestinationId = r.DestinationId,
            CreatedAt = r.CreatedAt,
            Reply = r.Reply
        }).ToList();

        return responses;
    }

    public async Task<string> AddImageReview(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Review>.Filter.Eq(o => o.Id, id);
        var reviewImage = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (reviewImage == null) return null;
        if (reviewImage.Images == null)
        {
            var newListImage = Builders<Review>.Update.Set(im => im.Images, new List<string>());
            await _collection.UpdateOneAsync(filter, newListImage);
        }
        var updateImage = Builders<Review>.Update.Push(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, updateImage, cancellationToken: cancellationToken);
        return imageUrl;
    }

    public async Task<Reply> AddReply(string id, Reply reply, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Review>.Filter.Eq(r => r.Id, id);
        var review = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (review == null) return null;
        if (review.Reply == null)
        {
            var newReply = Builders<Review>.Update.Set(im => im.Reply, new List<Reply>());
            await _collection.UpdateOneAsync(filter, newReply);
        }
        var updateReply = Builders<Review>.Update.Push(re => re.Reply, reply);
        var updateResult = await _collection.UpdateOneAsync(filter, updateReply, cancellationToken: cancellationToken);
        return reply;
    }

    public async Task<ReviewResponse> GetReviewByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<Review>.Filter.Eq(r => r.Id, id);
        var review = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (review == null)
            return null;

        var response = new ReviewResponse
        {
            Id = review.Id,
            Rating = review.Rating,
            Images = review.Images,
            Comment = review.Comment,
            UserId = review.UserId,
            DestinationId = review.DestinationId,
            CreatedAt = review.CreatedAt,
            Reply = review.Reply
        };

        return response;
    }
}
