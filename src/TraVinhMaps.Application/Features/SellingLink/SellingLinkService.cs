// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.SellingLink.Interface;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Application.UnitOfWorks;

namespace TraVinhMaps.Application.Features.SellingLink;
public class SellingLinkService : ISellingLinkService
{
    private readonly ISellingLinkRepository _sellingLinkRepository;
    public SellingLinkService(ISellingLinkRepository sellingLinkRepository)
    {
        _sellingLinkRepository = sellingLinkRepository;
    }

    public Task<Domain.Entities.SellingLink> AddAsync(Domain.Entities.SellingLink entity, CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.AddAsync(entity, cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<Domain.Entities.SellingLink, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.CountAsync(predicate, cancellationToken);
    }

    public Task DeleteAsync(Domain.Entities.SellingLink entity, CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.DeleteAsync(entity, cancellationToken);
    }

    public Task<Domain.Entities.SellingLink> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<IEnumerable<Domain.Entities.SellingLink>> GetSellingLinkByProductId(string productId, CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.GetSellingLinkByProductId(productId, cancellationToken);
    }

    public Task<IEnumerable<Domain.Entities.SellingLink>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.ListAllAsync(cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.SellingLink entity, CancellationToken cancellationToken = default)
    {
        return _sellingLinkRepository.UpdateAsync(entity, cancellationToken);
    }
}
