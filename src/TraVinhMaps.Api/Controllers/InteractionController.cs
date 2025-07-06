// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Interaction.Interface;
using TraVinhMaps.Application.Features.Interaction.Mappers;
using TraVinhMaps.Application.Features.Interaction.Models;
using TraVinhMaps.Application.Features.OcopType.Mappers;
using TraVinhMaps.Application.Features.OcopType.Models;
using TraVinhMaps.Application.Features.Review.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InteractionController : ControllerBase
{
    private readonly IInteractionService _interactionService;
    private readonly IHubContext<DashboardHub> _hubContext;
    public InteractionController(IInteractionService interactionService, IHubContext<DashboardHub> hubContext)
    {
        _interactionService = interactionService;
        _hubContext = hubContext;
    }
    [HttpGet]
    [Route("GetAllInteraction")]
    public async Task<IActionResult> GetAllInteraction()
    {
        var listInteraction = await _interactionService.ListAllAsync();
        return this.ApiOk(listInteraction);
    }
    [HttpGet]
    [Route("GetInteractionById/{id}", Name = "GetInteractionById")]
    public async Task<IActionResult> GetInteractionById(string id)
    {
        var interaction = await _interactionService.GetByIdAsync(id);
        return this.ApiOk(interaction);
    }
    [HttpGet]
    [Route("CountInteractions")]
    public async Task<IActionResult> CountInteractions()
    {
        var countInteractions = await _interactionService.CountAsync();
        return this.ApiOk(countInteractions);
    }

    [Authorize]
    [HttpPost("AddInteraction")]
    public async Task<IActionResult> AddInteraction([FromBody] List<CreateInteractionRequest> createInteractionRequests)
    {
        //if (!ModelState.IsValid)
        //    return BadRequest(ModelState);
        if (createInteractionRequests == null || !createInteractionRequests.Any())
            return this.ApiError("No interaction data provided.");
        List<Interaction> interactions = [];
        try
        {
            foreach(var createInteractionRequest in createInteractionRequests)
            {
                var interaction = await _interactionService.AddAsync(createInteractionRequest);
                interactions.Add(interaction);
            }
            return this.ApiOk(interactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing interaction", Error = ex.Message });
        }
    }

    [HttpPost("AddInteractionText{userId}")]
    public async Task<IActionResult> AddInteractionText(String userId,[FromBody] CreateInteractionRequest createInteractionRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var interaction = await _interactionService.AddTextAsync(userId, createInteractionRequest);
            await _hubContext.Clients.Group("admin").SendAsync("ReceiveFeedback", interaction.Id);
            await _hubContext.Clients.Group("super-admin").SendAsync("ReceiveFeedback", interaction.Id);

            return this.ApiOk(interaction);
        }
        catch (Exception ex)
        {
            return this.ApiError("An error occurred while processing interaction: " + ex.Message);
        }
    }



    [HttpDelete]
    [Route("DeleteInteraction/{id}")]
    public async Task<IActionResult> DeleteInteraction(string id)
    {
        var interaction = await _interactionService.GetByIdAsync(id);
        if (interaction == null)
        {
            throw new NotFoundException("Interaction not found.");
        }
        await _interactionService.DeleteAsync(interaction);
        return this.ApiOk("Interaction deleted successfully.");
    }
}
