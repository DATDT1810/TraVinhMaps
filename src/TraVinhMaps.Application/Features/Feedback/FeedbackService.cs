// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Security.Claims;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Feedback.Interface;
using TraVinhMaps.Application.Features.Feedback.Mapper;
using TraVinhMaps.Application.Features.Feedback.Models;
using TraVinhMaps.Application.Features.Users.Interface;
using TraVinhMaps.Application.UnitOfWorks;

namespace TraVinhMaps.Application.Features.Feedback;
public class FeedbackService : IFeedbackService
{
    private readonly IBaseRepository<Domain.Entities.Feedback> _feedbackRepository;
    private readonly ImageFeedbackService _imageFeedbackService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public FeedbackService(IBaseRepository<Domain.Entities.Feedback> feedbackRepository, ImageFeedbackService imageFeedbackService, IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _feedbackRepository = feedbackRepository;
        _imageFeedbackService = imageFeedbackService;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<Domain.Entities.Feedback> AddAsync(FeedbackRequest entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Lấy userId từ ClaimsPrincipal (được gán bởi SessionAuthenticationHandler) qua IHttpContextAccessor
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        // Map FeedbackRequest to Feedback and set UserId
        var feedback = FeedbackMapper.Mapper.Map<FeedbackRequest, Domain.Entities.Feedback>(entity);
        feedback.UserId = userId;
        feedback.CreatedAt = DateTime.UtcNow;
        feedback.CreatedAt = DateTime.SpecifyKind(feedback.CreatedAt, DateTimeKind.Utc);

        if (entity.Images != null && entity.Images.Any())
        {
            feedback.Images = await _imageFeedbackService.AddImageFeedback(entity.Images);
        }
        else
        {
            feedback.Images = new List<string>();
        }
        return await _feedbackRepository.AddAsync(feedback, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Feedback>> AddRangeAsync(IEnumerable<Domain.Entities.Feedback> entities, CancellationToken cancellationToken = default)
    {
        return await _feedbackRepository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task DeleteAsync(Domain.Entities.Feedback entity, CancellationToken cancellationToken = default)
    {
        await _feedbackRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<FeedbackResponse> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id, cancellationToken);
        if (feedback == null) throw new NotFoundException("Feedback not found");

        var user = await _userService.GetByIdAsync(feedback.UserId, cancellationToken);
        return new FeedbackResponse
        {
            Id = feedback.Id,
            UserId = feedback.UserId,
            Username = user?.Username ?? "Unknown",
            Content = feedback.Content,
            Images = feedback.Images,
            CreatedAt = feedback.CreatedAt,
        };
    }

    public async Task<IEnumerable<FeedbackResponse>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var feedbacks = await _feedbackRepository.ListAllAsync(cancellationToken);

        var feedbackResponses = FeedbackMapper.Mapper.Map<IEnumerable<FeedbackResponse>>(feedbacks);

        // Get unique UserIds from feedbacks
        var userIds = feedbacks.Select(f => f.UserId).Distinct();
        var userDict = new Dictionary<string, string>();

        // Fetch each user individually
        foreach (var userId in userIds)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            userDict[userId] = user?.Username ?? "Unknown";
        }

        // Map feedbacks to FeedbackResponse with Username
        return feedbackResponses.Select(fr =>
        {
            fr.Username = userDict.GetValueOrDefault(fr.UserId, "Unknown");
            return fr;
        }).ToList();
    }

    public async Task<IEnumerable<Domain.Entities.Feedback>> ListAsync(Expression<Func<Domain.Entities.Feedback, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _feedbackRepository.ListAsync(predicate, cancellationToken);
    }

    public async Task UpdateAsync(Domain.Entities.Feedback entity, CancellationToken cancellationToken = default)
    {
        await _feedbackRepository.UpdateAsync(entity, cancellationToken);
    }
}
