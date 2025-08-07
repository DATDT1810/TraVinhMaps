// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.LocalSpecialties;
using TraVinhMaps.Application.Features.LocalSpecialties.Interface;
using TraVinhMaps.Application.Features.LocalSpecialties.Models;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Api.Controllers;
// Controller responsible for managing local specialties in Tra Vinh
[Route("api/[controller]")]
[ApiController]
public class LocalSpecialtiesController : ControllerBase
{
    private readonly ILocalSpecialtiesService _localSpecialtiesService;
    private readonly ImageLocalSpecialtiesService _imageLocalSpecialtiesService;
    private readonly ICacheService _cacheService;

    // Constructor injection of services:
    // - _localSpecialtiesService handles core business logic for specialties
    // - _imageLocalSpecialtiesService handles image upload and deletion
    public LocalSpecialtiesController(ILocalSpecialtiesService localSpecialtiesService, ImageLocalSpecialtiesService imageLocalSpecialtiesService, ICacheService cacheService)
    {
        _localSpecialtiesService = localSpecialtiesService;
        _imageLocalSpecialtiesService = imageLocalSpecialtiesService;
        _cacheService = cacheService;
    }

    // GET: api/LocalSpecialties/all
    // Returns a list of all local specialties
    [HttpGet("all")]
    public async Task<IActionResult> GetAllLocalSpecialties()
    {
        var response = await _localSpecialtiesService.ListAllAsync();
        return this.ApiOk(response);
    }

    // GET: api/LocalSpecialties/Active
    // Returns a list of active local specialties
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveLocalSpecialties()
    {
        var response = await _localSpecialtiesService.ListAsync(l => l.Status == true);
        return this.ApiOk(response);
    }

    // GET: api/LocalSpecialties/{id}
    // Returns a single local specialty by ID
    [HttpGet("{id}", Name = "GetLocalSpecialtiesById")]
    public async Task<IActionResult> GetLocalSpecialtiesById(string id)
    {
        var localSpecialty = await _localSpecialtiesService.GetByIdAsync(id);
        return this.ApiOk(localSpecialty);
    }

    // POST: api/LocalSpecialties/create
    // Creates a new local specialty using form-data (with images, descriptions, etc.)
    [HttpPost("create")]
    public async Task<IActionResult> AddLocalSpecialties([FromForm] CreateLocalSpecialtiesRequest request)
    {
        var localSpecialties = await _localSpecialtiesService.AddAsync(request);
        return CreatedAtRoute("GetLocalSpecialtiesById", new { id = localSpecialties.Id }, this.ApiOk(localSpecialties));
    }

    // POST: api/LocalSpecialties/{id}/add-location
    // Adds a new selling location to a specific local specialty
    [HttpPost("{id}/add-location")]
    public async Task<IActionResult> AddLocation(string id, [FromBody] AddLocationRequest request)
    {
        var entity = await _localSpecialtiesService.GetByIdAsync(id);
        if (entity == null)
            return this.ApiError("Local specialties not found.");

        var location = request.ToLocationModel();

        var result = await _localSpecialtiesService.AddSellLocationAsync(id, location);
        return this.ApiOk(result);
    }

    // POST: api/LocalSpecialties/{id}/add-images
    // Uploads multiple images for a specific local specialty
    [HttpPost("{id}/add-images")]
    public async Task<IActionResult> AddImages(string id, [FromForm] List<IFormFile> imageFiles)
    {
        if (imageFiles == null || imageFiles.Count == 0)
            return this.ApiError("Please upload at least one image.");

        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(id);
        if (localSpecialties == null)
            return this.ApiError("Local specialties not found.");

        var listImageUrls = await _imageLocalSpecialtiesService.AddImageLocalSpecialties(imageFiles);
        foreach (var url in listImageUrls)
        {
            await _localSpecialtiesService.AddLocalSpecialtiesImage(id, url);
        }

        return this.ApiOk(listImageUrls);
    }

    // POST: api/LocalSpecialties/AddLocalSpecialtiesImage
    // Uploads a single image for a local specialty (used for admin form submissions)
    [HttpPost("AddLocalSpecialtiesImage")]
    public async Task<IActionResult> AddLocalSpecialtiesImage([FromForm] AddImageLocalSpecialtiesRequest request)
    {
        if (request == null)
            return this.ApiError("No image data was received. Please check your form submission.");

        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(request.Id);
        if (localSpecialties == null)
            throw new NotFoundException("Local Specialties not found");

        var listUrlImage = await _imageLocalSpecialtiesService.AddImageLocalSpecialties(request.ImageFile);
        if (listUrlImage == null)
            return this.ApiError("Upload file image is fail");

        foreach (var item in listUrlImage)
        {
            await _localSpecialtiesService.AddLocalSpecialtiesImage(request.Id, item);
        }
        return this.ApiOk(listUrlImage);
    }

    // POST: api/LocalSpecialties/DeleteLocalSpecialtiesImage
    // Deletes a specific image from a local specialty and storage
    [HttpPost]
    [Route("DeleteLocalSpecialtiesImage")]
    public async Task<IActionResult> DeleteLocalSpecialtiesImage([FromBody] DeleteImageLocalSpecialtiesRequest request)
    {
        if (request == null)
            return this.ApiError("Object can't be null");

        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(request.Id);
        if (localSpecialties == null)
            throw new NotFoundException("Local Specialties not found");

        var isDeleteUrl = await _imageLocalSpecialtiesService.DeleteImageLocalSpecialties(request.ImageUrl);
        if (!isDeleteUrl)
            return this.ApiError("No valid images url were removed.");

        var result = await _localSpecialtiesService.DeleteLocalSpecialtiesImage(request.Id, request.ImageUrl);
        if (result == "Image deleted successfully")
            return this.ApiOk(true);

        return this.ApiError("No valid images were removed.");
    }

