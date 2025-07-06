// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TraVinhMaps.Application.Common.Extensions;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Infrastructure.Db.Data;

public static class DataSeeding
{

    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IHost>>();
        var mongoDbSettings = serviceProvider.GetRequiredService<IOptions<MongoDbSetting>>().Value;

        try
        {
            var client = new MongoClient(mongoDbSettings.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.DatabaseName);

            //await SeedOcopProducts(database, logger);
            await SeedTagSample(database, logger);
            var markers = await seedMarker(database, logger);
            await SeedRole(database, logger);
            await SeedUserAccount(database, logger);
            await SeedCompany(database, logger);
            await SeedOcopType(database, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    //    private static async Task SeedOcopProducts(IMongoDatabase database, ILogger<IHost> logger)
    //    {
    //        var collection = database.GetCollection<OcopProduct>("OcopProduct");
    //        var count = await collection.CountDocumentsAsync(FilterDefinition<OcopProduct>.Empty);
    //        if (count > 0)
    //        {
    //            logger.LogInformation("OcopProducts collection already contains data. Skipping seeding.");
    //            return;
    //        }

    //        logger.LogInformation("Seeding OcopProducts collection...");

    //        // Create some sample OcopTypes to reference
    //        var ocopTypeId = await SeedOcopType(database);
    //        var companyId = await SeedCompany(database);
    //        var tagId = await SeedTag(database);
    //        var product = new List<OcopProduct>
    //        {
    //          new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Tra Vinh Coconut Candy",
    //    ProductDescription = "Traditional coconut candy with unique Tra Vinh flavor.",
    //    ProductImage = new List<string>
    //    {
    //          "https://example.com/coconut-candy.jpg",
    //                "https://example.com/candy-pack.jpg"
    //    },
    //    ProductPrice = "35000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cua hang dac san so 3",
    //            LocationAddress = "45 Le Loi, Phuong 1, TP Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.350, 9.950 }
    //            }
    //        },
    //        new SellLocation
    //        {
    //            LocationName = "Cho Tra Vinh",
    //            LocationAddress = "78 Nguyen Thi Minh Khai, Phuong 2",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.342, 9.948 }
    //            }
    //        },
    //    },
    //    OcopPoint = 5,
    //    OcopYearRelease = 2022,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Tra Vinh Palm Sugar",
    //    ProductDescription = "Organic palm sugar sourced from Tra Vinh's palm trees. Known for its rich caramel flavor and natural sweetness.",
    //     ProductImage = new List<string> {
    //                "https://example.com/coconut-candy.jpg",
    //                "https://example.com/candy-pack.jpg"
    //            },
    //    ProductPrice = "55000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cua hang duong thot not",
    //            LocationAddress = "123 Tran Phu, Phuong 3, TP Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.355, 9.952 }
    //            }
    //        },
    //        new SellLocation
    //        {
    //            LocationName = "Sieu thi Co.op Mart Tra Vinh",
    //            LocationAddress = "56 Nguyen Dang, TP Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.349, 9.947 }
    //            }
    //        },
    //        new SellLocation
    //        {
    //            LocationName = "Cua hang dac san Tra Vinh",
    //            LocationAddress = "22 Pham Thai Buong, Phuong 4",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.340, 9.955 }
    //            }
    //        },
    //    },
    //    OcopPoint = 3,
    //    OcopYearRelease = 2023,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Tra Vinh Dried Fish",
    //    ProductDescription = "Premium quality dried fish produced with traditional methods from Tra Vinh's coastal communities.",
    // ProductImage = new List<string> {
    //                "https://example.com/coconut-candy.jpg",
    //                "https://example.com/candy-pack.jpg"
    //            },
    //    ProductPrice = "85000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cua hang hai san Tra Vinh",
    //            LocationAddress = "67 Le Duan, TP Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.347, 9.949 }
    //            }
    //        },
    //        new SellLocation
    //        {
    //            LocationName = "Cho hai san Tra Vinh",
    //            LocationAddress = "34 Dinh Tien Hoang, Phuong 2",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.338, 9.944 }
    //            }
    //        },
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2021,
    //}, new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Tra Vinh Honey",
    //    ProductDescription = "Natural forest honey harvested from mangrove forests in Tra Vinh.",
    //    ProductImage = new List<string> { "https://example.com/honey.jpg" },
    //    ProductPrice = "120000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cơ sở mật ong rừng Hòa Minh",
    //            LocationAddress = "Hòa Minh, Châu Thành, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.354, 9.927 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2022,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Virgin Coconut Oil",
    //    ProductDescription = "Cold-pressed virgin coconut oil from Tra Vinh’s Ben Tre coconut groves.",
    //    ProductImage = new List<string> { "https://example.com/coconut-oil.jpg" },
    //    ProductPrice = "90000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cửa hàng dầu dừa Thanh Thủy",
    //            LocationAddress = "Ap My Hoa, Cau Ke, Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.285, 9.938 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2021,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Purple Sticky Rice Wine",
    //    ProductDescription = "Traditional fermented wine made from local purple sticky rice.",
    //    ProductImage = new List<string> { "https://example.com/purple-wine.jpg" },
    //    ProductPrice = "150000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Làng nghề rượu Phước Hưng",
    //            LocationAddress = "Phước Hưng, Trà Cú, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.310, 9.846 }
    //            }
    //        }
    //    },
    //    OcopPoint = 5,
    //    OcopYearRelease = 2023,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Banana Wine",
    //    ProductDescription = "Unique wine made from fermented bananas using Khmer techniques.",
    //    ProductImage = new List<string> { "https://example.com/banana-wine.jpg" },
    //    ProductPrice = "130000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cơ sở sản xuất rượu chuối",
    //            LocationAddress = "Ap Long Hoa, Cau Ngang, Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.386, 9.850 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2022,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Mangrove Pickled Fish",
    //    ProductDescription = "Fermented fish delicacy using traditional methods from mangrove regions.",
    //    ProductImage = new List<string> { "https://example.com/pickled-fish.jpg" },
    //    ProductPrice = "70000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Chợ cá Ba Động",
    //            LocationAddress = "Ba Động, Duyen Hai, Tra Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.495, 9.667 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2021,
    //},
    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Khô Cá Lóc Trà Vinh",
    //    ProductDescription = "Khô cá lóc truyền thống, phơi nắng tự nhiên và tẩm ướp gia vị đặc trưng.",
    //    ProductImage = new List<string> { "https://example.com/kho-ca-loc.jpg" },
    //    ProductPrice = "85000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cơ sở khô cá Hai Tâm",
    //            LocationAddress = "Huyện Tiểu Cần, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.343, 9.834 }
    //            }
    //        }
    //    },
    //    OcopPoint = 3,
    //    OcopYearRelease = 2020,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Bánh Tét Trà Cuốn Lá Cẩm",
    //    ProductDescription = "Bánh tét nhân đậu xanh thịt ba rọi, nấu bằng lá cẩm tạo màu tím bắt mắt.",
    //    ProductImage = new List<string> { "https://example.com/banh-tet-la-cam.jpg" },
    //    ProductPrice = "50000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Hợp tác xã bánh tét Trà Cú",
    //            LocationAddress = "Trà Cú, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.300, 9.796 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2021,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Trà Gừng Mật Ong",
    //    ProductDescription = "Trà gừng kết hợp mật ong tự nhiên, giúp tăng cường sức khỏe và kháng khuẩn.",
    //    ProductImage = new List<string> { "https://example.com/tra-gung.jpg" },
    //    ProductPrice = "65000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "HTX nông sản sạch Long Đức",
    //            LocationAddress = "Phường Long Đức, TP Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.328, 9.920 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2023,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Tôm Khô Cầu Ngang",
    //    ProductDescription = "Tôm khô đỏ au, ngọt tự nhiên, được phơi nắng và đóng gói sạch sẽ.",
    //    ProductImage = new List<string> { "https://example.com/tom-kho.jpg" },
    //    ProductPrice = "250000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cơ sở tôm khô Bảy Lựu",
    //            LocationAddress = "Cầu Ngang, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.379, 9.842 }
    //            }
    //        }
    //    },
    //    OcopPoint = 5,
    //    OcopYearRelease = 2022,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Bánh Tráng Dừa",
    //    ProductDescription = "Bánh tráng truyền thống kết hợp dừa nạo sấy khô, thơm ngon và giòn.",
    //    ProductImage = new List<string> { "https://example.com/banh-trang-dua.jpg" },
    //    ProductPrice = "40000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "HTX Bánh Tráng Trà Vinh",
    //            LocationAddress = "Huyện Càng Long, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.288, 9.999 }
    //            }
    //        }
    //    },
    //    OcopPoint = 3,
    //    OcopYearRelease = 2021,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Khô Mực Một Nắng",
    //    ProductDescription = "Mực tươi được phơi một nắng, giữ được độ dai và ngọt tự nhiên.",
    //    ProductImage = new List<string> { "https://example.com/kho-muc.jpg" },
    //    ProductPrice = "320000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Chợ hải sản Duyên Hải",
    //            LocationAddress = "Duyên Hải, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.490, 9.677 }
    //            }
    //        }
    //    },
    //    OcopPoint = 5,
    //    OcopYearRelease = 2023,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Trái Nhàu Sấy Khô",
    //    ProductDescription = "Trái nhàu tươi được sấy lạnh giữ nguyên hoạt chất có lợi cho sức khỏe.",
    //    ProductImage = new List<string> { "https://example.com/trai-nhau.jpg" },
    //    ProductPrice = "100000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Công ty TNHH Trái Nhàu Trà Vinh",
    //            LocationAddress = "Phường 8, TP Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.330, 9.918 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2020,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Dưa Mắm Trà Vinh",
    //    ProductDescription = "Đặc sản dưa mắm dân dã, đậm đà, ăn kèm cơm trắng hoặc cháo trắng.",
    //    ProductImage = new List<string> { "https://example.com/dua-mam.jpg" },
    //    ProductPrice = "45000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Làng nghề dưa mắm",
    //            LocationAddress = "Xã Hòa Minh, Châu Thành, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.355, 9.925 }
    //            }
    //        }
    //    },
    //    OcopPoint = 3,
    //    OcopYearRelease = 2021,
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Mắm Cá Linh",
    //    ProductDescription = "Mắm cá linh đậm đà, được ủ theo phương pháp truyền thống miền Tây.",
    //    ProductImage = new List<string> { "https://example.com/mam-ca-linh.jpg" },
    //    ProductPrice = "75000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cơ sở mắm Út Hồng",
    //            LocationAddress = "Huyện Trà Cú, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.316, 9.802 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2020
    //},

    //new OcopProduct
    //{
    //    Id = ObjectId.GenerateNewId().ToString(),
    //    ProductName = "Nước Mắm Nhĩ Cá Cơm",
    //    ProductDescription = "Nước mắm nhĩ truyền thống, rút từ lần đầu tiên, đậm đà vị cá cơm.",
    //    ProductImage = new List<string> { "https://example.com/nuoc-mam.jpg" },
    //    ProductPrice = "60000",
    //    OcopTypeId = ocopTypeId,
    //    CompanyId = companyId,
    //    TagId = tagId,
    //    Status = true,
    //    CreatedAt = DateTime.UtcNow,
    //    Sellocations = new List<SellLocation>
    //    {
    //        new SellLocation
    //        {
    //            LocationName = "Cơ sở nước mắm Ba Động",
    //            LocationAddress = "Xã Trường Long Hòa, Duyên Hải, Trà Vinh",
    //            Location = new Location
    //            {
    //                Type = "point",
    //                Coordinates = new List<double> { 106.496, 9.670 }
    //            }
    //        }
    //    },
    //    OcopPoint = 4,
    //    OcopYearRelease = 2022,
    //},

    //            };

    //        await collection.InsertManyAsync(product);
    //    }

    private static async Task SeedTagSample(IMongoDatabase database, ILogger<IHost> logger)
    {
        var collection = database.GetCollection<Tags>("Tags");
        var count = await collection.CountDocumentsAsync(FilterDefinition<Tags>.Empty);
        if (count > 0)
        {
            logger.LogInformation("Tags collection already contains data. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding Tag collection...");

        var Tags = new List<Tags>()
        {
            new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Introduce",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/coconut-tree_htkwqe.png",
                CreatedAt = DateTime.UtcNow,
            },
            new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Ocop",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206105/plantingtree_oyhi7m.png",
                CreatedAt = DateTime.UtcNow,
            },
            new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Tip Travel",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/lightbulb_mdvz4n.png",
                CreatedAt = DateTime.UtcNow,
            },
            new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Destination",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/destination_n8kp0f.png",
                CreatedAt = DateTime.UtcNow,
            },
            new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Local specialty",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/food_v6izlc.png",
                CreatedAt = DateTime.UtcNow,
            },
            new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Stay",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/hotel_rzhyqr.png",
                CreatedAt = DateTime.UtcNow,
            },
             new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Festivals",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/dragon-boat_vwsqix.png",
                CreatedAt = DateTime.UtcNow,
            },
              new Tags
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Utilities",
                image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206099/resource-allocation_gpmfgq.png",
                CreatedAt = DateTime.UtcNow,
            },
        };
        await collection.InsertManyAsync(Tags);
    }

    private static async Task<List<Marker>> seedMarker(IMongoDatabase database, ILogger<IHost> logger)
    {
        var collection = database.GetCollection<Marker>("Marker");
        var count = await collection.CountDocumentsAsync(FilterDefinition<Marker>.Empty);
        if (count > 0)
        {
            logger.LogInformation("Tags collection already contains data. Skipping seeding.");
            return await collection.Find(FilterDefinition<Marker>.Empty).ToListAsync();
        }
        logger.LogInformation("Seeding Tag collection...");

        var markers = new List<Marker>()
        {
            new Marker
            {
                Id= ObjectId.GenerateNewId().ToString(),
                Name = "Buiding Destination",
                Image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747207304/building_aczwaz.png",
                Status = true,
                CreatedAt = DateTime.UtcNow,
            },
            new Marker
            {
                Id= ObjectId.GenerateNewId().ToString(),
                Name = "Natural Destination",
                Image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747207304/park_fa2ttf.png",
                Status = true,
                CreatedAt = DateTime.UtcNow,
            },
            new Marker
            {
                Id= ObjectId.GenerateNewId().ToString(),
                Name = "Local Specialties",
                Image = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1748795520/food_1_zkhxsb.png",
                Status = true,
                CreatedAt = DateTime.UtcNow,
            },
            new Marker
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Ocop Product",
                Image =  "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1748795520/ocop_1_ghqj3c.png",
                Status = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        await collection.InsertManyAsync(markers);
        return markers;
    }

    // Role seeding method
    private static async Task SeedRole(IMongoDatabase database, ILogger<IHost> logger)
    {
        var collection = database.GetCollection<Role>("Role");
        var count = await collection.CountDocumentsAsync(FilterDefinition<Role>.Empty);
        if (count > 0)
        {
            logger.LogInformation("Role collection already contains data. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding Role collection...");

        var role = new List<Role>()
        {
            new Role
            {
                Id = ObjectId.GenerateNewId().ToString(),
                RoleName = "super-admin",
                RoleStatus = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                Id = ObjectId.GenerateNewId().ToString(),
                RoleName = "admin",
                RoleStatus = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                Id = ObjectId.GenerateNewId().ToString(),
                RoleName = "user",
                RoleStatus = true,
                CreatedAt = DateTime.Now
            }
        };

        await collection.InsertManyAsync(role);
    }

    private static async Task SeedOcopType(IMongoDatabase database, ILogger<IHost> logger)
    {
        var collection = database.GetCollection<OcopType>("OcopType");
        var count = await collection.CountDocumentsAsync(FilterDefinition<OcopType>.Empty);
        if (count > 0)
        {
            logger.LogInformation("Role collection already contains data. Skipping seeding.");
            return;
        }
        var ocopTypeList = new List<OcopType>()
        {
            new OcopType
            {
                Id =  ObjectId.GenerateNewId().ToString(),
                OcopTypeStatus = true,
                CreatedAt = DateTime.UtcNow,
                OcopTypeName = "Thực Phẩm"
            },
            new OcopType
            {
                Id =  ObjectId.GenerateNewId().ToString(),
                OcopTypeStatus = true,
                CreatedAt = DateTime.UtcNow,
                OcopTypeName = "Đồ Uống"
            },
            new OcopType
            {
                Id =  ObjectId.GenerateNewId().ToString(),
                OcopTypeStatus = true,
                CreatedAt = DateTime.UtcNow,
                OcopTypeName = "Thảo Dược"
            },
            new OcopType
            {
                Id =  ObjectId.GenerateNewId().ToString(),
                OcopTypeStatus = true,
                CreatedAt = DateTime.UtcNow,
                OcopTypeName = "Đồ Thủ Công"
            },
            new OcopType
            {
                Id =  ObjectId.GenerateNewId().ToString(),
                OcopTypeStatus = true,
                CreatedAt = DateTime.UtcNow,
                OcopTypeName = "May Mặc"
            }
        };

        logger.LogInformation("Seeding Role collection...");
        await collection.InsertManyAsync(ocopTypeList);
    }

    // seeding method account user
    private static async Task SeedUserAccount(IMongoDatabase database, ILogger<IHost> logger)
    {
        var collection = database.GetCollection<User>("User");
        var count = await collection.CountDocumentsAsync(FilterDefinition<User>.Empty);
        if (count > 0)
        {
            logger.LogInformation("Role collection already contains data. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding Role collection...");

        var role = await database.GetCollection<Role>("Role")
            .Find(r => r.RoleName == "admin")
            .FirstOrDefaultAsync();

        var roleUser = await database.GetCollection<Role>("Role")
            .Find(r => r.RoleName == "user")
            .FirstOrDefaultAsync();

        if (role == null)
        {
            logger.LogError("Role 'admin' not found. Cannot seed user accounts.");
            return;
        }
        var userList = new List<User>()
        {
            new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Email = "joonnguyen2712@gmail.com",
                Password = HashingExtension.HashWithSHA256("27122003"),
                CreatedAt = DateTime.UtcNow,
                RoleId = role.Id, // Replace with actual role ID
                Status = true,
                IsForbidden = false,
            },
 new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Email = "haoncce171957@fpt.edu.vn",
                Password = HashingExtension.HashWithSHA256("27122003"),
                CreatedAt = DateTime.UtcNow,
                RoleId = role.Id, // Replace with actual role ID
                Status = true,
                IsForbidden = false,
            },
             new User
             {
                 Id = ObjectId.GenerateNewId().ToString(),
                 PhoneNumber = "0869251053",
                 Password = HashingExtension.HashWithSHA256("27122003"),
                 RoleId    = roleUser.Id, // Replace with actual role ID
                 CreatedAt = DateTime.UtcNow,
                 Status = true,
                 IsForbidden = false,
             },
             new User
             {
                 Id = ObjectId.GenerateNewId().ToString(),
                 Email = "nguyen.van.a@gmail.com",
                 PhoneNumber = "0869251053",
                 Password = HashingExtension.HashWithSHA256("27122003"),
                 RoleId    = roleUser.Id, // Replace with actual role ID
                 CreatedAt = DateTime.UtcNow,
                 Status = true,
                 IsForbidden = false,
                 Profile = new Profile{
                     FullName = "Nguyen Van A",
                     Address = "123 Nguyen Trai, TP Tra Vinh",
                     DateOfBirth = new DateOnly(2023, 12 , 27),
                     Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
                     Gender = "Male",
                     PhoneNumber = "0869251053",
                 }
             },
             new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "ly.ngoc.diep@gmail.com",
        PhoneNumber = "01869592738",
        Password = HashingExtension.HashWithSHA256("LNDiep20"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Lý Ngọc Diệp",
            Address = "21/159 Châu Văn Liêm, P. Tân An, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2020, 6, 20),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "01869592738"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "tran.ngoc.uyen@gmail.com",
        PhoneNumber = "0299387905",
        Password = HashingExtension.HashWithSHA256("TNUyen71"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Trần Ngọc Uyên",
            Address = "71A/313 3/2, P. Hưng Lợi, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1971, 7, 24),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0299387905"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.huynh.thi.cat.ly@gmail.com",
        PhoneNumber = "08681180207",
        Password = HashingExtension.HashWithSHA256("NHTCLy20170113"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Huỳnh Thị Cát Ly",
            Address = "104 Nguyễn Thị Minh Khai, P. An Lạc, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2017, 1, 13),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "08681180207"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.uyen.tho@gmail.com",
        PhoneNumber = "01235746520",
        Password = HashingExtension.HashWithSHA256("NUTho1991"),
        RoleId = role.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Uyên Thơ",
            Address = "388 Lý Tự Trọng, P. An Cư, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1991, 5, 12),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "01235746520"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.kim.anh@gmail.com",
        PhoneNumber = "0255330903",
        Password = HashingExtension.HashWithSHA256("NKAnh1994"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Kim Ánh",
            Address = "343 QL1, P. Thường Thạnh, Q. Cái Răng, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1994, 10, 19),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0255330903"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "tran.xuan.linh@gmail.com",
        PhoneNumber = "0367821456",
        Password = HashingExtension.HashWithSHA256("TXLinh1986"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Trần Xuân Linh",
            Address = "66 Nguyễn Trãi, P. An Hội, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1986, 8, 14),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0367821456"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "phan.diem.hoa@gmail.com",
        PhoneNumber = "0923847109",
        Password = HashingExtension.HashWithSHA256("PDHoa1977"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Phan Diễm Hoa",
            Address = "115 Nguyễn Văn Cừ, P. An Khánh, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1977, 9, 9),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0923847109"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "le.duy.khoa@gmail.com",
        PhoneNumber = "0703187612",
        Password = HashingExtension.HashWithSHA256("LDKhoa2000"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Lê Duy Khoa",
            Address = "97 Nguyễn Thị Định, P. An Bình, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2000, 3, 22),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0703187612"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "ngo.thi.phuong.thy@gmail.com",
        PhoneNumber = "0834562349",
        Password = HashingExtension.HashWithSHA256("NTPThy1998"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Ngô Thị Phương Thy",
            Address = "212A Lý Tự Trọng, P. An Cư, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1998, 11, 30),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0834562349"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.huu.minh@gmail.com",
        PhoneNumber = "0912345678",
        Password = HashingExtension.HashWithSHA256("NHMinh1995"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Hữu Minh",
            Address = "456 Trần Hưng Đạo, P. An Nghiệp, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1995, 2, 15),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0912345678"
        }
    },
     new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "do.thi.van@gmail.com",
        PhoneNumber = "0909090901",
        Password = HashingExtension.HashWithSHA256("DTVan1993"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Đỗ Thị Vân",
            Address = "12A Nguyễn Văn Linh, P. Hưng Lợi, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1993, 3, 18),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0909090901"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.le.nam@gmail.com",
        PhoneNumber = "0911122233",
        Password = HashingExtension.HashWithSHA256("NLNam1990"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Lê Nam",
            Address = "54 Trần Quang Diệu, P. An Khánh, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1990, 4, 9),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0911122233"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "le.diem.hang@gmail.com",
        PhoneNumber = "0966554433",
        Password = HashingExtension.HashWithSHA256("LDHang2002"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Lê Diễm Hằng",
            Address = "100A Trần Văn Hoài, P. Xuân Khánh, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2002, 6, 5),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0966554433"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "tran.duy.phuc@gmail.com",
        PhoneNumber = "0933445566",
        Password = HashingExtension.HashWithSHA256("TDPhuc1988"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Trần Duy Phúc",
            Address = "22/4 Nguyễn Việt Hồng, P. An Phú, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1988, 8, 23),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0933445566"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "huynh.phuong.tuyen@gmail.com",
        PhoneNumber = "0855778899",
        Password = HashingExtension.HashWithSHA256("HPTuyen1996"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Huỳnh Phương Tuyền",
            Address = "59 Phạm Ngọc Thạch, P. Cái Khế, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1996, 12, 7),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0855778899"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "vo.thanh.nghia@gmail.com",
        PhoneNumber = "0977889900",
        Password = HashingExtension.HashWithSHA256("VTNghia1992"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Võ Thanh Nghĩa",
            Address = "134C Lê Hồng Phong, P. Bình Thủy, Q. Bình Thủy, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1992, 9, 15),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0977889900"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.minh.tam@gmail.com",
        PhoneNumber = "0822233344",
        Password = HashingExtension.HashWithSHA256("NMTam1999"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Minh Tâm",
            Address = "5 Lê Văn Sỹ, P. An Hòa, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1999, 7, 1),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0822233344"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "dang.thi.hoa@gmail.com",
        PhoneNumber = "0900111222",
        Password = HashingExtension.HashWithSHA256("DTHoa1997"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Đặng Thị Hoa",
            Address = "21 Võ Văn Tần, P. Xuân Khánh, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1997, 3, 29),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0900111222"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "huynh.tan.phong@gmail.com",
        PhoneNumber = "0944332211",
        Password = HashingExtension.HashWithSHA256("HTPhong1985"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Huỳnh Tấn Phong",
            Address = "38 Hoàng Văn Thụ, P. An Hội, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1985, 6, 18),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0944332211"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "tran.minh.tuan@gmail.com",
        PhoneNumber = "0988001122",
        Password = HashingExtension.HashWithSHA256("TMTuan1991"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Trần Minh Tuấn",
            Address = "9 Nguyễn Văn Linh, P. Hưng Lợi, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1991, 8, 12),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0988001122"
        }
    },

    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "tran.thi.giang.thanh@gmail.com",
        PhoneNumber = "0911185473",
        Password = HashingExtension.HashWithSHA256("TTGThanh2008"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Trần Thị Giang Thanh",
            Address = "108 Trần Hưng Đạo, P. An Nghiệp, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2008, 1, 28),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0911185473"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "nguyen.tuyet.hong@gmail.com",
        PhoneNumber = "0971936626",
        Password = HashingExtension.HashWithSHA256("NTHong1970"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Nguyễn Tuyết Hồng",
            Address = "123 QL91, P. Thới Hòa, Q. Ô Môn, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1970, 2, 12),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "0971936626"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "ha.van.vinh@gmail.com",
        PhoneNumber = "0929701068",
        Password = HashingExtension.HashWithSHA256("HVVinh2010"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Hà Văn Vĩnh",
            Address = "117 QL91, P. Thới Hòa, Q. Ô Môn, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2010, 8, 26),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "0929701068"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "tran.thi.thu.huong@gmail.com",
        PhoneNumber = "01286421145",
        Password = HashingExtension.HashWithSHA256("TTTHuong2023"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Trần Thị Thu Hường",
            Address = "280 Nguyễn An Ninh, P. Tân An, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(2023, 1, 24),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Female",
            PhoneNumber = "01286421145"
        }
    },
    new User
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = "mac.nang@gmail.com",
        PhoneNumber = "01282034321",
        Password = HashingExtension.HashWithSHA256("MNang1977"),
        RoleId = roleUser.Id,
        CreatedAt = DateTime.UtcNow,
        Status = true,
        IsForbidden = false,
        Profile = new Profile
        {
            FullName = "Mạc Năng",
            Address = "303 Trần Ngọc Quế, P. Hưng Lợi, Q. Ninh Kiều, TP. Cần Thơ",
            DateOfBirth = new DateOnly(1977, 11, 9),
            Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
            Gender = "Male",
            PhoneNumber = "01282034321"
        }
    },
   new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    Email = "pham.le.tinh.nhi@gmail.com",
    PhoneNumber = "0921140398",
    Password = HashingExtension.HashWithSHA256("PLTNhi1981"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Phạm Lê Tịnh Nhi",
        Address = "181/146 3/2, P. An Bình, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1981, 2, 5),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0921140398"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    Email = "tran.thi.xuan@gmail.com",
    PhoneNumber = "01284892052",
    Password = HashingExtension.HashWithSHA256("TTXuan05"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Trần Thị Xuân",
        Address = "30/202 3/2, P. An Bình, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2005, 3, 29),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "01284892052"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    Email = "nguyen.thi.thuong.huyen@gmail.com",
    PhoneNumber = "0898705356",
    Password = HashingExtension.HashWithSHA256("NTTHuyen2009"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Thị Thương Huyền",
        Address = "145 Lý Tự Trọng, P. An Cư, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2009, 11, 4),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0898705356"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0220348599",
    Email = "LPQuyen19730517@gmail.com",
    Password = HashingExtension.HashWithSHA256("LPQuyen19730517"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Lê Phương Quyên",
        Address = "388 Châu Văn Liêm, P. An Lạc, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1973, 5, 17),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0220348599"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01286029966",
    Email = "LDAn01052022@gmail.com",
    Password = HashingExtension.HashWithSHA256("LDAn01052022"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Lê Ðức Ân",
        Address = "205 Lê Lợi, P. Cái Khế, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2022, 5, 1),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "01286029966"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01632658094",
    Email = "trannguyen2111@gmail.com" ,
    Password = HashingExtension.HashWithSHA256("NTran1904"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Trân",
        Address = "234 Đặng Thanh Sử, P. Phước Thới, Q. Ô Môn, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1999, 4, 19),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "01632658094"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01625991428",
    Email = "NgduongPhi@gmail.com",
    Password = HashingExtension.HashWithSHA256("NDPhi200701"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Ðức Phi",
        Address = "38/319 Bùi Hữu Nghĩa, P. Long Hòa , Q. Bình Thủy, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2020, 7, 1),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "01625991428"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0259357353",
    Email = "LtThuan@gmail.com",
    Password = HashingExtension.HashWithSHA256("LTThuan2612"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Lê Thanh Thuận",
        Address = "105 , TP. Cần Thơ",
        DateOfBirth = new DateOnly(2019, 12, 26),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "0259357353"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01293340031",
    Email = "TuongBX@gmail.com",
    Password = HashingExtension.HashWithSHA256("BXTuong11071982"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Bùi Xuân Tường",
        Address = "48A/139 Ngô Quyền, P. An Cư, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1982, 7, 11),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "01293340031"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01203992900",
    Email = "DiepVanThuan@gmail.com",
    Password = HashingExtension.HashWithSHA256("NVDiep18"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Văn Ðiệp",
        Address = "369 QL1, P. Hưng Phú, Q. Cái Răng, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2018, 12, 28),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "01203992900"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0225385178",
    Email = "LyThuanChau@gmail.com",
    Password = HashingExtension.HashWithSHA256("NHChau10022009"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Hoàn Châu",
        Address = "363 Trương Văn Diễn, P. Phước Thới, Q. Ô Môn, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2009, 2, 10),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0225385178"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0275324963",
    Email = "ThanhNgan@gmail.com",
    Password = HashingExtension.HashWithSHA256("NTTNgan84"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Thị Thúy Ngân",
        Address = "147 Ngô Quyền, P. An Cư, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1984, 5, 31),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0275324963"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0270356081",
    Email = "ViPhi@gmail.com",
    Password = HashingExtension.HashWithSHA256("VPPhi241218"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Võ Phương Phi",
        Address = "132 Nguyễn Chí Thanh, P. Trà Nóc , Q. Bình Thủy, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2024, 12, 18),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "0270356081"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "09985839608",
    Email = "AnhTai@gmail.com",
    Password = HashingExtension.HashWithSHA256("TATai1976"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Tạ Anh Tài",
        Address = "247 Châu Văn Liêm, P. An Lạc, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1976, 3, 24),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "09985839608"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0232313724",
    Email = "NqQuung@gmail.com",
    Password = HashingExtension.HashWithSHA256("NNQuynh1208"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Như Quỳnh",
        Address = "64 Phan Văn Trị, P. An Phú, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2009, 8, 12),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0232313724"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0235320278",
    Email = "NHVThuy2912@gmail.com",
    Password = HashingExtension.HashWithSHA256("NHVThuy2912"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Nguyễn Huỳnh Vân Thúy",
        Address = "155/369 Nguyễn Trãi, P. Cái Khế, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2003, 12, 29),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0235320278"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0220313740",
    Email = "TTTPhuong12011986@gmail.com",
    Password = HashingExtension.HashWithSHA256("TTTPhuong12011986"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Trang Thị Thanh Phương",
        Address = "62/202 Xô Viết Nghệ Tĩnh, P. An Cư, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1986, 1, 12),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0220313740"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "0911402437",
    Email = "LNTLan2006@gmail.com",
    Password = HashingExtension.HashWithSHA256("LNTLan2006"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Lý Nguyễn Thanh Lan",
        Address = "70B/120 Nguyễn Trãi, P. An Hội, Q. Ninh Kiều, TP. Cần Thơ",
        DateOfBirth = new DateOnly(2006, 6, 13),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "0911402437"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01670236071",
    Email = "Phung.Gaming@gmail.com",
    Password = HashingExtension.HashWithSHA256("PHung110170"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Phạm Hưng",
        Address = "71/319 Bùi Hữu Nghĩa, P. Bình Thủy, Q. Bình Thủy, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1970, 1, 11),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Male",
        PhoneNumber = "01670236071"
    }
},
new User
{
    Id = ObjectId.GenerateNewId().ToString(),
    PhoneNumber = "01640032288",
    Email = "LNTLTuyen14091996@gmail.com",
    Password = HashingExtension.HashWithSHA256("LNTLTuyen14091996"),
    RoleId = roleUser.Id,
    CreatedAt = DateTime.UtcNow,
    Status = true,
    IsForbidden = false,
    Profile = new Profile
    {
        FullName = "Lý Nguyễn Thị Lâm Tuyền",
        Address = "111 Nguyễn Đệ, P. An Thới, Q. Bình Thủy, TP. Cần Thơ",
        DateOfBirth = new DateOnly(1996, 9, 14),
        Avatar = "https://res.cloudinary.com/ddaj2hsk5/image/upload/v1747206098/avatar_default.png",
        Gender = "Female",
        PhoneNumber = "01640032288"
    }
},
    };

        await collection.InsertManyAsync(userList);
    }

    private static async Task SeedCompany(IMongoDatabase database, ILogger<IHost> logger)
    {
        var collection = database.GetCollection<Company>("Company");
        var count = await collection.CountDocumentsAsync(FilterDefinition<Company>.Empty);
        if (count > 0)
        {
            logger.LogInformation("Role collection already contains data. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding Role collection...");

        var companyList = new List<Company>
        {
            new Company
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Công ty TNHH ABC",
                Address = "123 Đường ABC, Quận 1, TP. Hồ Chí Minh",
                CreatedAt = DateTime.UtcNow,
                Locations = new List<Location>
                {
                    new Location
                    {
                        Type = "Point",
                        Coordinates = new List<double> { 106.6297, 10.7769 }, // Example coordinates for Ho Chi Minh City
                    },
                    new Location
                    {
                       Type = "Point",
                        Coordinates = new List<double> { 106.6297, 10.7769 }, // Example coordinates for Ho Chi Minh City
                    }
                },
                Contact = new Contact
                {
                    Email = "ABCFirm@gmail.com",
                    Phone = "0123456789",
                    Website = "https://www.example.com",
                }
            },
            new Company
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Công ty TNHH XYZ",
                Address = "456 Đường XYZ, Quận 2, TP. Hồ Chí Minh",
                CreatedAt = DateTime.UtcNow,
                Locations = new List<Location>
                {
                    new Location
                    {
                        Type = "Point",
                        Coordinates = new List<double> { 106.6297, 10.7769 }, // Example coordinates for Ho Chi Minh City
                    },
                    new Location
                    {
                        Type = "Point",
                        Coordinates = new List<double> { 106.6297, 10.7769 }, // Example coordinates for Ho Chi Minh City
                    }
                },
                Contact = new Contact
                {
                    Email = "XyzFimr@gmail.com",
                    Phone = "0987654321",
                    Website = "https://www.example.com",
                }
        },
            new Company{
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Công ty TNHH DEF",
                Address = "789 Đường DEF, Quận 3, TP. Hồ Chí Minh",
                CreatedAt = DateTime.UtcNow,
                Locations = new List<Location>
                {
                    new Location
                    {
                        Type = "Point",
                        Coordinates = new List<double> { 106.6297, 10.7769 }, // Example coordinates for Ho Chi Minh City
                    },
                    new Location
                    {
                        Type = "Point",
                        Coordinates = new List<double> { 106.6297, 10.7769 }, // Example coordinates for Ho Chi Minh City
                    }
                },
                Contact = new Contact
                {
                    Email = "DEFFim@gmail.com",
                    Phone = "0987654321",
                    Website = "https://www.example.com",

        }
            }

        };
        await collection.InsertManyAsync(companyList);
    }

    private static async Task<string> SeedTag(IMongoDatabase database)
    {
        return ObjectId.GenerateNewId().ToString();
    }

    private static async Task<string> SeedCompany(IMongoDatabase database)
    {
        return ObjectId.GenerateNewId().ToString();
    }

    private static async Task<string> SeedOcopType(IMongoDatabase database)
    {
        return ObjectId.GenerateNewId().ToString();
    }

}

