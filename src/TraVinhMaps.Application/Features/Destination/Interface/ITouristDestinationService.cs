// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Features.Destination.Models;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;

namespace TraVinhMaps.Application.Features.Destination.Interface;
/// <summary>
///
/// </summary>
public interface ITouristDestinationService
{
    /// <summary>
    /// Gets the by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<TouristDestination> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists all asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<TouristDestination>> ListAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists the asynchronous with the conditional
    /// </summary>
    /// <param name="predicate">The predicate is the conditional when you list (ex : a =&gt; a.id == 1)</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<TouristDestination>> ListAsync(Expression<Func<TouristDestination, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the asynchronous.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<TouristDestination> AddAsync(TouristDestination entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the range asynchronous with conditional
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<TouristDestination>> AddRangeAsync(IEnumerable<TouristDestination> entities, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates the asynchronous.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task UpdateAsync(TouristDestination entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes the asynchronous.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task DeleteAsync(TouristDestination entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Counts the asynchronous.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<long> CountAsync(Expression<Func<TouristDestination, bool>> predicate = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the by tag identifier asynchronous.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<TouristDestination>> GetByTagIdAsync(string tagId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the destination image.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<String> AddDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes the destination image.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<String> DeleteDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds the destination history story image.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<String> AddDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes the destination history story image.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="imageUrl">The image URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<String> DeleteDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the tourist destination.
    /// </summary>
    /// <param name="touristDestinationSpecParams">The tourist destination spec parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Pagination<TouristDestination>> GetTouristDestination(TouristDestinationSpecParams touristDestinationSpecParams, CancellationToken cancellationToken = default);
    /// <summary>
    /// Pluses the favorite.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> PlusFavorite(string id, CancellationToken cancellationToken = default);
    Task<bool> MinusFavorite(string id, CancellationToken cancellationToken = default);
    Task UpdateAverageRatingAsync(string destinationId, double newAverageRating, CancellationToken cancellationToken = default);

    // Overview Statistics for All Destinations
    Task<DestinationStatsOverview> GetDestinationStatsOverviewAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Destinations by Number of Likes
    Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Top Destinations by Number of Views
    Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByViewsAsync(int topCount = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // User Analysis by Age Group and Hometown
    Task<IEnumerable<DestinationUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Destination Comparison
    Task<IEnumerable<DestinationAnalytics>> CompareDestinationsAsync(IEnumerable<string> destinationIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the top10 favorite destination.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<List<TopFavoriteRequest>> GetTop10FavoriteDestination(CancellationToken cancellationToken = default);
    Task<IEnumerable<TouristDestination>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default);
    int AdjustLimitByZoomLevel(double? zoomLevel);
    Task<List<TouristDestination>> GetDestinationsInBoundingBoxAsync(double north, double south, double east, double west, string? destinationTypeId, int adjustedLimit);

    Task<List<TouristDestination>> GetNearbyDestinationsAsync(
      double latitude, double longitude, double radiusKm, int limit, string? destinationTypeId);
    Task<List<TouristDestination>> GetTopDestinationsAsync(int limit);

}
