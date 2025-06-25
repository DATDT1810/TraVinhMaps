// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Interaction.Mappers;
using TraVinhMaps.Application.Features.Interaction.Models;
using TraVinhMaps.Application.Features.InteractionLogs.Mappers;
using TraVinhMaps.Application.Features.InteractionLogs.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InteractionLogsController : ControllerBase
{
    private readonly IBaseRepository<Domain.Entities.InteractionLogs> _repository;
    public InteractionLogsController(IBaseRepository<Domain.Entities.InteractionLogs> repository)
    {
        _repository = repository;
    }
    [HttpGet]
    [Route("GetAllInteractionLogs")]
    public async Task<IActionResult> GetAllInteractionLogs()
    {
        var listInteractionLogs = await _repository.ListAllAsync();
        return this.ApiOk(listInteractionLogs);
    }
    [HttpGet]
    [Route("GetInteractionLogsById/{id}", Name = "GetInteractionLogsById")]
    public async Task<IActionResult> GetInteractionLogsById(string id)
    {
        var interactionLogs = await _repository.GetByIdAsync(id);
        return this.ApiOk(interactionLogs);
    }
    [HttpGet]
    [Route("CountInteractionLogs")]
    public async Task<IActionResult> CountInteractionLogs()
    {
        var countInteractionLogs = await _repository.CountAsync();
        return this.ApiOk(countInteractionLogs);
    }
    [HttpPost]
    [Route("AddInteractionLogs")]
    public async Task<IActionResult> AddInteractionLogs([FromForm] CreateInteractionLogsRequest createInteractionLogsRequest)
    {
        var createInteractionLogs = InteractionLogsMapper.Mapper.Map<InteractionLogs>(createInteractionLogsRequest);
        var interactionLogs = await _repository.AddAsync(createInteractionLogs);
        return CreatedAtRoute("GetInteractionLogsById", new { id = interactionLogs.Id }, this.ApiOk(interactionLogs));
    }
    [HttpPut]
    [Route("UpdateInteractionLogs")]
    public async Task<IActionResult> UpdateInteractionLogs([FromBody] UpdateInteractionLogsRequest updateInteractionLogsRequest)
    {
        var existingInteractionLogs = await _repository.GetByIdAsync(updateInteractionLogsRequest.Id);
        if (existingInteractionLogs == null)
        {
            throw new NotFoundException("InteractionLogs not found.");
        }

        existingInteractionLogs.UserId = updateInteractionLogsRequest.UserId;
        existingInteractionLogs.ItemId = updateInteractionLogsRequest.ItemId;
        existingInteractionLogs.ItemType = updateInteractionLogsRequest.ItemType;
        existingInteractionLogs.Duration = updateInteractionLogsRequest.Duration;
        await _repository.UpdateAsync(existingInteractionLogs);
        return this.ApiOk("InteractionLogs updated successfully.");
    }
    [HttpDelete]
    [Route("DeleteInteractionLogs/{id}")]
    public async Task<IActionResult> DeleteInteractionLogs(string id)
    {
        var interactionLogs = await _repository.GetByIdAsync(id);
        if (interactionLogs == null)
        {
            throw new NotFoundException("InteractionLogs not found.");
        }
        await _repository.DeleteAsync(interactionLogs);
        return this.ApiOk("InteractionLogs deleted successfully.");
    }
}
