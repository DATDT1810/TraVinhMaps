// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Interaction.Mappers;
using TraVinhMaps.Application.Features.Interaction.Models;
using TraVinhMaps.Application.Features.OcopType.Mappers;
using TraVinhMaps.Application.Features.OcopType.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InteractionController : ControllerBase
{
    private readonly IBaseRepository<Domain.Entities.Interaction> _repository;
    public InteractionController(IBaseRepository<Domain.Entities.Interaction> repository)
    {
        _repository = repository;
    }
    [HttpGet]
    [Route("GetAllInteraction")]
    public async Task<IActionResult> GetAllInteraction()
    {
        var listInteraction = await _repository.ListAllAsync();
        return this.ApiOk(listInteraction);
    }
    [HttpGet]
    [Route("GetInteractionById/{id}", Name = "GetInteractionById")]
    public async Task<IActionResult> GetInteractionById(string id)
    {
        var interaction = await _repository.GetByIdAsync(id);
        return this.ApiOk(interaction);
    }
    [HttpGet]
    [Route("CountInteractions")]
    public async Task<IActionResult> CountInteractions()
    {
        var countInteractions = await _repository.CountAsync();
        return this.ApiOk(countInteractions);
    }
    [HttpPost]
    [Route("AddInteraction")]
    public async Task<IActionResult> AddInteraction([FromForm] CreateInteractionRequest createInteractionRequest)
    {
        var createInteraction = InteractionMapper.Mapper.Map<Interaction>(createInteractionRequest);
        var interaction = await _repository.AddAsync(createInteraction);
        return CreatedAtRoute("GetInteractionById", new { id = interaction.Id }, this.ApiOk(interaction));
    }
    [HttpPut]
    [Route("UpdateInteraction")]
    public async Task<IActionResult> UpdateInteraction([FromBody] UpdateInteractionRequest updateInteractionRequest)
    {
        var existingInteraction = await _repository.GetByIdAsync(updateInteractionRequest.Id);
        if (existingInteraction == null)
        {
            throw new NotFoundException("Interaction not found.");
        }

        existingInteraction.UserId = updateInteractionRequest.UserId;
        existingInteraction.ItemId = updateInteractionRequest.ItemId;
        existingInteraction.ItemType = updateInteractionRequest.ItemType;
        existingInteraction.TotalCount = updateInteractionRequest.TotalCount;
        existingInteraction.LastInteractionAt = updateInteractionRequest.LastInteractionAt;
        await _repository.UpdateAsync(existingInteraction);
        return this.ApiOk("Interaction updated successfully.");
    }
    [HttpDelete]
    [Route("DeleteInteraction/{id}")]
    public async Task<IActionResult> DeleteInteraction(string id)
    {
        var interaction = await _repository.GetByIdAsync(id);
        if (interaction == null)
        {
            throw new NotFoundException("Interaction not found.");
        }
        await _repository.DeleteAsync(interaction);
        return this.ApiOk("Interaction deleted successfully.");
    }
}
