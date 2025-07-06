// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Application.Features.Interaction.Interface;
using TraVinhMaps.Application.Features.Review.Mappers;
using TraVinhMaps.Application.Features.Review.Models;
using TraVinhMaps.Application.Features.Review;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Application.Features.Interaction.Models;
using Microsoft.AspNetCore.Http;
using TraVinhMaps.Application.Features.Feedback;
using TraVinhMaps.Application.Features.Users.Interface;
using TraVinhMaps.Application.Features.Interaction.Mappers;

namespace TraVinhMaps.Application.Features.Interaction;
public class InteractionService : IInteractionService
{
    private readonly IBaseRepository<Domain.Entities.Interaction> _baseRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public InteractionService(IBaseRepository<Domain.Entities.Interaction> baseRepository, IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _baseRepository = baseRepository;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<Domain.Entities.Interaction> AddAsync(CreateInteractionRequest createInteractionRequest, CancellationToken cancellationToken = default)
    {
        if (createInteractionRequest == null)
            throw new ArgumentNullException(nameof(createInteractionRequest));

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var itemId = createInteractionRequest.ItemId;
        var today = DateTime.UtcNow.Date;

        // Tìm interaction gần nhất với userId và itemId
        var existingInteraction = (await _baseRepository.ListAllAsync(cancellationToken))
            .Where(x => x.UserId == userId && x.ItemId == itemId)
            .OrderByDescending(x => x.LastInteractionAt ?? x.CreatedAt)
            .FirstOrDefault();

        if (existingInteraction == null)
        {
            // Không có: tạo mới
            var interaction = InteractionMapper.Mapper.Map<CreateInteractionRequest, Domain.Entities.Interaction>(createInteractionRequest);
            interaction.UserId = userId;
            interaction.LastInteractionAt = DateTime.UtcNow;
            return await _baseRepository.AddAsync(interaction, cancellationToken);
        }
        else
        {
            // Đã có: kiểm tra ngày
            var lastDate = (existingInteraction.LastInteractionAt ?? existingInteraction.CreatedAt).Date;
            if (lastDate != today)
            {
                // Nếu khác ngày/tháng/năm: tạo interaction mới
                var interaction = InteractionMapper.Mapper.Map<CreateInteractionRequest, Domain.Entities.Interaction>(createInteractionRequest);
                interaction.UserId = userId;
                interaction.LastInteractionAt = DateTime.UtcNow;
                return await _baseRepository.AddAsync(interaction, cancellationToken);
            }
            else
            {
                // Nếu cùng ngày: update interaction cũ
                existingInteraction.TotalCount += createInteractionRequest.TotalCount;
                existingInteraction.LastInteractionAt = DateTime.UtcNow;
                await _baseRepository.UpdateAsync(existingInteraction, cancellationToken);
                return existingInteraction;
            }
        }
    }

    public async Task<Domain.Entities.Interaction> AddTextAsync(string userId, CreateInteractionRequest createInteractionRequest, CancellationToken cancellationToken = default)
    {
        if (createInteractionRequest == null)
            throw new ArgumentNullException(nameof(createInteractionRequest));

        var itemId = createInteractionRequest.ItemId;
        var today = DateTime.UtcNow.Date;

        // Tìm interaction gần nhất với userId và itemId
        var existingInteraction = (await _baseRepository.ListAllAsync(cancellationToken))
            .Where(x => x.UserId == userId && x.ItemId == itemId)
            .OrderByDescending(x => x.LastInteractionAt ?? x.CreatedAt)
            .FirstOrDefault();

        if (existingInteraction == null)
        {
            // Không có: tạo mới
            var interaction = InteractionMapper.Mapper.Map<CreateInteractionRequest, Domain.Entities.Interaction>(createInteractionRequest);
            interaction.UserId = userId;
            interaction.LastInteractionAt = DateTime.UtcNow;
            return await _baseRepository.AddAsync(interaction, cancellationToken);
        }
        else
        {
            // Đã có: kiểm tra ngày
            var lastDate = (existingInteraction.LastInteractionAt ?? existingInteraction.CreatedAt).Date;
            if (lastDate != today)
            {
                // Nếu khác ngày/tháng/năm: tạo interaction mới
                var interaction = InteractionMapper.Mapper.Map<CreateInteractionRequest, Domain.Entities.Interaction>(createInteractionRequest);
                interaction.UserId = userId;
                interaction.LastInteractionAt = DateTime.UtcNow;
                return await _baseRepository.AddAsync(interaction, cancellationToken);
            }
            else
            {
                // Nếu cùng ngày: update interaction cũ
                existingInteraction.TotalCount += createInteractionRequest.TotalCount;
                existingInteraction.LastInteractionAt = DateTime.UtcNow;
                await _baseRepository.UpdateAsync(existingInteraction, cancellationToken);
                return existingInteraction;
            }
        }
    }

    public Task<long> CountAsync(Expression<Func<Domain.Entities.Interaction, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return _baseRepository.CountAsync(predicate, cancellationToken);
    }

    public Task DeleteAsync(Domain.Entities.Interaction entity, CancellationToken cancellationToken = default)
    {
        return _baseRepository.DeleteAsync(entity, cancellationToken);
    }

    public Task<Domain.Entities.Interaction> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _baseRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<IEnumerable<Domain.Entities.Interaction>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return _baseRepository.ListAllAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateInteractionRequest updateInteractionRequest, CancellationToken cancellationToken = default)
    {
        if (updateInteractionRequest == null)
            throw new ArgumentNullException(nameof(updateInteractionRequest));

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        var existingInteraction = await _baseRepository.GetByIdAsync(updateInteractionRequest.Id, cancellationToken);
        if (existingInteraction == null)
            throw new KeyNotFoundException($"Interaction with id '{updateInteractionRequest.Id}' not found.");

        var interaction = InteractionMapper.Mapper.Map<UpdateInteractionRequest, Domain.Entities.Interaction>(updateInteractionRequest);
        interaction.UserId = existingInteraction.Id;

        await _baseRepository.UpdateAsync(interaction, cancellationToken);
    }
}
