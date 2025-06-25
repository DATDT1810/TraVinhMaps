// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Infrastructure.Db;

namespace TraVinhMaps.Infrastructure.CustomRepositories;
public class OcopProductRepository : BaseRepository<OcopProduct>, IOcopProductRepository
{
    private readonly IMongoCollection<Interaction> _interactionCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<InteractionLogs> _interactionLogsCollection;

    public OcopProductRepository(IDbContext dbContext) : base(dbContext)
    {
        _interactionCollection = dbContext.Database.GetCollection<Interaction>("Interaction");
        _userCollection = dbContext.Database.GetCollection<User>("User");
        _interactionLogsCollection = dbContext.Database.GetCollection<InteractionLogs>("InteractionLogs");

        // Tạo index để tối ưu hiệu suất
        _interactionCollection.Indexes.CreateOne(new CreateIndexModel<Interaction>(
            Builders<Interaction>.IndexKeys.Ascending(i => i.ItemId).Ascending(i => i.ItemType).Ascending(i => i.CreatedAt)));
        _userCollection.Indexes.CreateOne(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending("favorites.favorite_itemId").Ascending("favorites.favorite_type")));
        _interactionLogsCollection.Indexes.CreateOne(new CreateIndexModel<InteractionLogs>(
            Builders<InteractionLogs>.IndexKeys.Ascending(i => i.ItemId).Ascending(i => i.ItemType).Ascending(i => i.CreatedAt)));
    }
    public async Task<OcopProduct> GetOcopProductByName(string name, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(c => c.ProductName, name) & Builders<OcopProduct>.Filter.Eq(s => s.Status, true);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<IEnumerable<OcopProduct>> GetOcopProductByCompanyId(string companyId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(c => c.CompanyId, companyId) & Builders<OcopProduct>.Filter.Eq(s => s.Status, true);
        var ocopProduct = await _collection.Find(filter).ToListAsync();
        return ocopProduct;
    }

    public async Task<IEnumerable<OcopProduct>> GetOcopProductByOcopTypeId(string ocopTypeId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(c => c.OcopTypeId, ocopTypeId) & Builders<OcopProduct>.Filter.Eq(s => s.Status, true);
        var ocopProduct = await _collection.Find(filter).ToListAsync();
        return ocopProduct;
    }
    public async Task<bool> DeleteOcopProductAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(o => o.Id, id);
        var restore = Builders<OcopProduct>.Update.Set(r => r.Status, false);
        var restoreOcopProduct = await _collection.UpdateOneAsync(filter, restore);
        return restoreOcopProduct.IsAcknowledged && restoreOcopProduct.ModifiedCount < 0;
    }
    public async Task<bool> RestoreOcopProductAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(o => o.Id, id);
        var restore = Builders<OcopProduct>.Update.Set(r => r.Status, true);
        var restoreOcopProduct = await _collection.UpdateOneAsync(filter, restore);
        return restoreOcopProduct.IsAcknowledged && restoreOcopProduct.ModifiedCount < 0;
    }

    public async Task<string> AddImageOcopProduct(string id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(o => o.Id, id);
        var ocopProduct = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (ocopProduct == null) return null;
        if (ocopProduct.ProductImage == null)
        {
            var newListImage = Builders<OcopProduct>.Update.Set(im => im.ProductImage, new List<string>());
            await _collection.UpdateOneAsync(filter, newListImage);
        }
        var updateImage = Builders<OcopProduct>.Update.Push(p => p.ProductImage, imageUrl);
        var updateResult = await _collection.UpdateOneAsync(filter, updateImage, cancellationToken: cancellationToken);
        return imageUrl;
    }
    public async Task<SellLocation> AddSellLocation(string id, SellLocation sellLocation, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(o => o.Id, id);
        var ocopProduct = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (ocopProduct == null) return null;
        if (ocopProduct.Sellocations == null)
        {
            var newListSellLocation = Builders<OcopProduct>.Update.Set(im => im.Sellocations, new List<SellLocation>());
            await _collection.UpdateOneAsync(filter, newListSellLocation);
        }
        var updateSellLocaton = Builders<OcopProduct>.Update.Push(p => p.Sellocations, sellLocation);
        var updateResult = await _collection.UpdateOneAsync(filter, updateSellLocaton, cancellationToken: cancellationToken);
        return sellLocation;
    }

    public async Task<bool> DeleteSellLocation(string ocopProductId, string sellLocationName, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.And(Builders<OcopProduct>.Filter.Eq(o => o.Id, ocopProductId), Builders<OcopProduct>.Filter.ElemMatch(p => p.Sellocations, s => s.LocationName == sellLocationName));
        var update = Builders<OcopProduct>.Update.PullFilter(p => p.Sellocations, s => s.LocationName == sellLocationName);
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateSellLocation(string ocopProductId, SellLocation sellLocation, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OcopProduct>.Filter.Eq(o => o.Id, ocopProductId);
        var ocopProduct = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (ocopProduct == null || ocopProduct.Sellocations == null) return false;
        var index = ocopProduct.Sellocations.FindIndex(s => s.LocationName == sellLocation.LocationName);
        ocopProduct.Sellocations[index] = sellLocation;
        var update = Builders<OcopProduct>.Update.Set(o => o.Sellocations, ocopProduct.Sellocations);
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    // ViewCount: Số lượt xem(từ bảng InteractionLogs)

    // InteractionCount: Số lượng tương tác(từ bảng Interaction)

    // WishlistCount: Số lượt sản phẩm được thêm vào yêu thích(từ bảng User.favorites)
    public async Task<IEnumerable<OcopProductAnalytics>> GetProductAnalyticsAsync(
    string timeRange = "month",
    DateTime? startDate = null,
    DateTime? endDate = null,
    CancellationToken cancellationToken = default)
    {
        // Get the current UTC time
        var now = DateTime.UtcNow;

        DateTime filterStartDate;
        DateTime? filterEndDate = null;

        // Determine the filter range
        if (startDate.HasValue && endDate.HasValue)
        {
            // Use explicit date range if provided
            filterStartDate = startDate.Value;
            filterEndDate = endDate.Value;
        }
        else
        {
            // Use relative time range based on the selected timeRange
            filterStartDate = timeRange.ToLower() switch
            {
                "day" => now.AddDays(-1),
                "week" => now.AddDays(-7),
                "month" => new DateTime(now.Year, now.Month, 1),
                "year" => new DateTime(now.Year, 1, 1),
                _ => new DateTime(now.Year, now.Month, 1)
            };
        }

        // Convert C# DateTime to BsonValue for MongoDB usage
        var bsonStartDate = BsonValue.Create(filterStartDate);
        var bsonEndDate = filterEndDate.HasValue ? BsonValue.Create(filterEndDate.Value) : null;

        // Filter conditions for Interaction collection
        var interactionCond = new BsonArray
    {
        new BsonDocument("$eq", new BsonArray { "$$interaction.itemType", "Ocop Product" }),
        new BsonDocument("$gte", new BsonArray { "$$interaction.createdAt", bsonStartDate })
    };
        if (bsonEndDate != null)
        {
            interactionCond.Add(new BsonDocument("$lte", new BsonArray { "$$interaction.createdAt", bsonEndDate }));
        }

        // Filter conditions for InteractionLogs collection
        var logCond = new BsonArray
    {
        new BsonDocument("$eq", new BsonArray { "$$log.itemType", "Ocop Product" }),
        new BsonDocument("$gte", new BsonArray { "$$log.createdAt", bsonStartDate })
    };
        if (bsonEndDate != null)
        {
            logCond.Add(new BsonDocument("$lte", new BsonArray { "$$log.createdAt", bsonEndDate }));
        }

        // MongoDB aggregation pipeline to gather analytics
        var pipeline = new List<BsonDocument>
        {
            new("$match", new BsonDocument("status", true)),
            new("$project", new BsonDocument
            {
                { "_id", 1 },
                { "productName", 1 }
            }),
            new("$lookup", new BsonDocument
            {
                { "from", "Interaction" },
                { "localField", "_id" },
                { "foreignField", "itemId" },
                { "as", "interactionsMed" }
            }),
            new("$addFields", new BsonDocument("interactions",
                new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$interactionsMed" },
                    { "as", "interaction" },
                    { "cond", new BsonDocument("$and", interactionCond) }
                })
            )),
            new("$lookup", new BsonDocument
            {
                { "from", "InteractionLogs" },
                { "localField", "_id" },
                { "foreignField", "itemId" },
                { "as", "interactionLogsMed" }
            }),
            new("$addFields", new BsonDocument("interactionLogs",
                new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$interactionLogsMed" },
                    { "as", "log" },
                    { "cond", new BsonDocument("$and", logCond) }
                })
            )),
            new("$lookup", new BsonDocument
            {
                { "from", "User" },
                { "localField", "_id" },
                { "foreignField", "favorites.itemId" },
                { "as", "users" }
            }),
            new("$project", new BsonDocument
            {
                { "Id", new BsonDocument("$toString", "$_id") },
                { "ProductName", "$productName" },
                { "ViewCount", new BsonDocument("$size", "$interactionLogs") },
                { "InteractionCount", new BsonDocument("$sum", "$interactions.totalCount") },
                { "WishlistCount", new BsonDocument("$size",
                    new BsonDocument("$reduce",
                        new BsonDocument
                        {
                            { "input", "$users" },
                            { "initialValue", new BsonArray() },
                            { "in", new BsonDocument("$concatArrays",
                                new BsonArray
                                {
                                    "$$value",
                                    new BsonDocument("$filter",
                                        new BsonDocument
                                        {
                                            { "input", "$$this.favorites" },
                                            { "as", "fav" },
                                            { "cond", new BsonDocument("$eq",
                                                new BsonArray { "$$fav.itemId", "$_id" })
                                            }
                                        }
                                    )
                                }
                            )}
                        }
                    )
                )}
            })
        };

        // Execute pipeline and retrieve results
        var rawAnalytics = await _collection.Aggregate<BsonDocument>(pipeline, null, cancellationToken).ToListAsync();

        // Map raw Bson results to strongly-typed DTOs
        return rawAnalytics.Select(b => new OcopProductAnalytics
        {
            Id = b.GetValue("Id", "").AsString,
            ProductName = b.GetValue("ProductName", "").AsString,
            ViewCount = b.GetValue("ViewCount", 0).ToInt64(),
            InteractionCount = b.GetValue("InteractionCount", 0).ToInt64(),
            WishlistCount = b.GetValue("WishlistCount", 0).ToInt64()
        });
    }

    public async Task<IEnumerable<OcopProductUserDemographics>> GetUserDemographicsAsync(
    string timeRange = "month",
    DateTime? startDate = null,
    DateTime? endDate = null,
    CancellationToken cancellationToken = default)
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
            filterEndDate = now; // Ensure endDate covers the data range
        }

        var bsonStartDate = BsonValue.Create(filterStartDate);
        var bsonEndDate = BsonValue.Create(filterEndDate.Value);

        var interactionCond = new BsonArray
    {
        new BsonDocument("$eq", new BsonArray { "$$interaction.itemType", "Ocop Product" }),
        new BsonDocument("$gte", new BsonArray { "$$interaction.createdAt", bsonStartDate }),
        new BsonDocument("$lte", new BsonArray { "$$interaction.createdAt", bsonEndDate })
    };

        var logCond = new BsonArray
    {
        new BsonDocument("$eq", new BsonArray { "$$log.itemType", "Ocop Product" }),
        new BsonDocument("$gte", new BsonArray { "$$log.createdAt", bsonStartDate }),
        new BsonDocument("$lte", new BsonArray { "$$log.createdAt", bsonEndDate })
    };

        var pipeline = new List<BsonDocument>
    {
        // Step 1: Match active products
        new BsonDocument("$match", new BsonDocument("status", true)),

        // Step 2: Project essential fields
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 1 },
            { "productName", new BsonDocument("$ifNull", new BsonArray { "$productName", "Unknown" }) }
        }),

        // Step 3: Lookup interactions
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Interaction" },
            { "localField", "_id" },
            { "foreignField", "itemId" },
            { "as", "interactions" }
        }),

        // Step 4: Filter interactions
        new BsonDocument("$addFields", new BsonDocument("interactions",
            new BsonDocument("$filter", new BsonDocument
            {
                { "input", "$interactions" },
                { "as", "interaction" },
                { "cond", new BsonDocument("$and", interactionCond) }
            })
        )),

        // Step 5: Lookup interaction logs
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "InteractionLogs" },
            { "localField", "_id" },
            { "foreignField", "itemId" },
            { "as", "interactionLogs" }
        }),

        // Step 6: Filter interaction logs
        new BsonDocument("$addFields", new BsonDocument("interactionLogs",
            new BsonDocument("$filter", new BsonDocument
            {
                { "input", "$interactionLogs" },
                { "as", "log" },
                { "cond", new BsonDocument("$and", logCond) }
            })
        )),

        // Step 7: Lookup users for interactions
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

        // Step 8: Lookup users for interaction logs
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

        // Step 9: Lookup users for wishlist
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "User" },
            { "localField", "_id" },
            { "foreignField", "favorites.itemId" },
            { "as", "wishlistUsers" }
        }),

        // Step 10: Combine all users
        new BsonDocument("$addFields", new BsonDocument
        {
            { "allUsers", new BsonDocument("$setUnion", new BsonArray
            {
                new BsonDocument("$ifNull", new BsonArray { "$interactionUsers", new BsonArray() }),
                new BsonDocument("$ifNull", new BsonArray { "$logUsers", new BsonArray() }),
                new BsonDocument("$ifNull", new BsonArray { "$wishlistUsers", new BsonArray() })
            })
            }
        }),

        // Step 11: Debug - Log intermediate result
        new BsonDocument("$addFields", new BsonDocument
        {
            { "debug", new BsonDocument
            {
                { "interactionsCount", new BsonDocument("$size", "$interactions") },
                { "interactionLogsCount", new BsonDocument("$size", "$interactionLogs") },
                { "wishlistUsersCount", new BsonDocument("$size", "$wishlistUsers") },
                { "allUsersCount", new BsonDocument("$size", "$allUsers") }
            }
            }
        }),

        // Step 12: Filter products with users
        new BsonDocument("$match", new BsonDocument
        {
            { "allUsers.0", new BsonDocument("$exists", true) }
        }),

        // Step 13: Unwind allUsers
        new BsonDocument("$unwind", new BsonDocument
        {
            { "path", "$allUsers" },
            { "preserveNullAndEmptyArrays", false }
        }),

        // Step 14: Calculate age and standardize hometown
        new BsonDocument("$addFields", new BsonDocument
        {
            { "age", new BsonDocument("$cond", new BsonDocument
            {
                { "if", new BsonDocument("$and", new BsonArray
                {
                    new BsonDocument("$ne", new BsonArray { "$allUsers.profile.dateOfBirth", BsonNull.Value }),
                    new BsonDocument("$ne", new BsonArray { "$allUsers.profile.dateOfBirth", "" })
                }) },
                { "then", new BsonDocument("$floor", new BsonDocument("$divide",
                    new BsonArray
                    {
                        new BsonDocument("$subtract",
                            new BsonArray
                            {
                                new BsonDocument("$toDate", now),
                                "$allUsers.profile.dateOfBirth"
                            }),
                        31557600000 // milliseconds in a year (365.25 days)
                    })) },
                { "else", -1 }
            }) },
            { "hometown", new BsonDocument("$cond", new BsonDocument
            {
                { "if", new BsonDocument("$and", new BsonArray
                {
                    new BsonDocument("$ne", new BsonArray { "$allUsers.profile.address", BsonNull.Value }),
                    new BsonDocument("$ne", new BsonArray { "$allUsers.profile.address", "" })
                }) },
                { "then", new BsonDocument("$trim", new BsonDocument
                {
                    { "input", new BsonDocument("$arrayElemAt",
                        new BsonArray
                        {
                            new BsonDocument("$split", new BsonArray { "$allUsers.profile.address", "," }),
                            -1
                        })
                    }
                }) },
                { "else", "Unknown" }
            }) }
        }),

        // Step 15: Group by product, age group, and hometown
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
            {
                { "productId", "$_id" },
                { "productName", "$productName" },
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
                            }) },
                            { "then", "0-18" }
                        },
                        new BsonDocument
                        {
                            { "case", new BsonDocument("$and", new BsonArray
                            {
                                new BsonDocument("$gte", new BsonArray { "$age", 19 }),
                                new BsonDocument("$lte", new BsonArray { "$age", 30 })
                            }) },
                            { "then", "18-30" }
                        },
                        new BsonDocument
                        {
                            { "case", new BsonDocument("$and", new BsonArray
                            {
                                new BsonDocument("$gte", new BsonArray { "$age", 31 }),
                                new BsonDocument("$lte", new BsonArray { "$age", 50 })
                            }) },
                            { "then", "30-50" }
                        },
                        new BsonDocument
                        {
                            { "case", new BsonDocument("$gte", new BsonArray { "$age", 51 }) },
                            { "then", "50+" }
                        }
                    } },
                    { "default", "Unknown" }
                }) },
                { "hometown", "$hometown" }
            }
            },
            { "userCount", new BsonDocument("$sum", 1) }
        }),

        // Step 16: Final projection
        new BsonDocument("$project", new BsonDocument
        {
            { "Id", new BsonDocument("$toString", "$_id.productId") },
            { "ProductName", "$_id.productName" },
            { "AgeGroup", "$_id.ageGroup" },
            { "Hometown", "$_id.hometown" },
            { "UserCount", "$userCount" }
        })
    };

        try
        {
            // Debug: Log intermediate results
            var debugResult = await _collection.Aggregate<BsonDocument>(pipeline.Take(11).ToList(), null, cancellationToken).ToListAsync();
            Console.WriteLine($"Debug - Products after step 11: {debugResult.Count}");
            foreach (var doc in debugResult)
            {
                Console.WriteLine($"Product: {doc["_id"]}, Interactions: {doc["debug"]["interactionsCount"]}, Logs: {doc["debug"]["interactionLogsCount"]}, Wishlist: {doc["debug"]["wishlistUsersCount"]}, AllUsers: {doc["debug"]["allUsersCount"]}");
            }

            var rawAnalytics = await _collection.Aggregate<BsonDocument>(pipeline, null, cancellationToken).ToListAsync();
            if (!rawAnalytics.Any())
            {
                Console.WriteLine("NoėjusNo analytics data found after full pipeline.");
            }

            return rawAnalytics.Select(b => new OcopProductUserDemographics
            {
                Id = b.GetValue("Id", "").AsString,
                ProductName = b.GetValue("ProductName", "Unknown").AsString,
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

    public async Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByInteractionsAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = await GetProductAnalyticsAsync(timeRange, startDate, endDate, cancellationToken);
        return analytics.OrderByDescending(p => p.InteractionCount)
            .ThenByDescending(p => p.ViewCount)
            .ThenBy(p => p.ProductName)
            .Take(top)
            .ToList();
    }

    public async Task<IEnumerable<OcopProductAnalytics>> GetTopProductsByFavoritesAsync(int top = 5, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = await GetProductAnalyticsAsync(timeRange, startDate, endDate, cancellationToken);
        // sort desc by WishlistCount, get top
        return analytics.OrderByDescending(p => p.WishlistCount)
            .ThenByDescending(p => p.ViewCount)
            .ThenBy(p => p.ProductName)
            .Take(top)
            .ToList();
    }

    public async Task<IEnumerable<OcopProductAnalytics>> CompareProductsAsync(IEnumerable<string> productIds, string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = await GetProductAnalyticsAsync(timeRange,startDate,endDate, cancellationToken);
        // Retrieve only the list of products to compare, preserving the input order.
        var idSet = productIds.ToHashSet() ?? new HashSet<string>();
        var result = analytics.Where(p => idSet.Contains(p.Id)).ToList();
        // Sort by the order of productIds if necessary
        // var orderMap = productIds?.Select((id, idx) => new { id, idx }).ToDictionary(x => x.id, x => x.idx) ?? new Dictionary<string, int>();
        // return result.OrderBy(p => orderMap.TryGetValue(p.Id, out var idx) ? idx : int.MaxValue).ToList();
        return result;
    }
}
