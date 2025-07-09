// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Destination;
using TraVinhMaps.Application.Features.Destination.Interface;
using TraVinhMaps.Application.Features.ItineraryPlan.Interface;
using TraVinhMaps.Application.Features.ItineraryPlan.Mappers;
using TraVinhMaps.Application.Features.ItineraryPlan.Models;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ItineraryPlanController : ControllerBase
{
    private readonly IItineraryPlanService _itineraryPlanService;
    private readonly ITouristDestinationService _touristDestinationService;

    public ItineraryPlanController(IItineraryPlanService itineraryPlanService, ITouristDestinationService touristDestinationService)
    {
        _itineraryPlanService = itineraryPlanService;
        _touristDestinationService = touristDestinationService;
    }

    [HttpGet]
    [Route("GetAllItineraryPlan")]
    public async Task<IActionResult> GetAllItineraryPlan()
    {
        var listItineraryPlan = await this._itineraryPlanService.ListAllAsync();
        return this.ApiOk(listItineraryPlan);
    }

    [HttpGet]
    [Route("[action]/{id}", Name = "GetItineraryPlanById")]
    public async Task<IActionResult> GetItineraryPlanById(string id)
    {
        var itineraryPlanDetail = await this._itineraryPlanService.GetByIdAsync(id);
        return this.ApiOk(itineraryPlanDetail);
    }

    [HttpPost]
    [Route("CreateItineraryPlan")]
    public async Task<IActionResult> CreateItineraryPlan(ItineraryPlanRequest itineraryPlanRequest)
    {
        if (itineraryPlanRequest.Locations.Count < 2)
        {
            return this.ApiError("The itinerary must have at least 2 addresses.");
        }
        var itineraryPlan = ItineraryPlanMapper.Mapper.Map<ItineraryPlan>(itineraryPlanRequest);
        var itineraryPlanCreated = await _itineraryPlanService.AddAsync(itineraryPlan);
        return CreatedAtRoute("GetItineraryPlanById", new { id = itineraryPlanCreated.Id }, this.ApiOk(itineraryPlanCreated));
    }

    [HttpPut]
    [Route("UpdateItineraryPlan")]
    public async Task<IActionResult> UpdateItineraryPlan(UpdateItineraryPlanRequest updateItineraryPlanRequest)
    {
        if (updateItineraryPlanRequest == null)
        {
            return this.ApiError("Object can't be null");
        }
        var itineraryPlan = await _itineraryPlanService.GetByIdAsync(updateItineraryPlanRequest.Id);
        if (itineraryPlan == null)
        {
            throw new NotFoundException("No itineraryPlan was found");
        }
        itineraryPlan.Name = updateItineraryPlanRequest.Name;
        itineraryPlan.Duration = updateItineraryPlanRequest.Duration;
        itineraryPlan.EstimatedCost = updateItineraryPlanRequest.EstimatedCost;
        itineraryPlan.UpdateAt = DateTime.Now.ToLocalTime();
        await this._itineraryPlanService.UpdateAsync(itineraryPlan);
        return CreatedAtRoute("GetItineraryPlanById", new { id = itineraryPlan.Id }, this.ApiOk(itineraryPlan));
    }

    [HttpDelete]
    [Route("DeleteItineraryPlan")]
    public async Task<IActionResult> DeleteItineraryPlan(string id)
    {
        if (id == null)
        {
            return this.ApiError("id can't be null");
        }
        var itineraryPlan = await _itineraryPlanService.GetByIdAsync(id);
        if (itineraryPlan == null)
        {
            throw new NotFoundException("No Destination was found");
        }
        if (itineraryPlan.Status == false)
        {
            return this.ApiError("Itinerary plan is already inactive");
        }
        itineraryPlan.Status = false;
        itineraryPlan.UpdateAt = DateTime.Now.ToLocalTime();
        await this._itineraryPlanService.UpdateAsync(itineraryPlan);
        //return NoContent();
        return this.ApiOk("Itinerary plan deleted successfully");
    }

    [HttpPut]
    [Route("RestoreItineraryPlan/{id}")]
    public async Task<IActionResult> RestoreItineraryPlan(string id)
    {
        if (id == null)
        {
            return this.ApiError("id can't be null");
        }
        var itineraryPlan = await _itineraryPlanService.GetByIdAsync(id);
        if (itineraryPlan == null)
        {
            throw new NotFoundException("No Destination was found");
        }
        if (itineraryPlan.Status == true)
        {
            return this.ApiError("Itinerary plan is already inactive");
        }
        itineraryPlan.Status = true;
        itineraryPlan.UpdateAt = DateTime.Now.ToLocalTime();
        await this._itineraryPlanService.UpdateAsync(itineraryPlan);
        //return NoContent();
        return this.ApiOk("Itinerary plan restor successfully");
    }

    [HttpGet]
    [Route("GetAllItineraryPlanWithDestination")]
    public async Task<IActionResult> GetAllItineraryPlanWithDestination()
    {
        var listItineraryPlan = await this._itineraryPlanService.ListAsync(p => p.Status == true);
        var listDestination = await this._touristDestinationService.GetDestinationsByIds(listItineraryPlan.FirstOrDefault().Locations);
        if (listItineraryPlan == null || !listItineraryPlan.Any())
            return Ok(new List<ItineraryPlanResponse>());
        // Tổng hợp toàn bộ location IDs từ các itinerary
        var allLocationIds = listItineraryPlan
            .Where(x => x.Locations != null)
            .SelectMany(x => x.Locations!)
            .Distinct()
            .ToList();

        // get all destination by id list
        var allDestinations = await _touristDestinationService.GetDestinationsByIds(allLocationIds);

        // Create result list
        var result = listItineraryPlan.Select(plan => new ItineraryPlanResponse
        {
            Id = plan.Id,
            Name = plan.Name,
            Duration = plan.Duration,
            EstimatedCost = plan.EstimatedCost,
            Locations = plan.Locations,
            touristDestinations = allDestinations
                .Where(d => plan.Locations != null && plan.Locations.Contains(d.Id))
                .ToList()
        }).ToList();

        return this.ApiOk(result);
    }
}
