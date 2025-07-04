// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Application.Features.Review.Models;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.Review.Interface;
public interface IReviewService
{
    Task<Domain.Entities.Review> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Review>> GetReviewsAsync(int rating, string destinationTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Review>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.Review> AddAsync(CreateReviewRequest createReviewRequest, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Reply> AddReply(string id, CreateReplyRequest createReplyRequest, CancellationToken cancellationToken = default);
    Task<String> AddImageReview(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.Review entity, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.Review, bool>> predicate = null, CancellationToken cancellationToken = default);
}
