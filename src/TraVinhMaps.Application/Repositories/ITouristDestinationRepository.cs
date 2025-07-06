// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TraVinhMaps.Application.Features.Destination.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.UnitOfWorks;
public interface ITouristDestinationRepository : IBaseRepository<TouristDestination>
{
    Task<IEnumerable<TouristDestination>> GetByTagIdAsync(string tagId, CancellationToken cancellationToken = default);
    Task<String> AddDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<String> DeleteDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<String> AddDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<String> DeleteDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    Task<Pagination<TouristDestination>> GetTouristDestination(TouristDestinationSpecParams touristDestinationSpecParams, CancellationToken cancellationToken = default);
    Task<bool> PlusFavorite(string id, CancellationToken cancellationToken = default);
    Task<bool> MinusFavorite(string id, CancellationToken cancellationToken = default);
    // Overview Statistics for All Destinations
    Task<DestinationStatsOverview> GetDestinationStatsOverviewAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Destinations by Number of Likes
    Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Destinations by Number of Views
    Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByViewsAsync(int topCount = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // User Analysis by Age Group and Hometown
    Task<IEnumerable<DestinationUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Compare destination
    Task<IEnumerable<DestinationAnalytics>> CompareDestinationsAsync(IEnumerable<string> destinationIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TouristDestination>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default);
}
