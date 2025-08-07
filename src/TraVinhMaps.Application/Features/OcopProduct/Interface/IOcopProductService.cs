// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualBasic;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.OcopProduct.Interface;
public interface IOcopProductService
{
    Task<Domain.Entities.OcopProduct> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.OcopProduct> AddAsync(Domain.Entities.OcopProduct entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.OcopProduct entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteOcopProductAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> RestoreOcopProductAsync(string id, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.OcopProduct, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<Domain.Entities.OcopProduct> GetOcopProductByName(string name, CancellationToken cancellationToken = default);
    Task<Domain.Entities.SellLocation> GetSellLocationByName(string id, string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductByOcopTypeId(string ocopTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductByCompanyId(string companyId, CancellationToken cancellationToken = default);
    Task<String> AddImageOcopProduct(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<SellLocation> AddSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default);
    Task<bool> UpdateSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default);
    Task<bool> DeleteSellLocation(string ocopProductId, string sellLocationName, CancellationToken cancellationToken = default);
    Task<ProductLookUpsResponse> LooksUpForProduct();
    // Analytics
    Task<IEnumerable<OcopProductAnalytics>> GetProductAnalyticsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OcopProductUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Interacted OCOP Products
    Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByInteractionsAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Wishlisted OCOP Products
    Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByFavoritesAsync(int top =5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // OCOP Product Comparison
    Task<IEnumerable<OcopProductAnalytics>> CompareProductsAsync(IEnumerable<string> productIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductsByIds(List<string> idList, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopProduct>> GetCurrentOcopProduct(CancellationToken cancellationToken = default);
    Task<Pagination<Domain.Entities.OcopProduct>> GetOcopProductPaging(OcopProductSpecParams specParams, CancellationToken cancellationToken = default);
}
