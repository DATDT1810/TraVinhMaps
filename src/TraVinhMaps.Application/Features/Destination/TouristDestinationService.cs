// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Destination.Interface;
using TraVinhMaps.Application.Features.Destination.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.Destination;
public class TouristDestinationService : ITouristDestinationService
{
    private readonly ITouristDestinationRepository _repository;
    private readonly ILogger<TouristDestinationService> _logger;
    public TouristDestinationService(ITouristDestinationRepository repository, ILogger<TouristDestinationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<TouristDestination> AddAsync(TouristDestination entity, CancellationToken cancellationToken = default)
    {
        return await _repository.AddAsync(entity, cancellationToken);
    }

    public async Task<string> AddDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _repository.AddDestinationHistoryStoryImage(id, imageUrl, cancellationToken);
    }

    public async Task<string> AddDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _repository.AddDestinationImage(id, imageUrl, cancellationToken);
    }

    public async Task<IEnumerable<TouristDestination>> AddRangeAsync(IEnumerable<TouristDestination> entities, CancellationToken cancellationToken = default)
    {
        return await _repository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<IEnumerable<DestinationAnalytics>> CompareDestinationsAsync(IEnumerable<string> productIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range. Use: day, week, month, year");

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future.");
        }
        return await _repository.CompareDestinationsAsync(productIds, timeRange, startDate, endDate, cancellationToken);  
    }

    public async Task<long> CountAsync(Expression<Func<TouristDestination, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return await _repository.CountAsync(predicate, cancellationToken);
    }

    public Task DeleteAsync(TouristDestination entity, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<string> DeleteDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteDestinationHistoryStoryImage(id, imageUrl, cancellationToken);
    }

    public async Task<string> DeleteDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteDestinationImage(id, imageUrl, cancellationToken);
    }

    public async Task<TouristDestination> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<TouristDestination>> GetByTagIdAsync(string tagId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByTagIdAsync(tagId, cancellationToken);
    }

    public async Task<DestinationStatsOverview> GetDestinationStatsOverviewAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range. Use: day, week, month, year");

        if(startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future.");
        }

        return await _repository.GetDestinationStatsOverviewAsync(timeRange, startDate, endDate, cancellationToken);
    }

    public async Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range. Use: day, week, month, day");

        if(startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future");
        }

        return await _repository.GetTopDestinationsByFavoritesAsync(top, timeRange, startDate, endDate, cancellationToken);
    }

    public async Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByViewsAsync(int topCount = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {

        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range. Use: day, week, month, day");

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future");
        }
        return await _repository.GetTopDestinationsByViewsAsync(topCount,timeRange, startDate, endDate, cancellationToken);
    }

    public async Task<Pagination<TouristDestination>> GetTouristDestination(TouristDestinationSpecParams touristDestinationSpecParams, CancellationToken cancellationToken = default)
    {
        return await _repository.GetTouristDestination(touristDestinationSpecParams, cancellationToken);
    }

    public async Task<IEnumerable<DestinationUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Validation
        if (!string.IsNullOrEmpty(timeRange) && !new[] { "day", "week", "month", "year" }.Contains(timeRange.ToLower()))
            throw new ArgumentException("Invalid time range. Use: day, week, month, day");

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future");
        }
        return await _repository.GetUserDemographicsAsync(timeRange, startDate, endDate, cancellationToken);
    }

    public async Task<IEnumerable<TouristDestination>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.ListAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<TouristDestination>> ListAsync(Expression<Func<TouristDestination, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _repository.ListAsync(predicate, cancellationToken);
    }

    public async Task<bool> PlusFavorite(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.PlusFavorite(id, cancellationToken);
    }

    public Task UpdateAsync(TouristDestination entity, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<List<TopFavoriteRequest>> GetTop10FavoriteDestination(CancellationToken cancellationToken = default)
    {
        try
        {
            var destinations = await _repository.ListAsync(x => x.FavoriteCount > 0, cancellationToken);

            if (!destinations.Any())
            {
                throw new NotFoundException("No favorite destinations found.");
            }

            return destinations
                .OrderByDescending(x => x.FavoriteCount)
                .Take(10)
                .Select(x => new TopFavoriteRequest
                {
                    Id = x.Id,
                    Name = x.Name,
                    Image = x.Images?.FirstOrDefault() ?? string.Empty,
                    AverageRating = x.AvarageRating, // Fixed typo
                    Description = x.Description,
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top 10 favorite destinations");
            return new List<TopFavoriteRequest>(); // Ensure a return value in case of an exception
        }
    }

    public async Task<IEnumerable<TouristDestination>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default)
    {
        return await _repository.GetDestinationsByIds(idList, cancellationToken);
    }

    public async Task<bool> MinusFavorite(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.MinusFavorite(id, cancellationToken);
    }
}
