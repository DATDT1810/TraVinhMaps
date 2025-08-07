// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Repositories;
public interface IOcopProductRepository : IBaseRepository<OcopProduct>
{
    Task<OcopProduct> GetOcopProductByName(string name, CancellationToken cancellationToken = default);
    Task<Domain.Entities.SellLocation> GetSellLocationByName(string id, string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<OcopProduct>> GetOcopProductByCompanyId(string companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OcopProduct>> GetOcopProductByOcopTypeId(string ocopTypeId, CancellationToken cancellationToken = default);
    Task<SellLocation> AddSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default);
    Task<bool> UpdateSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default);
    Task<bool> DeleteSellLocation(string ocopProductId, string sellLocationName, CancellationToken cancellationToken = default);
    Task<bool> DeleteOcopProductAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> RestoreOcopProductAsync(string id, CancellationToken cancellationToken = default);
    Task<String> AddImageOcopProduct(string id, string imageUrl, CancellationToken cancellationToken = default);

    // Analytics
    Task<IEnumerable<OcopProductAnalytics>> GetProductAnalyticsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OcopProductUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Interacted OCOP Products
    Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByInteractionsAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Wishlisted OCOP Products
    Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // OCOP Product Comparison
    Task<IEnumerable<OcopProductAnalytics>> CompareProductsAsync(IEnumerable<string> productIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OcopProduct>> GetOcopProductsByIds(List<string> idList, CancellationToken cancellationToken = default);
    Task<Pagination<OcopProduct>> GetOcopProductPaging(OcopProductSpecParams specParams, CancellationToken cancellationToken = default);
}