    // DELETE: api/LocalSpecialties/{id}
    // Soft deletes (disables) a local specialty by setting its Status = false
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocalSpecialties(string id)
    {
        if (string.IsNullOrEmpty(id))
            return this.ApiError("Id can't be null or empty");

        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(id);
        if (localSpecialties == null)
            throw new NotFoundException("No local specialties was found");

        localSpecialties.Status = false;
        await _localSpecialtiesService.DeleteLocalSpecialtiesAsync(localSpecialties.Id);
        return this.ApiOk("Local specialties delete successfully");
    }

    // PUT: api/LocalSpecialties/restore/{id}
    // Restores a previously soft-deleted local specialty
    [HttpPut("restore/{id}")]
    public async Task<IActionResult> RestoreLocalSpecialties(string id)
    {
        if (string.IsNullOrEmpty(id))
            return this.ApiError("Id can't be null or empty");

        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(id);
        if (localSpecialties == null)
            throw new NotFoundException("No local specialties was found");

        if (localSpecialties.Status == true)
            return this.ApiError("Local specialties is already active");

        localSpecialties.Status = true;
        await _localSpecialtiesService.RestoreLocalSpecialtiesAsync(id);
        return this.ApiOk("Local specialties restored successfully");
    }

    // PUT: api/LocalSpecialties/update
    // Updates the information of an existing local specialty
    [HttpPut("update")]
    public async Task<IActionResult> UpdateLocalSpecialties([FromBody] UpdateLocalSpecialtiesRequest request)
    {
        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(request.Id);
        if (localSpecialties == null)
            return this.ApiError("Local specialties not found.");

        request.Id = localSpecialties.Id; // Ensure ID is consistent
        await _localSpecialtiesService.UpdateAsync(request);
        return this.ApiOk("Update successfully");
    }

    // GET: api/LocalSpecialties/{id}/locations
    // Returns a list of all selling locations for the given local specialty
    [HttpGet("{id}/locations")]
    public async Task<IActionResult> GetLocations(string id)
    {
        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(id);
        if (localSpecialties == null)
            return this.ApiError("Local specialties not found.");

        return this.ApiOk(localSpecialties.Locations);
    }

    // GET: api/LocalSpecialties/{id}/locations/{markerId}
    // Returns a specific location's detail by marker ID for the local specialty
    [HttpGet("{id}/locations/{markerId}")]
    public async Task<IActionResult> GetLocationDetail(string id, string markerId)
    {
        var specialty = await _localSpecialtiesService.GetByIdAsync(id);
        if (specialty == null)
            return this.ApiError("Local specialties not found.");

        var location = specialty.Locations?.FirstOrDefault(x => x.MarkerId == markerId);
        if (location == null)
            return this.ApiError("Location not found.");

        return this.ApiOk(location);
    }

    // PUT: api/LocalSpecialties/{id}/locations
    // Updates a specific selling location of a local specialty
    [HttpPut("{id}/locations")]
    public async Task<IActionResult> UpdateLocation(string id, [FromBody] UpdateLocationRequest request)
    {
        if (string.IsNullOrEmpty(id) || request == null || string.IsNullOrEmpty(request.LocationId))
            return this.ApiError("Invalid input.");

        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(id);
        if (localSpecialties == null)
            return this.ApiError("Local specialties not found.");

        var result = await _localSpecialtiesService.UpdateSellLocationAsync(id, request.ToLocationModel());
        if (result == null)
            return this.ApiError("Failed to update location.");

        return this.ApiOk(result);
    }

    // DELETE: api/LocalSpecialties/{id}/locations/{locationId}
    // Deletes a specific selling location from a local specialty
    [HttpDelete("{id}/locations/{locationId}")]
    public async Task<IActionResult> DeleteLocation(string id, string locationId)
    {
        var localSpecialties = await _localSpecialtiesService.GetByIdAsync(id);
        if (localSpecialties == null)
            return this.ApiError("Local specialties not found.");

        var result = await _localSpecialtiesService.RemoveSellLocationAsync(id, locationId);
        if (result)
            return this.ApiOk("Location deleted successfully");

        return this.ApiError("Failed to delete location");
    }

    [HttpPost]
    [Route("GetLocalSpecialtiByIds")]
    public async Task<IActionResult> GetLocalSpecialtiByIds([FromBody] List<string> listId)
    {
        return this.ApiOk(await _localSpecialtiesService.GetDestinationsByIds(listId));
    }

    [HttpGet("LocalSpecialities-Paging")]
    public async Task<IActionResult> GetLocalSpeialitiesPaging([FromQuery] LocalSpecialtiesSpecParams specParams)
    {
        var cacheKey = BuildCacheHelper.BuildCacheKeyForLocalSpecialties(specParams);
        var cacheResult = await _cacheService.GetData<Pagination<Domain.Entities.LocalSpecialties>>(cacheKey);
        if (cacheResult != null)
        {
            return this.ApiOk(cacheResult);
        }
        var result = await _localSpecialtiesService.GetLocalSpecialtiesPaging(specParams);
        if (result == null)
        {
            return this.ApiError("No local specialties found.");
        }
        // Cache the result for 5 minutes
        var cacheTTL = BuildCacheHelper.GetCacheTtl(specParams.PageIndex);
        await _cacheService.SetData(cacheKey, result, cacheTTL);
        return this.ApiOk(result);
    }
}
