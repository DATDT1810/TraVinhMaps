// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Interaction.Mappers;
using TraVinhMaps.Application.Features.Interaction.Models;
using TraVinhMaps.Application.Features.InteractionLogs.Interface;
using TraVinhMaps.Application.Features.InteractionLogs.Mappers;
using TraVinhMaps.Application.Features.InteractionLogs.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InteractionLogsController : ControllerBase
{
    private readonly IInteractionLogsService _interactionLogsService;
    private readonly IHubContext<DashboardHub> _hubContext;
    public InteractionLogsController(IInteractionLogsService interactionLogsService, IHubContext<DashboardHub> hubContext)
    {
        _interactionLogsService = interactionLogsService;
        _hubContext = hubContext;
    }
    [HttpGet]
    [Route("GetAllInteractionLogs")]
    public async Task<IActionResult> GetAllInteractionLogs()
    {
        var listInteractionLogs = await _interactionLogsService.ListAllAsync();
        return this.ApiOk(listInteractionLogs);
    }
    [HttpGet]
    [Route("GetInteractionLogsById/{id}", Name = "GetInteractionLogsById")]
    public async Task<IActionResult> GetInteractionLogsById(string id)
    {
        var interactionLogs = await _interactionLogsService.GetByIdAsync(id);
        return this.ApiOk(interactionLogs);
    }
    [HttpGet]
    [Route("CountInteractionLogs")]
    public async Task<IActionResult> CountInteractionLogs()
    {
        var countInteractionLogs = await _interactionLogsService.CountAsync();
        return this.ApiOk(countInteractionLogs);
    }
    [Authorize]
    [HttpPost("AddInteractionLog")]
    public async Task<IActionResult> AddInteractionLog([FromBody] List<CreateInteractionLogsRequest> createInteractionLogsRequests)
    {
        //if (!ModelState.IsValid)
        //    return BadRequest(ModelState);
        if(createInteractionLogsRequests == null || !createInteractionLogsRequests.Any())
        {
            return this.ApiError("No interaction log data provided.");
        }
        List<InteractionLogs> InteractionLogs = [];
        try
        {
            foreach(var createInteractionLogsRequest in createInteractionLogsRequests)
            {
                var interactionLogs = await _interactionLogsService.AddAsync(createInteractionLogsRequest);
                InteractionLogs.Add(interactionLogs);
            }

            return this.ApiOk(InteractionLogs);
        }
        catch (Exception ex)
        {
            return this.ApiError("An error occurred while adding interaction logs: "+ ex.Message);
        }
    }
    [HttpDelete]
    [Route("DeleteInteractionLogs/{id}")]
    public async Task<IActionResult> DeleteInteractionLogs(string id)
    {
        var interactionLogs = await _interactionLogsService.GetByIdAsync(id);
        if (interactionLogs == null)
        {
            throw new NotFoundException("InteractionLogs not found.");
        }
        await _interactionLogsService.DeleteAsync(interactionLogs);
        return this.ApiOk("InteractionLogs deleted successfully.");
    }
}
