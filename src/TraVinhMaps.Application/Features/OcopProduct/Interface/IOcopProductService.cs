// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.OcopProduct.Interface;
public interface IOcopProductService
{
    Task<Domain.Entities.OcopProduct> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.OcopProduct> AddAsync(Domain.Entities.OcopProduct entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.OcopProduct entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteOcopProductAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> RestoreOcopProductAsync(string id, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.OcopProduct, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductByOcopTypeId(string ocopTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductByCompanyId(string companyId, CancellationToken cancellationToken = default);
    Task<String> AddImageOcopProduct(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<SellLocation> AddSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default);
    Task<bool> UpdateSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default);
    Task<bool> DeleteSellLocation(string ocopProductId, string sellLocationName, CancellationToken cancellationToken = default);
    Task<ProductLookUpsResponse> LooksUpForProduct();
}
