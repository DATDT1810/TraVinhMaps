// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TraVinhMaps.Application.Features.Interaction.Mappers;
using TraVinhMaps.Application.Features.Interaction.Models;
using TraVinhMaps.Application.Features.InteractionLogs.Interface;
using TraVinhMaps.Application.Features.InteractionLogs.Mappers;
using TraVinhMaps.Application.Features.InteractionLogs.Models;
using TraVinhMaps.Application.Features.Users.Interface;
using TraVinhMaps.Application.UnitOfWorks;

namespace TraVinhMaps.Application.Features.InteractionLogs;
public class InteractionLogsService : IInteractionLogsService
{
    private readonly IBaseRepository<Domain.Entities.InteractionLogs> _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    public InteractionLogsService(IBaseRepository<Domain.Entities.InteractionLogs> repository, IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<Domain.Entities.InteractionLogs> AddAsync(CreateInteractionLogsRequest createInteractionLogsRequest, CancellationToken cancellationToken = default)
    {
        if (createInteractionLogsRequest == null)
            throw new ArgumentNullException(nameof(createInteractionLogsRequest));

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var interactionLogs = InteractionLogsMapper.Mapper.Map<CreateInteractionLogsRequest, Domain.Entities.InteractionLogs>(createInteractionLogsRequest);
        interactionLogs.UserId = userId;

        return await _repository.AddAsync(interactionLogs, cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<Domain.Entities.InteractionLogs, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return _repository.CountAsync(predicate, cancellationToken);
    }

    public Task DeleteAsync(Domain.Entities.InteractionLogs entity, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(entity, cancellationToken);
    }

    public Task<Domain.Entities.InteractionLogs> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<IEnumerable<Domain.Entities.InteractionLogs>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return _repository.ListAllAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateInteractionLogsRequest updateInteractionLogsRequest, CancellationToken cancellationToken = default)
    {
        if (updateInteractionLogsRequest == null)
            throw new ArgumentNullException(nameof(updateInteractionLogsRequest));

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        var existingInteractionLogs = await _repository.GetByIdAsync(updateInteractionLogsRequest.Id, cancellationToken);
        if (existingInteractionLogs == null)
            throw new KeyNotFoundException($"Interaction with id '{updateInteractionLogsRequest.Id}' not found.");

        var interactionLogs = InteractionLogsMapper.Mapper.Map<UpdateInteractionLogsRequest, Domain.Entities.InteractionLogs>(updateInteractionLogsRequest);
        interactionLogs.UserId = existingInteractionLogs.Id;

        await _repository.UpdateAsync(interactionLogs, cancellationToken);
    }
}
