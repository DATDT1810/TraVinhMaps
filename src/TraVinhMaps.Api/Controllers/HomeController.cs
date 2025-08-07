// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.Company.Interface;
using TraVinhMaps.Application.Features.Destination.Interface;
using TraVinhMaps.Application.Features.Destination.Models;
using TraVinhMaps.Application.Features.EventAndFestivalFeature.Interface;
using TraVinhMaps.Application.Features.OcopProduct.Interface;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly ITouristDestinationService _touristDestinationService;
    private readonly IEventAndFestivalService _eventAndFestivalService;
    private readonly IOcopProductService _ocopProductService;
    private readonly ICompanyService _companyService;
    private readonly ICacheService _cacheService;

    public HomeController(
        ITouristDestinationService touristDestinationService,
        IEventAndFestivalService eventAndFestivalService,
        IOcopProductService ocopProductService,
        ICompanyService companyService,
        ICacheService cacheService
    )
    {
        _touristDestinationService = touristDestinationService;
        _eventAndFestivalService = eventAndFestivalService;
        _ocopProductService = ocopProductService;
        _companyService = companyService;
        _cacheService = cacheService;
    }

    [HttpGet]
    [Route("GetDataHomePage")]
    public async Task<IActionResult> GetDataHomePage()
    {
        var cacheKeyFavoriteDestination = "HomePageData_favorite_destination";
        var cacheKeyEvents = "HomePageData_top_events";
        var cacheKeyOcopProducts = "HomePageData_ocop_products";

        // check cache firsts
        var cachedDataTopFavorite = await _cacheService.GetData<List<TopFavoriteRequest>>(cacheKeyFavoriteDestination);
        var cachedDataTopEvents = await _cacheService.GetData<IEnumerable<EventAndFestival>>(cacheKeyEvents);
        var cachedDataOcopProducts = await _cacheService.GetData<List<OcopProductResponse>>(cacheKeyOcopProducts);
        if (cachedDataTopFavorite != null && cachedDataTopEvents != null && cachedDataOcopProducts != null)
        {
            var resultCacheData = new
            {
                FavoriteDestinations = cachedDataTopFavorite,
                TopEvents = cachedDataTopEvents,
                OcopProducts = cachedDataOcopProducts
            };
            return this.ApiOk(resultCacheData);
        }
        // Gọi song song cho nhanh (tối ưu), hoặc tuần tự nếu cần
        var favoriteDestinationsTask = _touristDestinationService.GetTop10FavoriteDestination();
        var topEventsTask = _eventAndFestivalService.GetTopUpcomingEvents();
        var ocopProductsTask = _ocopProductService.GetCurrentOcopProduct();

        await Task.WhenAll(favoriteDestinationsTask, topEventsTask, ocopProductsTask);
        var ocopProducts = await ocopProductsTask;

        var companies = await _companyService.ListAllAsync();
        var companyMap = companies.ToDictionary(c => c.Id, c => c);
        var ocopResponse = ocopProducts.Select(product =>
        {
            companyMap.TryGetValue(product.CompanyId, out var company);

            return new OcopProductResponse
            {
                Id = product.Id,
                CreatedAt = product.CreatedAt,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductImage = product.ProductImage,
                ProductPrice = product.ProductPrice,
                OcopTypeId = product.OcopTypeId,
                Status = product.Status,
                UpdateAt = product.UpdateAt,
                Sellocations = product.Sellocations,
                CompanyId = product.CompanyId,
                OcopPoint = product.OcopPoint,
                OcopYearRelease = product.OcopYearRelease,
                TagId = product.TagId,
                company = company != null
                    ? new CompanyDto
                    {
                        Id = company.Id,
                        Name = company.Name
                    }
                    : new CompanyDto
                    {
                        Id = string.Empty,
                        Name = "Unknown"
                    }
            };
        }).ToList();

        var cacheTtl = TimeSpan.FromMinutes(5); // Set a reasonable cache TTL

        await _cacheService.SetData(cacheKeyFavoriteDestination, favoriteDestinationsTask.Result, cacheTtl);
        await _cacheService.SetData(cacheKeyEvents, topEventsTask.Result, cacheTtl);
        await _cacheService.SetData(cacheKeyOcopProducts, ocopResponse, cacheTtl);

        var result = new
        {
            FavoriteDestinations = favoriteDestinationsTask.Result,
            TopEvents = topEventsTask.Result,
            OcopProducts = ocopResponse
        };

        return this.ApiOk(result);
    }
}

