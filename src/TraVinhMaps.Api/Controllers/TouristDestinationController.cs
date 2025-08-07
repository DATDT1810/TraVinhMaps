// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Application.Common.Dtos;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.Destination;
using TraVinhMaps.Application.Features.Destination.Interface;
using TraVinhMaps.Application.Features.Destination.Mappers;
using TraVinhMaps.Application.Features.Destination.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
//[EnableRateLimiting("DefaultPolicy")]
public class TouristDestinationController : ControllerBase
{
    private readonly ITouristDestinationService _touristDestinationService;
    private readonly ImageManagementDestinationServices _imageManagementDestinationServices;
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ICacheService _cacheService;
    private ILogger<TouristDestinationController> _logger;

    public TouristDestinationController(ITouristDestinationService touristDestinationService, ImageManagementDestinationServices imageManagementDestinationServices, IHubContext<DashboardHub> hubContext, ICacheService cacheService, ILogger<TouristDestinationController> logger)
    {
        _touristDestinationService = touristDestinationService;
        _imageManagementDestinationServices = imageManagementDestinationServices;
        _hubContext = hubContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    [Route("GetAllDestinations")]
    public async Task<IActionResult> GetAllDestinations()
    {
        var list = await this._touristDestinationService.ListAllAsync();
        return this.ApiOk(list);
    }

    [HttpGet]
    [Route("GetTouristDestinationPaging")]
    public async Task<IActionResult> GetTouristDestinationPaging([FromQuery] TouristDestinationSpecParams touristDestinationSpecParams)
    {
        var cacheKey = BuildCacheHelper.BuildCacheKey(touristDestinationSpecParams);
        var cacheResult = await _cacheService.GetData<Pagination<TouristDestination>>(cacheKey);
        if (cacheResult != null)
        {
            Response.Headers.Add("X-Cache", "HIT");
            Response.Headers.Add("X-Cache-Key", cacheKey);
            _logger.LogDebug("Cache hit for destinations: {CacheKey}", cacheKey);

            return this.ApiOk(cacheResult);
        }
        Response.Headers.Add("X-Cache", "MISS");
        _logger.LogDebug("Cache miss for destinations: {CacheKey}", cacheKey);

        var list = await _touristDestinationService.GetTouristDestination(touristDestinationSpecParams);
        if (list == null)
        {
            return this.ApiError("No tourist destinations found.");
        }
        // Cache the result for 5 minutes
        var cacheTtl = BuildCacheHelper.GetCacheTtl(touristDestinationSpecParams.PageIndex);
        await _cacheService.SetData(cacheKey, list, cacheTtl);

        _logger.LogInformation("Destinations cached: {CacheKey}, TTL: {TTL}, Count: {Count}",
        cacheKey, cacheTtl, list.Data.Count);
        return this.ApiOk(list);
    }

    [HttpGet]
    [Route("GetDeletedDestination")]
    public async Task<IActionResult> GetDeletedDestination()
    {
        var list = await this._touristDestinationService.ListAsync(p => p.status == false);
        return this.ApiOk(list);
    }

    [HttpGet]
    [Route("GetActiveDestination")]
    public async Task<IActionResult> GetActiveDestination()
    {
        var list = await this._touristDestinationService.ListAsync(p => p.status == true);
        return this.ApiOk(list);
    }

    [HttpGet]
    [Route("GetCountDestination")]
    public async Task<IActionResult> GetCountDestination()
    {
        var count = await this._touristDestinationService.CountAsync();
        return this.ApiOk(count);
    }

    [HttpGet]
    [Route("[action]/{id}", Name = "GetDestinationById")]
    public async Task<IActionResult> GetDestinationById(string id)
    {
        if (id == null)
        {
            return BadRequest("id can't be null");
        }
        var destination = await this._touristDestinationService.GetByIdAsync(id);
        if (destination == null)
        {
            return NotFound();
        }
        return this.ApiOk(destination);
    }

    [HttpPost]
    [Route("CreateDestination")]
    public async Task<IActionResult> CreateDestination([FromForm] TouristDestinationRequest touristDestination)
    {
        List<String> linkHistoryImage = null;

        if (touristDestination.ImagesFile == null || touristDestination.ImagesFile.Count == 0)
        {
            return this.ApiError("Tourist attractions must have at least 1 photo");
        }
        var linkImage = await _imageManagementDestinationServices.AddImageDestination(touristDestination.ImagesFile);

        if (linkImage == null) return this.ApiError("No valid image uploaded.");

        if (touristDestination.HistoryStory != null &&
            touristDestination.HistoryStory.ImagesFile != null &&
            touristDestination.HistoryStory.ImagesFile.Count > 0)
        {
            linkHistoryImage = await _imageManagementDestinationServices.AddImageDestination(touristDestination.HistoryStory.ImagesFile);
            if (linkHistoryImage == null)
                return this.ApiError("No valid history image uploaded.");
        }

        var touristDestination1 = DestinationMapper.Mapper.Map<TouristDestination>(touristDestination);
        var newDestination = await this._touristDestinationService.AddAsync(touristDestination1);

        foreach (var item in linkImage)
        {
            await this._touristDestinationService.AddDestinationImage(newDestination.Id, item);
        }
        if (linkHistoryImage != null)
        {
            foreach (var historyItem in linkHistoryImage)
            {
                await this._touristDestinationService.AddDestinationHistoryStoryImage(newDestination.Id, historyItem);
            }
        }
        await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
        return CreatedAtRoute("GetDestinationById", new { id = newDestination.Id }, this.ApiOk(newDestination));
    }

    [HttpPost]
    [Route("AddDestinationImage")]
    public async Task<IActionResult> AddDestinationImage([FromForm] AddImageRequest addImageRequest)
    {
        var linkImage = await _imageManagementDestinationServices.AddImageDestination(addImageRequest.imageFile);
        foreach (var item in linkImage)
        {
            await this._touristDestinationService.AddDestinationImage(addImageRequest.id, item);
        }
        return this.ApiOk(linkImage);
    }

    [HttpPost]
    [Route("AddDestinationHistoryStoryImage")]
    public async Task<IActionResult> AddDestinationHistoryStoryImage([FromForm] AddImageRequest addImageRequest)
    {
        var linkImage = await _imageManagementDestinationServices.AddImageDestination(addImageRequest.imageFile);
        foreach (var item in linkImage)
        {
            await this._touristDestinationService.AddDestinationHistoryStoryImage(addImageRequest.id, item);
        }
        return this.ApiOk(linkImage);
    }

    [HttpPost]
    [Route("DeleteDestinationImage")]
    public async Task<IActionResult> DeleteDestinationImage([FromBody] DeleteDestinationImageRequest deleteDestinationImageRequest)
    {
        var isDeleteUrl = await this._imageManagementDestinationServices.DeleteImageDestination(deleteDestinationImageRequest.imageUrl);
        if (!isDeleteUrl)
        {
            return this.ApiError("No valid images url were removed.");
        }
        var result = await this._touristDestinationService.DeleteDestinationImage(deleteDestinationImageRequest.id, deleteDestinationImageRequest.imageUrl);

        if (result == "Image deleted successfully")
        {
            return this.ApiOk(true);
        }
        return this.ApiError("No valid images were removed.");
    }

    [HttpPost]
    [Route("DeleteDestinationHistoryStoryImage")]
    public async Task<IActionResult> DeleteDestinationHistoryStoryImage([FromBody] DeleteDestinationImageRequest deleteDestinationImageRequest)
    {
        var isDeleteUrl = await this._imageManagementDestinationServices.DeleteImageDestination(deleteDestinationImageRequest.imageUrl);
        if (!isDeleteUrl)
        {
            return this.ApiError("No valid images url were removed.");
        }
        var result = await this._touristDestinationService.DeleteDestinationHistoryStoryImage(deleteDestinationImageRequest.id, deleteDestinationImageRequest.imageUrl);

        if (result == "Image deleted successfully")
        {
            return this.ApiOk(true);
        }
        return this.ApiError("No valid images were removed.");
    }

    [HttpPut]
    [Route("UpdateDestination")]
    public async Task<IActionResult> UpdateDestination([FromBody] UpdateDestinationRequest updateDestinationRequest)
    {
        if (updateDestinationRequest == null)
        {
            return this.ApiError("Object can't be null");
        }
        var destination = await _touristDestinationService.GetByIdAsync(updateDestinationRequest.Id);
        if (destination == null)
        {
            throw new NotFoundException("No Destination was found");
        }
        if (destination.HistoryStory == null)
        {
            destination.HistoryStory = new HistoryStory();
        }
        destination.Name = updateDestinationRequest.Name;
        destination.Description = updateDestinationRequest.Description;
        destination.AvarageRating = updateDestinationRequest.AvarageRating;
        destination.Address = updateDestinationRequest.Address;
        destination.Location = updateDestinationRequest.Location;
        destination.HistoryStory.Content = updateDestinationRequest.HistoryStory.Content;
        destination.DestinationTypeId = updateDestinationRequest.DestinationTypeId;
        destination.OpeningHours = updateDestinationRequest.OpeningHours;
        destination.Capacity = updateDestinationRequest.Capacity;
        destination.Contact = updateDestinationRequest.Contact;
        destination.TagId = updateDestinationRequest.TagId;
        destination.Ticket = updateDestinationRequest.Ticket;
        //destination.TicketCount = updateDestinationRequest.TicketCount;
        destination.UpdateAt = DateTime.Now.ToLocalTime();
        await this._touristDestinationService.UpdateAsync(destination);
        await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
        return CreatedAtRoute("GetDestinationById", new { id = updateDestinationRequest.Id }, this.ApiOk(updateDestinationRequest));
    }

    [HttpPut]
    [Route("PlusFavorite/{id}")]
    public async Task<IActionResult> PlusFavorite(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return this.ApiError("id can't be null or empty");

        var result = await _touristDestinationService.PlusFavorite(id);

        if (result)
        {
            await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
            return this.ApiOk("Favorite count increased successfully");
        }
        throw new NotFoundException("Destination not found or update failed");
    }

    [HttpDelete]
    [Route("DeleteDestination/{id}")]
    public async Task<IActionResult> DeleteDestination(string id)
    {
        if (id == null)
        {
            return this.ApiError("id can't be null");
        }
        var destination = await _touristDestinationService.GetByIdAsync(id);
        if (destination == null)
        {
            throw new NotFoundException("No Destination was found");
        }
        if (destination.status == false)
        {
            return this.ApiError("Destination is already inactive");
        }
        destination.status = false;
        destination.UpdateAt = DateTime.Now.ToLocalTime();
        await this._touristDestinationService.UpdateAsync(destination);
        await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
        return this.ApiOk("Destination deleted successfully");
    }

    [HttpPut]
    [Route("RestoreDestination/{id}")]
    public async Task<IActionResult> RestoreDestination(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return this.ApiError("id can't be null or empty");
        }

        var destination = await _touristDestinationService.GetByIdAsync(id);
        if (destination == null)
        {
            throw new NotFoundException("No Destination was found");
        }

        if (destination.status == true)
        {
            return this.ApiError("Destination is already active");
        }

        destination.status = true;
        destination.UpdateAt = DateTime.Now.ToLocalTime();

        await _touristDestinationService.UpdateAsync(destination);
        await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
        return this.ApiOk("Destination restored successfully");
    }


