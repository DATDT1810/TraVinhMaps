// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Application.Features.OcopType.Interface;
using TraVinhMaps.Application.Features.OcopType.Mappers;
using TraVinhMaps.Application.Features.OcopType.Models;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OcopTypeController : ControllerBase
{
    private readonly IOcopTypeService _typeService;
    public OcopTypeController(IBaseRepository<Domain.Entities.OcopType> repository, IOcopTypeService ocopTypeService)
    {
        _typeService = ocopTypeService;
    }
    [HttpGet]
    [Route("GetAllOcopType")]
    public async Task<IActionResult> GetAllOcopType()
    {
        var listOcopType = await _typeService.ListAllAsync();
        return this.ApiOk(listOcopType);
    }
    [HttpGet]
    [Route("GetOcopTypeById/{id}", Name = "GetOcopTypeById")]
    public async Task<IActionResult> GetOcopTypeById(string id)
    {
        var ocopType = await _typeService.GetByIdAsync(id);
        return this.ApiOk(ocopType);
    }
    [HttpGet]
    [Route("CountOcopTypes")]
    public async Task<IActionResult> CountOcopTypes()
    {
        var countOcopTypes = await _typeService.CountAsync();
        return this.ApiOk(countOcopTypes);
    }
    [HttpPost]
    [Route("AddOcopType")]
    public async Task<IActionResult> AddOcopType([FromForm] CreateOcopTypeRequest createOcopTypeRequest)
    {
        var exitingOcopType = await _typeService.GetOcopTypeByName(createOcopTypeRequest.OcopTypeName);
        if (exitingOcopType != null)
        {
            return this.ApiError("Ocop type name already exists.");
        }
        var createOcopType = OcopTypeMapper.Mapper.Map<OcopType>(createOcopTypeRequest);
        createOcopType.OcopTypeStatus = true;
        var ocopType = await _typeService.AddAsync(createOcopType);
        return CreatedAtRoute("GetOcopTypeById", new { id = ocopType.Id }, this.ApiOk(ocopType));
    }
    [HttpPut]
    [Route("UpdateOcopType")]
    public async Task<IActionResult> UpdateOcopType([FromBody] UpdateOcopTypeRequest updateOcopTypeRequest)
    {
        var existingOcopType = await _typeService.GetByIdAsync(updateOcopTypeRequest.Id);
        if (existingOcopType == null)
        {
            throw new NotFoundException("Ocop type not found.");
        }

        existingOcopType.OcopTypeName = updateOcopTypeRequest.OcopTypeName;

        if (updateOcopTypeRequest.UpdateAt.HasValue)
        {
            existingOcopType.UpdateAt = updateOcopTypeRequest.UpdateAt.Value;
        }
        await _typeService.UpdateAsync(existingOcopType);
        return this.ApiOk("Ocop type updated successfully.");
    }
}
