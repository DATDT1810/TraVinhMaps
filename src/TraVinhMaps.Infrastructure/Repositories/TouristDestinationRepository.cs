// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoDB.Bson;
using MongoDB.Driver;
using TraVinhMaps.Application.Features.Destination.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;
using TraVinhMaps.Infrastructure.Db;

namespace TraVinhMaps.Infrastructure.CustomRepositories;
public class TouristDestinationRepository : BaseRepository<TouristDestination>, ITouristDestinationRepository
{
    private readonly IMongoCollection<Interaction> _interactionCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<InteractionLogs> _interactionLogsCollection;
    public TouristDestinationRepository(IDbContext context) : base(context)
    {
        _interactionCollection = context.Database.GetCollection<Interaction>("Interaction");
        _userCollection = context.Database.GetCollection<User>("User");
        _interactionLogsCollection = context.Database.GetCollection<InteractionLogs>("InteractionLogs");

        // Tạo index để tối ưu hiệu suất
        _interactionCollection.Indexes.CreateOne(new CreateIndexModel<Interaction>(
            Builders<Interaction>.IndexKeys.Ascending(i => i.ItemId).Ascending(i => i.ItemType).Ascending(i => i.CreatedAt)));
        _userCollection.Indexes.CreateOne(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending("favorites.favorite_itemId").Ascending("favorites.favorite_type")));
        _interactionLogsCollection.Indexes.CreateOne(new CreateIndexModel<InteractionLogs>(
            Builders<InteractionLogs>.IndexKeys.Ascending(i => i.ItemId).Ascending(i => i.ItemType).Ascending(i => i.CreatedAt)));
    }

    public async Task<string> AddDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.Id, id);

        var destination = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (destination == null)
        {
            return null;
        }
        if (destination.HistoryStory.Images == null)
        {
            var setImagesUpdate = Builders<TouristDestination>.Update.Set(p => p.HistoryStory.Images, new List<string>());
            await _collection.UpdateOneAsync(filter, setImagesUpdate);
        }
        var pushImageUpdate = Builders<TouristDestination>.Update.Push(p => p.HistoryStory.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pushImageUpdate, cancellationToken: cancellationToken);
        return imageUrl;
    }

    public async Task<string> AddDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.Id, id);

        var destination = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (destination == null) return null;
        if (destination.Images == null)
        {
            var setImagesUpdate = Builders<TouristDestination>.Update.Set(p => p.Images, new List<string>());
            await _collection.UpdateOneAsync(filter, setImagesUpdate);
        }
        var pushImageUpdate = Builders<TouristDestination>.Update.Push(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pushImageUpdate, cancellationToken: cancellationToken);
        return imageUrl;
    }

    public Task<IEnumerable<DestinationAnalytics>> CompareProductsAsync(IEnumerable<string> productIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string> DeleteDestinationHistoryStoryImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.Id, id);

        var destination = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (destination == null || destination.HistoryStory.Images == null || !destination.HistoryStory.Images.Contains(imageUrl))
        {
            return null;
        }

        var pullImageUpdate = Builders<TouristDestination>.Update.Pull(p => p.HistoryStory.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pullImageUpdate, cancellationToken: cancellationToken);
        return "Image deleted successfully";
    }

    public async Task<string> DeleteDestinationImage(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.Id, id);

        var destination = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (destination == null || destination.Images == null || !destination.Images.Contains(imageUrl))
        {
            return null;
        }

        var pullImageUpdate = Builders<TouristDestination>.Update.Pull(p => p.Images, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, pullImageUpdate, cancellationToken: cancellationToken);
        return "Image deleted successfully";
    }

    public async Task<IEnumerable<TouristDestination>> GetByTagIdAsync(string tagId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.TagId, tagId);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves aggregated destination statistics including views, interactions, and favorites.
    /// This method builds and executes a MongoDB aggregation pipeline over multiple collections:
    /// - Destination
    /// - Interaction
    /// - InteractionLogs
    /// - User (favorites)
    /// </summary>
    public async Task<DestinationStatsOverview> GetDestinationStatsOverviewAsync(
        string timeRange = "month",
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Get current UTC time
        var now = DateTime.UtcNow;

        DateTime filterStartDate;
        DateTime? filterEndDate = null;

        // Determine the date range to filter based on input or fallback to timeRange
        if (startDate.HasValue && endDate.HasValue)
        {
            filterStartDate = startDate.Value;
            filterEndDate = endDate.Value;
        }
        else
        {
            filterStartDate = startDate ?? timeRange.ToLower() switch
            {
                "day" => now.AddDays(-1),
                "week" => now.AddDays(-7),
                "month" => new DateTime(now.Year, now.Month, 1),
                "year" => new DateTime(now.Year, 1, 1),
                _ => new DateTime(now.Year, now.Month, 1)
            };
        }

        // Convert to BsonDateTime for filtering
        var bsonStartDate = (BsonDateTime)filterStartDate;
        var bsonEndDate = filterEndDate.HasValue ? (BsonDateTime)filterEndDate.Value : null;

        // Interaction filter condition
        var interactionCond = new BsonArray
    {
        new BsonDocument("$eq", new BsonArray { "$$interaction.itemType", "Destination" }),
        new BsonDocument("$gte", new BsonArray { "$$interaction.createdAt", bsonStartDate })
    };
        if (bsonEndDate != null)
            interactionCond.Add(new BsonDocument("$lte", new BsonArray { "$$interaction.createdAt", bsonEndDate }));

        // InteractionLogs filter condition
        var logCond = new BsonArray
    {
        new BsonDocument("$eq", new BsonArray { "$$log.itemType", "Destination" }),
        new BsonDocument("$gte", new BsonArray { "$$log.createdAt", bsonStartDate })
    };
        if (bsonEndDate != null)
            logCond.Add(new BsonDocument("$lte", new BsonArray { "$$log.createdAt", bsonEndDate }));

        // Build aggregation pipeline
        var pipeline = new List<BsonDocument>
    {
        // Stage 1: Match only active destinations
        new BsonDocument("$match", new BsonDocument("status", true)),

        // Stage 2: Project only needed fields
        new BsonDocument("$project", new BsonDocument { { "_id", 1 }, { "name", 1 } }),

        // Stage 3: Lookup interactions related to the destination
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Interaction" },
            { "localField", "_id" },
            { "foreignField", "itemId" },
            { "as", "interactionsMed" }
        }),

        // Stage 4: Filter interactions by date and itemType
        new BsonDocument("$addFields", new BsonDocument("interactions",
            new BsonDocument("$filter", new BsonDocument
            {
                { "input", "$interactionsMed" },
                { "as", "interaction" },
                { "cond", new BsonDocument("$and", interactionCond) }
            })
        )),

        // Stage 5: Lookup interaction logs
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "InteractionLogs" },
            { "localField", "_id" },
            { "foreignField", "itemId" },
            { "as", "interactionLogsMed" }
        }),

        // Stage 6: Filter logs by date and itemType
        new BsonDocument("$addFields", new BsonDocument("interactionLogs",
            new BsonDocument("$filter", new BsonDocument
            {
                { "input", "$interactionLogsMed" },
                { "as", "log" },
                { "cond", new BsonDocument("$and", logCond) }
            })
        )),

        // Stage 7: Lookup users who have this destination in their favorites
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "User" },
            { "localField", "_id" },
            { "foreignField", "favorites.itemId" },
            { "as", "users" }
        }),

        // Stage 8: Count how many users favorited this destination
        new BsonDocument("$addFields", new BsonDocument("FavoriteCount",
            new BsonDocument("$size", "$users")
        )),

        // Stage 9: Add string version of destination id
        new BsonDocument("$addFields", new BsonDocument("Id",
            new BsonDocument("$toString", "$_id"))),

        // Stage 10: Project computed fields
        new BsonDocument("$project", new BsonDocument
        {
            { "Id", 1 },
            { "LocationName", "$name" },
            { "ViewCount", new BsonDocument("$size", "$interactionLogs") },
            { "InteractionCount", new BsonDocument("$sum", "$interactions.totalCount") },
            { "FavoriteCount", 1 }
        }),

        // Stage 11: Group result into single summary + details
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "TotalDestinations", new BsonDocument("$sum", 1) },
            { "TotalViews", new BsonDocument("$sum", "$ViewCount") },
            { "TotalInteractions", new BsonDocument("$sum", "$InteractionCount") },
            { "TotalFavorites", new BsonDocument("$sum", "$FavoriteCount") },
            { "DestinationDetails", new BsonDocument("$push", "$$ROOT") }
        }),

        // Stage 12: Remove internal _id and keep only needed summary fields
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "TotalDestinations", 1 },
            { "TotalViews", 1 },
            { "TotalInteractions", 1 },
            { "TotalFavorites", 1 },
            { "DestinationDetails", 1 }
        })
    };

        // Execute the pipeline
        var bsonResult = await _collection
            .Aggregate<BsonDocument>(pipeline)
            .FirstOrDefaultAsync(cancellationToken);

        // Handle empty result
        if (bsonResult == null)
        {
            return new DestinationStatsOverview
            {
                TotalDestinations = 0,
                TotalViews = 0,
                TotalInteractions = 0,
                TotalFavorites = 0,
                DestinationDetails = new List<DestinationAnalytics>()
            };
        }

        // Map BSON result to C# DTO
        var overview = new DestinationStatsOverview
        {
            TotalDestinations = bsonResult.GetValue("TotalDestinations", 0).ToInt32(),
            TotalViews = bsonResult.GetValue("TotalViews", 0).ToInt64(),
            TotalInteractions = bsonResult.GetValue("TotalInteractions", 0).ToInt64(),
            TotalFavorites = bsonResult.GetValue("TotalFavorites", 0).ToInt64(),
            DestinationDetails = bsonResult.GetValue("DestinationDetails", new BsonArray())
                .AsBsonArray
                .Select(item =>
                {
                    var doc = item.AsBsonDocument;
                    return new DestinationAnalytics
                    {
                        Id = doc.GetValue("Id", "").AsString,
                        LocationName = doc.GetValue("LocationName", "").AsString,
                        ViewCount = doc.GetValue("ViewCount", 0).ToInt64(),
                        InteractionCount = doc.GetValue("InteractionCount", 0).ToInt64(),
                        FavoriteCount = doc.GetValue("FavoriteCount", 0).ToInt64()
                    };
                })
                .ToList()
        };

        // Return the final result
        return overview;
    }

    public async Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = await GetDestinationStatsOverviewAsync(timeRange, startDate, endDate, cancellationToken);
        return analytics.DestinationDetails.OrderByDescending(d => d.FavoriteCount)
            .ThenByDescending(d => d.LocationName)
            .Take(top)
            .ToList();
    }

    public async Task<IEnumerable<DestinationAnalytics>> GetTopDestinationsByViewsAsync(int topCount = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = await GetDestinationStatsOverviewAsync(timeRange, startDate, endDate, cancellationToken);
        return analytics.DestinationDetails.OrderByDescending(d => d.ViewCount)
            .ThenByDescending(d => d.LocationName)
            .Take(topCount)
            .ToList();
    }

    public async Task<IEnumerable<DestinationUserDemographics>> GetUserDemographicsAsync(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        DateTime filterStartDate;
        DateTime? filterEndDate = null;

        if (startDate.HasValue && endDate.HasValue)
        {
            filterStartDate = startDate.Value;
            filterEndDate = endDate.Value;
        }
        else
        {
            filterStartDate = timeRange.ToLower() switch
            {
                "day" => now.AddDays(-1),
                "week" => now.AddDays(-7),
                "month" => new DateTime(now.Year, now.Month, 1),
                "year" => new DateTime(now.Year, 1, 1),
                _ => new DateTime(now.Year, now.Month, 1)
            };

            filterEndDate = now;
        }
        var bsonStartDate = BsonValue.Create(filterStartDate);
        var bsonEndDate = BsonValue.Create(filterEndDate.Value);

        var interactionCond = new BsonArray
        {
            new BsonDocument("$eq", new BsonArray { "$$interaction.itemType", "Destination" }),
            new BsonDocument("$gte", new BsonArray { "$$interaction.createdAt", bsonStartDate }),
            new BsonDocument("$lte", new BsonArray { "$$interaction.createdAt", bsonEndDate })
        };

        var logCond = new BsonArray
        {
            new BsonDocument("$eq", new BsonArray { "$$log.itemType", "Destination" }),
            new BsonDocument("$gte", new BsonArray { "$$log.createdAt", bsonStartDate }),
            new BsonDocument("$lte", new BsonArray { "$$log.createdAt", bsonEndDate })
        };

        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", new BsonDocument("status", true)),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "name", new BsonDocument("$ifNull", new BsonArray { "$name", "Unknown" }) }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Interaction" },
                { "localField", "_id" },
                { "foreignField", "itemId" },
                { "as", "interactions" }
            }),
            new BsonDocument("$addFields", new BsonDocument("interactions",
                new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$interactions" },
                    { "as", "interaction" },
                    { "cond", new BsonDocument("$and", interactionCond) }
                })
            )),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "InteractionLogs" },
                { "localField", "_id" },
                { "foreignField", "itemId" },
                { "as", "interactionLogs" }
            }),
            new BsonDocument("$addFields", new BsonDocument("interactionLogs",
                new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$interactionLogs" },
                    { "as", "log" },
                    { "cond", new BsonDocument("$and", logCond) }
                })
            )),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "User" },
                { "let", new BsonDocument("userIds", "$interactions.userId") },
                { "pipeline", new BsonArray
                {
                    new BsonDocument("$match", new BsonDocument("$expr",
                        new BsonDocument("$in", new BsonArray { "$_id", "$$userIds" })
                    ))
                } },
                { "as", "interactionUsers" }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "User" },
                { "let", new BsonDocument("userIds", "$interactionLogs.userId") },
                { "pipeline", new BsonArray
                {
                    new BsonDocument("$match", new BsonDocument("$expr",
                        new BsonDocument("$in", new BsonArray { "$_id", "$$userIds" })
                    ))
                } },
                { "as", "logUsers" }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "User" },
                { "localField", "_id" },
                { "foreignField", "favorites.itemId" },
                { "as", "wishlistUsers" }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
                { "allUsers", new BsonDocument("$setUnion", new BsonArray
                {
                    new BsonDocument("$ifNull", new BsonArray { "$interactionUsers", new BsonArray() }),
                    new BsonDocument("$ifNull", new BsonArray { "$logUsers", new BsonArray() }),
                    new BsonDocument("$ifNull", new BsonArray { "$wishlistUsers", new BsonArray() })
                })}
            }),
            new BsonDocument("$match", new BsonDocument
            {
                { "allUsers.0", new BsonDocument("$exists", true) }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$allUsers" },
                { "preserveNullAndEmptyArrays", false }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
                { "age", new BsonDocument("$cond", new BsonDocument
                {
                    { "if", new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("$ne", new BsonArray { "$allUsers.profile.dateOfBirth", BsonNull.Value }),
                        new BsonDocument("$ne", new BsonArray { "$allUsers.profile.dateOfBirth", "" })
                    })},
                    { "then", new BsonDocument("$floor", new BsonDocument("$divide",
                        new BsonArray
                        {
                            new BsonDocument("$subtract", new BsonArray
                            {
                                new BsonDocument("$toDate", now),
                                "$allUsers.profile.dateOfBirth"
                            }),
                            31557600000
                        }))},
                    { "else", -1 }
                })},
                { "hometown", new BsonDocument("$cond", new BsonDocument
                {
                    { "if", new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("$ne", new BsonArray { "$allUsers.profile.address", BsonNull.Value }),
                        new BsonDocument("$ne", new BsonArray { "$allUsers.profile.address", "" })
                    })},
                    { "then", new BsonDocument("$trim", new BsonDocument
                    {
                        { "input", new BsonDocument("$arrayElemAt",
                            new BsonArray
                            {
                                new BsonDocument("$split", new BsonArray { "$allUsers.profile.address", "," }),
                                -1
                            })
                        }
                    })},
                    { "else", "Unknown" }
                })}
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        { "destinationId", "$_id" },
                        { "locationName", "$name" },
                        { "ageGroup", new BsonDocument("$switch", new BsonDocument
                            {
                                { "branches", new BsonArray
                                    {
                                        new BsonDocument
                                        {
                                            { "case", new BsonDocument("$and", new BsonArray
                                                {
                                                    new BsonDocument("$gte", new BsonArray { "$age", 0 }),
                                                    new BsonDocument("$lte", new BsonArray { "$age", 18 })
                                                })},
                                            { "then", "0-18" }
                                        },
                                        new BsonDocument
                                        {
                                            { "case", new BsonDocument("$and", new BsonArray
                                                {
                                                    new BsonDocument("$gte", new BsonArray { "$age", 19 }),
                                                    new BsonDocument("$lte", new BsonArray { "$age", 30 })
                                                })},
                                            { "then", "18-30" }
                                        },
                                        new BsonDocument
                                        {
                                            { "case", new BsonDocument("$and", new BsonArray
                                                {
                                                    new BsonDocument("$gte", new BsonArray { "$age", 31 }),
                                                    new BsonDocument("$lte", new BsonArray { "$age", 50 })
                                                })},
                                            { "then", "30-50" }
                                        },
                                        new BsonDocument
                                        {
                                            { "case", new BsonDocument("$gte", new BsonArray { "$age", 51 })},
                                            { "then", "50+" }
                                        }
                                    }},
                                { "default", "Unknown" }
                            })
                        },
                        { "hometown", "$hometown" }
                    }
                },
                { "userCount", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "Id", new BsonDocument("$toString", "$_id.destinationId") },
                { "LocationName", "$_id.locationName" },
                { "AgeGroup", "$_id.ageGroup" },
                { "Hometown", "$_id.hometown" },
                { "UserCount", "$userCount" }
            })
        };

        try
        {

            var rawAnalytics = await _collection.Aggregate<BsonDocument>(pipeline, null, cancellationToken).ToListAsync();
            if (!rawAnalytics.Any())
            {
                Console.WriteLine("No analytics data found after full pipeline.");
            }

            return rawAnalytics.Select(b => new DestinationUserDemographics
            {
                Id = b.GetValue("Id", "").AsString,
                LocationName = b.GetValue("LocationName", "Unknown").AsString,
                AgeGroup = b.GetValue("AgeGroup", "Unknown").AsString,
                Hometown = b.GetValue("Hometown", "Unknown").AsString,
                UserCount = b.GetValue("UserCount", 0).ToInt64()
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserDemographicsAsync: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    public async Task<Pagination<TouristDestination>> GetTouristDestination(TouristDestinationSpecParams touristDestinationSpecParams, CancellationToken cancellationToken = default)
    {
        var builder = Builders<TouristDestination>.Filter;
        var filter = builder.Eq(x => x.status, true);
        if (!string.IsNullOrEmpty(touristDestinationSpecParams.Search))
        {
            var searchFilter = builder.Regex(x => x.Name, new BsonRegularExpression(touristDestinationSpecParams.Search, "i"));
            filter &= searchFilter;
        }

        if (!string.IsNullOrEmpty(touristDestinationSpecParams.Sort))
        {
            return new Pagination<TouristDestination>
            {
                PageSize = touristDestinationSpecParams.PageSize,
                PageIndex = touristDestinationSpecParams.PageIndex,
                Data = await DataFilter(touristDestinationSpecParams, filter, cancellationToken),
                Count = await _collection.CountDocumentsAsync(Builders<TouristDestination>.Filter.Empty, cancellationToken: cancellationToken)
            };
        }
        return new Pagination<TouristDestination>
        {
            PageSize = touristDestinationSpecParams.PageSize,
            PageIndex = touristDestinationSpecParams.PageIndex,
            Data = await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Ascending("Name"))
                    .Skip(touristDestinationSpecParams.PageSize * (touristDestinationSpecParams.PageIndex - 1))
                    .Limit(touristDestinationSpecParams.PageSize)
                    .ToListAsync(cancellationToken),
            Count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken)
        };
    }

    public async Task<bool> PlusFavorite(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.Id, id);
        var update = Builders<TouristDestination>.Update.Inc(p => p.FavoriteCount, 1);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    private async Task<IReadOnlyList<TouristDestination>> DataFilter(TouristDestinationSpecParams catalogSpecParams, FilterDefinition<TouristDestination> filter, CancellationToken cancellationToken = default)
    {
        switch (catalogSpecParams.Sort)
        {
            // favorite
            case "favoriteAsc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Ascending("favoriteCount"))
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
            case "favoriteDesc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Descending("favoriteCount"))
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
            // rating
            case "ratingAsc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Ascending("avarageRating"))
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
            case "ratingDesc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Descending("avarageRating"))
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
            // name
            case "nameAsc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Ascending("name"))
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
            case "nameDesc":
                return await _collection
                    .Find(filter)
                    .Sort(Builders<TouristDestination>.Sort.Descending("name"))
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
            default:
                return await _collection
                    .Find(filter)
                    .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
                    .Limit(catalogSpecParams.PageSize)
                    .ToListAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<DestinationAnalytics>> CompareDestinationsAsync(IEnumerable<string> destinationIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = await GetDestinationStatsOverviewAsync(timeRange, startDate, endDate, cancellationToken);
        var idSet = destinationIds.ToHashSet() ?? new HashSet<string>();

        var result = analytics.DestinationDetails.Where(p => idSet.Contains(p.Id)).ToList();

        return result;
    }

    public async Task<IEnumerable<TouristDestination>> GetDestinationsByIds(List<string> idList, CancellationToken cancellationToken = default)
    {
        if (idList == null || idList.Count == 0)
            return Enumerable.Empty<TouristDestination>();

        var filter = Builders<TouristDestination>.Filter.In(d => d.Id, idList);
        var destinations = await _collection.Find(filter).ToListAsync(cancellationToken);
        return destinations;
    }

    public async Task<bool> MinusFavorite(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TouristDestination>.Filter.Eq(p => p.Id, id);
        var update = Builders<TouristDestination>.Update.Inc(p => p.FavoriteCount, -1);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }
}