    // Analytics
    // Format date: yyyy-mm-dd

    [HttpGet("stats-overview")]
    public async Task<IActionResult> GetStatsOverview([FromQuery] string timeRange = "month", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _touristDestinationService.GetDestinationStatsOverviewAsync(timeRange, startDate, endDate);
            if (stats == null)
                throw new NotFoundException("No analytics data available.");
            return this.ApiOk(stats);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // Top 5 Favorite Destinations
    [HttpGet("stats-getTopFavoritesDestinations")]
    public async Task<IActionResult> GetTopDestinationsByFavorites(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null)
    {
        var cacheKey = CacheKey.Destination_Top_Favorite;

        var cacheResult = await _cacheService.GetData<IEnumerable<DestinationAnalytics>>(cacheKey);
        if (cacheResult != null)
        {
            Response.Headers.Add("X-Cache", "HIT");
            Response.Headers.Add("X-Cache-Key", cacheKey);
            _logger.LogDebug("Cache hit for top favorite destinations: {CacheKey}", cacheKey);
            return this.ApiOk(cacheResult);
        }
        _logger.LogDebug("Cache miss for top favorite destinations: {CacheKey}", cacheKey);
        try
        {
            var getTop = await _touristDestinationService.GetTopDestinationsByFavoritesAsync(5, timeRange, startDate, endDate);
            if (getTop == null)
            {
                throw new NotFoundException("No analytics data available.");
            }
            // Cache the result for 5 minutes
            var cacheTtl = BuildCacheHelper.GetCacheTtl(5);
            await _cacheService.SetData(cacheKey, getTop, cacheTtl);
            return this.ApiOk(getTop);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // Top 5 View Destinations
    [HttpGet("stats-getTopViewsDestinations")]
    public async Task<IActionResult> GetTopDestinationsByViews(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var getTop = await _touristDestinationService.GetTopDestinationsByViewsAsync(5, timeRange, startDate, endDate);
            if (getTop == null)
                throw new NotFoundException("No analytics data available.");
            return this.ApiOk(getTop);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // UserDemographicsAsync
    [HttpGet("stats-getUserDemographics")]
    public async Task<IActionResult> GetUserDemographics(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var getTop = await _touristDestinationService.GetUserDemographicsAsync(timeRange, startDate, endDate);
            if (getTop == null)
                throw new NotFoundException("No analytics data available.");
            return this.ApiOk(getTop);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // Compare
    [HttpGet("stats-compare")]
    public async Task<IActionResult> CompareDestinations([FromQuery] IEnumerable<string> destinationIds, [FromQuery] string timeRange = "month", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var getTop = await _touristDestinationService.CompareDestinationsAsync(destinationIds, timeRange, startDate, endDate);
            if (getTop == null)
                throw new NotFoundException("No analytics data available.");
            return this.ApiOk(getTop);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    [HttpPost]
    [Route("GetDestinationsByIds")]
    public async Task<IActionResult> GetDestinationsByIds([FromBody] List<string> listId)
    {
        return this.ApiOk(await _touristDestinationService.GetDestinationsByIds(listId));
    }

    ///
    /// Caching optimization endpoint with redis cache
    ///

    [HttpGet("top-favorite-destination")]
    public async Task<IActionResult> GetTopFavoriteDestination([FromQuery] int limit = 10)
    {
        var cacheKey = CacheKey.Destination_Top_Favorite;
        var cacheResult = await _cacheService.GetData<List<TopFavoriteRequest>>(cacheKey);
        if (cacheResult != null)
        {
            Response.Headers.Add("X-Cache", "HIT");
            Response.Headers.Add("X-Cache-Key", cacheKey);
            _logger.LogDebug("Cache hit for top favorite destinations: {CacheKey}", cacheKey);
            return this.ApiOk(cacheResult);
        }
        var result = await _touristDestinationService.GetTopDestinationsAsync(limit);
        if (result == null || !result.Any())
        {
            this.ApiError("No favorite destinations found.");
        }
        // Cache the result for 5 minutes
        var cacheTtl = BuildCacheHelper.GetCacheTtl(5); // Cache for 5 minutes
        await _cacheService.SetData(cacheKey, result, cacheTtl);
        return this.ApiOk(result);
    }


    [HttpGet("GetDestinationsInViewport")]
    public async Task<IActionResult> GetDestinationsInViewport(
           [FromQuery] double north,
           [FromQuery] double south,
           [FromQuery] double east,
           [FromQuery] double west,
           [FromQuery] double? zoomLevel = null,
           [FromQuery] string? locationTypeId = null,
           [FromQuery] string? tagId = null,
           [FromQuery] int limit = 200)
    {
        if (north <= south || east <= west)
        {
            return BadRequest(new { success = false, message = "Invalid bounding box coordinates" });
        }
        var cacheKey = BuildCacheHelper.BuildCacheKeyForViewport(north, south, east, west, zoomLevel, locationTypeId, tagId, limit);
        var cacheResult = await _cacheService.GetData<IEnumerable<TouristDestination>>(cacheKey);

        if (cacheResult != null)
        {
            return this.ApiOk(cacheResult);
        }

        var adjustedLimit = _touristDestinationService.AdjustLimitByZoomLevel(zoomLevel);

        var destinations = await _touristDestinationService
      .GetDestinationsInBoundingBoxAsync(north, south, east, west, locationTypeId, adjustedLimit);
        if (destinations == null || !destinations.Any())
        {
            return this.ApiError("No destinations found in the specified area.");
        }
        // Cache the result for 5 minutes
        var cacheTtl = BuildCacheHelper.GetCacheTtl(5); // Cache for 5 minutes
        await _cacheService.SetData(cacheKey, destinations, cacheTtl);
        return this.ApiOk(destinations);
    }

    [HttpGet("GetDestinationsNearBy")]
    public async Task<IActionResult> GetDestinationsNearBy(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusKm = 50, // Default radius in meters
        [FromQuery] string? locationTypeId = null,
        [FromQuery] string? tagId = null,
        [FromQuery] int limit = 50)
    {
        var cacheKey = $"Destinations_Nearby_{latitude}_{longitude}_{radiusKm}_{locationTypeId}_{tagId}_{limit}";
        var cacheResult = await _cacheService.GetData<IEnumerable<TouristDestination>>(cacheKey);

        if (cacheResult != null)
        {
            return this.ApiOk(cacheResult);
        }

        var destinations = await _touristDestinationService.GetNearbyDestinationsAsync(
            latitude, longitude, radiusKm, limit, locationTypeId);
        if (destinations == null || !destinations.Any())
        {
            return this.ApiError("No destinations found near the specified location.");
        }
        // Cache the result for 5 minutes
        var cacheTtl = BuildCacheHelper.GetCacheTtl(5); // Cache for 5 minutes
        await _cacheService.SetData(cacheKey, destinations, cacheTtl);
        return this.ApiOk(destinations);
    }
}
