// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.EventAndFestivalFeature;
using TraVinhMaps.Application.Features.EventAndFestivalFeature.Interface;
using TraVinhMaps.Application.Features.EventAndFestivalFeature.Mappers;
using TraVinhMaps.Application.Features.EventAndFestivalFeature.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventAndFestivalController : ControllerBase
{
    private readonly IEventAndFestivalService _eventAndFestivalService;
    private readonly ImageManagementEventAndFestivalServices _imageManagementEventAndFestivalServices;
    private readonly ICacheService _cacheService;

    public EventAndFestivalController(IEventAndFestivalService eventAndFestivalService, ImageManagementEventAndFestivalServices imageManagementEventAndFestivalServices, ICacheService cacheService)
    {
        _eventAndFestivalService = eventAndFestivalService;
        _imageManagementEventAndFestivalServices = imageManagementEventAndFestivalServices;
        _cacheService = cacheService;
    }

    [HttpGet]
    [Route("GetAllEventAndFestinal")]
    public async Task<IActionResult> GetAllEventAndFestival()
    {
        var eventAndFestival = await _eventAndFestivalService.ListAllAsync();
        return this.ApiOk(eventAndFestival);
    }

    [HttpGet]
    [Route("[action]/{id}", Name = "GetEventAndFestivalById")]
    public async Task<IActionResult> GetEventAndFestivalById(string id)
    {
        var eventAndFestival = await _eventAndFestivalService.GetByIdAsync(id);
        return this.ApiOk(eventAndFestival);
    }

    [HttpPost]
    [Route("CreateEventAndFestival")]
    public async Task<IActionResult> CreateEventAndFestival([FromForm] CreateEventAndFestivalRequest createEventAndFestivalRequest)
    {
        if (createEventAndFestivalRequest == null)
        {
            return this.ApiError("Object can't be null");
        }
        if (createEventAndFestivalRequest.StartDate.Date <= DateTime.Now.Date)
        {
            return this.ApiError("StartDate must be after the current time");
        }
        if (createEventAndFestivalRequest.EndDate.Date <= createEventAndFestivalRequest.StartDate.Date)
        {
            return this.ApiError("EndDate must be after StartDate");
        }

        if (createEventAndFestivalRequest.ImagesFile == null || !createEventAndFestivalRequest.ImagesFile.Any())
        {
            return this.ApiError("Image file of event is null");
        }
        var listUrlImage = await _imageManagementEventAndFestivalServices.AddImageEventAndFestival(createEventAndFestivalRequest.ImagesFile);
        if (listUrlImage == null)
        {
            return this.ApiError("Upload file image is fail");
        }
        var newEventAndFestival = EventAndFestivalMapper.Mapper.Map<EventAndFestival>(createEventAndFestivalRequest);
        var eventAndFestival = await _eventAndFestivalService.AddAsync(newEventAndFestival);
        foreach (var item in listUrlImage)
        {
            await _eventAndFestivalService.AddEventAndFestivalImage(eventAndFestival.Id, item);
        }
        //return CreatedAtRoute("GetEventAndFestivalById", new { id = eventAndFestival.Id }, this.ApiOk(eventAndFestival));
        return this.ApiOk(eventAndFestival);
    }

    [HttpPost]
    [Route("AddEventAndFestivalImage")]
    public async Task<IActionResult> AddEventAndFestivalImage([FromForm] AddImageEventAndFestivalRequest addImageEventAndFestivalRequest)
    {
        if (addImageEventAndFestivalRequest == null)
        {
            return this.ApiError("No image data was received. Please check your form submission.");
        }
        var eventAndFestival = await _eventAndFestivalService.GetByIdAsync(addImageEventAndFestivalRequest.id);
        if (eventAndFestival == null)
        {
            throw new NotFoundException("Event or Festival not found");
        }
        var listUrlImage = await _imageManagementEventAndFestivalServices.AddImageEventAndFestival(addImageEventAndFestivalRequest.imageFile);
        if (listUrlImage == null)
        {
            return this.ApiError("Upload file image is fail");
        }
        foreach (var item in listUrlImage)
        {
            await _eventAndFestivalService.AddEventAndFestivalImage(eventAndFestival.Id, item);
        }
        return this.ApiOk(listUrlImage);
    }

    [HttpPost]
    [Route("DeleteEventAndFestivalImage")]
    public async Task<IActionResult> DeleteEventAndFestivalImage([FromBody] DeleteEventAndFestivalImage deleteEventAndFestivalImage)
    {
        if (deleteEventAndFestivalImage == null)
        {
            return this.ApiError("Object can't be null");
        }
        var eventAndFestival = await _eventAndFestivalService.GetByIdAsync(deleteEventAndFestivalImage.id);
        if (eventAndFestival == null)
        {
            throw new NotFoundException("Event or Festival not found");
        }
        var isDeleteUrl = await this._imageManagementEventAndFestivalServices.DeleteImageEventAndFestival(deleteEventAndFestivalImage.imageUrl);
        if (!isDeleteUrl)
        {
            return this.ApiError("No valid images url were removed.");
        }
        var result = await this._eventAndFestivalService.DeleteEventAndFestivalImage(eventAndFestival.Id, deleteEventAndFestivalImage.imageUrl);
        if (result == "Image deleted successfully")
        {
            return this.ApiOk(true);
        }
        return this.ApiError("No valid images were removed.");
    }

    [HttpPut]
    [Route("UpdateEventAndFestival")]
    public async Task<IActionResult> UpdateEventAndFestival([FromBody] UpdateEventAndFestivalRequest updateEventAndFestivalRequest)
    {
        if (updateEventAndFestivalRequest == null)
        {
            return BadRequest("Object can't be null");
        }
        if (updateEventAndFestivalRequest.StartDate <= DateTime.Now)
        {
            return this.ApiError("StartDate must be after the current time");
        }
        if (updateEventAndFestivalRequest.EndDate <= updateEventAndFestivalRequest.StartDate)
        {
            return this.ApiError("EndDate must be after StartDate");
        }
        var eventAndFestival = await _eventAndFestivalService.GetByIdAsync(updateEventAndFestivalRequest.Id);
        if (eventAndFestival == null)
        {
            throw new NotFoundException("Event or Festival not found");
        }
        eventAndFestival.NameEvent = updateEventAndFestivalRequest.NameEvent;
        eventAndFestival.Description = updateEventAndFestivalRequest.Description;
        eventAndFestival.StartDate = updateEventAndFestivalRequest.StartDate;
        eventAndFestival.EndDate = updateEventAndFestivalRequest.EndDate;
        eventAndFestival.Category = updateEventAndFestivalRequest.Category;
        eventAndFestival.Location = updateEventAndFestivalRequest.Location;
        eventAndFestival.TagId = updateEventAndFestivalRequest.TagId;
        await this._eventAndFestivalService.UpdateAsync(eventAndFestival);
        return CreatedAtRoute("GetEventAndFestivalById", new { id = eventAndFestival.Id }, this.ApiOk(eventAndFestival));
    }

    [HttpDelete]
    [Route("[action]/{id}", Name = "DeleteEventAndFestival")]
    public async Task<IActionResult> DeleteEventAndFestival(string id)
    {
        if (id == null)
        {
            return this.ApiError("id can't be null");
        }
        var eventAndFestival = await _eventAndFestivalService.GetByIdAsync(id);
        if (eventAndFestival == null)
        {
            throw new NotFoundException("Event or Festival not found");
        }
        if (eventAndFestival.Status == false)
        {
            return this.ApiError("Event or Festival is already inactive");
        }
        eventAndFestival.Status = false;

        await this._eventAndFestivalService.UpdateAsync(eventAndFestival);
        return CreatedAtRoute("GetEventAndFestivalById", new { id = eventAndFestival.Id }, this.ApiOk(eventAndFestival));
    }

    [HttpPut]
    [Route("RestoreEventAndFestival/{id}")]
    public async Task<IActionResult> RestoreEventAndFestival(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return this.ApiError("id can't be null or empty");
        }

        var eventAndFestival = await _eventAndFestivalService.GetByIdAsync(id);
        if (eventAndFestival == null)
        {
            throw new NotFoundException("No event and festival was found");
        }

        if (eventAndFestival.Status == true)
        {
            return this.ApiError("event and festival is already active");
        }

        eventAndFestival.Status = true;

        await _eventAndFestivalService.UpdateAsync(eventAndFestival);
        return this.ApiOk("Event and festival restored successfully");
    }

    [HttpGet]
    [Route("GetTopEvents")]
    public async Task<IActionResult> GetTopEvents()
    {
        var events = await _eventAndFestivalService.GetTopUpcomingEvents();
        return this.ApiOk(events);
    }

    [HttpGet]
    [Route("GetEventAndFestivalPaging")]
    public async Task<IActionResult> GetEventAndFestivalPaging([FromQuery] EventAndFestivalSpecParams specParams)
    {
        var cacheKey = BuildCacheHelper.BuildCacheKeyForEventAndFestival(specParams);
        var cachedData = await _cacheService.GetData<Pagination<EventAndFestival>>(cacheKey);
        if (cachedData != null)
        {
            return this.ApiOk(cachedData);
        }
        var pagedResult = await _eventAndFestivalService.GetEventAndFestivalPaging(specParams);
        if (pagedResult == null)
        {
            return this.ApiError("No event and festival found for the given parameters.");
        }
        var cacheTtl = BuildCacheHelper.GetCacheTtl(specParams.PageIndex);
        await _cacheService.SetData(cacheKey, pagedResult, cacheTtl);
        return this.ApiOk(pagedResult);
    }
}
