// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.OcopProduct.Interface;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;
using TraVinhMaps.Application.UnitOfWorks;

namespace TraVinhMaps.Application.Features.OcopProduct;
public class OcopProductService : IOcopProductService
{
    private readonly IOcopProductRepository _ocopProductRepository;
    private readonly IBaseRepository<Domain.Entities.Company> _companyRepository;
    private readonly IBaseRepository<Domain.Entities.OcopType> _ocopTypeRepository;
    private readonly IBaseRepository<Domain.Entities.Tags> _tagRepository;

    public OcopProductService(IOcopProductRepository ocopProductRepository, IBaseRepository<Domain.Entities.Company> companyRepository,
        IBaseRepository<Domain.Entities.OcopType> ocopTypeRepository,
        IBaseRepository<Domain.Entities.Tags> tagRepository)
    {
        _ocopProductRepository = ocopProductRepository;
        _companyRepository = companyRepository;
        _ocopTypeRepository = ocopTypeRepository;
        _tagRepository = tagRepository;
    }
    public Task<Domain.Entities.OcopProduct> AddAsync(Domain.Entities.OcopProduct entity, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.AddAsync(entity, cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<Domain.Entities.OcopProduct, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.CountAsync(predicate, cancellationToken);
    }
    public Task<bool> DeleteOcopProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.DeleteOcopProductAsync(id, cancellationToken);
    }
    public Task<bool> RestoreOcopProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.RestoreOcopProductAsync(id, cancellationToken);
    }
    public Task<Domain.Entities.OcopProduct> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<Domain.Entities.OcopProduct> GetOcopProductByName(string name, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.GetOcopProductByName(name, cancellationToken);
    }
    public Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductByCompanyId(string companyId, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.GetOcopProductByCompanyId(companyId, cancellationToken);
    }

    public Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductByOcopTypeId(string ocopTypeId, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.GetOcopProductByOcopTypeId(ocopTypeId, cancellationToken);
    }

    public Task<IEnumerable<Domain.Entities.OcopProduct>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.ListAllAsync(cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.OcopProduct entity, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.UpdateAsync(entity, cancellationToken);
    }

    public Task<String> AddImageOcopProduct(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.AddImageOcopProduct(id, imageUrl, cancellationToken);
    }

    public Task<SellLocation> AddSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.AddSellLocation(id, sellLocation, cancellationToken);
    }

    public Task<bool> DeleteSellLocation(string ocopProductId, string sellLocationName, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.DeleteSellLocation(ocopProductId, sellLocationName, cancellationToken);
    }

    public Task<bool> UpdateSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.UpdateSellLocation(id, sellLocation, cancellationToken);
    }

    public async Task<ProductLookUpsResponse> LooksUpForProduct()
    {
        var ocopTypes = await _ocopTypeRepository.ListAllAsync();
        var companies = await _companyRepository.ListAllAsync();
        var tags = await _tagRepository.GetAsyns(a => a.Name == "Ocop");

        // Fetching data for lookups
        var productLookUps = new ProductLookUpsResponse
        {
            Companies = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,

            }).ToList(),
            OcopTypes = ocopTypes.Select(o => new OcopTypeDto
            {
                Id = o.Id,
                Name = o.OcopTypeName,
            }).ToList(),
            Tags = new TagDto
            {
                Id = tags.Id,
                Name = tags.Name
            }
        };

        return productLookUps;
    }

    public async Task<IEnumerable<OcopProductAnalytics>> GetProductAnalyticsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range.Use: day, week, month, year.");

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date.");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future.");
        }

        var analytics = await _ocopProductRepository.GetProductAnalyticsAsync(timeRange, startDate, endDate, cancellationToken);
        // Chỉ trả về sản phẩm có ít nhất một chỉ số > 0
        return analytics.Where(a => a.ViewCount > 0 || a.InteractionCount > 0 || a.FavoriteCount > 0);
        //return analytics.ToList();
    }

    public async Task<IEnumerable<OcopProductUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range. Use: day, week, month, year.");

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date.");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future.");
        }

        return await _ocopProductRepository.GetUserDemographicsAsync(timeRange, startDate, endDate, cancellationToken);
    }

    // GetTopProductsByInteractionsAsync
    public async Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByInteractionsAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range.Use: day, week, month, year.");

        return await _ocopProductRepository.GetTopProductsByInteractionsAsync(top, timeRange, startDate, endDate, cancellationToken);
    }

    // GetTopProductsByFavoritesAsync
    public async Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range.Use: day, week, month, year.");

        return await _ocopProductRepository.GetTopProductsByFavoritesAsync(top, timeRange, startDate, endDate, cancellationToken);
    }

    // CompareProductsAsync
    public async Task<IEnumerable<OcopProductAnalytics>> CompareProductsAsync(IEnumerable<string> productIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null,  CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range.Use: day, week, month, year.");

        return await _ocopProductRepository.CompareProductsAsync(productIds, timeRange, startDate, endDate, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.OcopProduct>> GetOcopProductsByIds(List<string> idList, CancellationToken cancellationToken = default)
    {
        return await _ocopProductRepository.GetOcopProductsByIds(idList, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.OcopProduct>> GetCurrentOcopProduct(CancellationToken cancellationToken = default)
    {
        var ocops = await _ocopProductRepository.ListAllAsync(cancellationToken);
        return ocops
        .OrderByDescending(p => p.OcopYearRelease)
        .Take(10);

    }

    public Task<SellLocation> GetSellLocationByName(string id, string name, CancellationToken cancellationToken = default)
    {
        return _ocopProductRepository.GetSellLocationByName(id, name, cancellationToken);
    }
    public async Task<IEnumerable<Domain.Entities.OcopProduct>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        var ocops = await _ocopProductRepository.ListAllAsync(cancellationToken);
        return ocops.Where(p => p.Status == true);
    }

    public async Task<Pagination<Domain.Entities.OcopProduct>> GetOcopProductPaging(OcopProductSpecParams specParams, CancellationToken cancellationToken = default)
    {
        return await _ocopProductRepository.GetOcopProductPaging(specParams, cancellationToken);
    }
}
