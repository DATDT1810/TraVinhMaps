// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Company.Interface;
using TraVinhMaps.Application.Features.Markers.Interface;
using TraVinhMaps.Application.Features.OcopProduct;
using TraVinhMaps.Application.Features.OcopProduct.Interface;
using TraVinhMaps.Application.Features.OcopProduct.Mappers;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Application.Features.OcopType.Interface;
using TraVinhMaps.Domain.Entities;
using static StackExchange.Redis.Role;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OcopProductController : ControllerBase
{
    private readonly IOcopProductService _service;
    private readonly IMarkerService _markerService;
    private readonly ImageManagementOcopProductServices _imageManagementOcopProductServices;
    private readonly ICompanyService _companyService;
    private readonly IOcopTypeService _ocopTypeService;
    public OcopProductController(IOcopProductService service, ImageManagementOcopProductServices imageManagementOcopProductServices, IMarkerService markerService, ICompanyService companyService, IOcopTypeService _ocopTypeService)
    {
        _service = service;
        _imageManagementOcopProductServices = imageManagementOcopProductServices;
        _markerService = markerService;
        _companyService = companyService;
        this._ocopTypeService = _ocopTypeService;
    }

    [HttpGet]
    [Route("GetAllOcopProduct")]
    public async Task<IActionResult> GetAllOcopProduct()
    {
        var listOcopProduct = await _service.ListAllAsync();
        return this.ApiOk(listOcopProduct);
    }

    [HttpGet]
    [Route("GetActiveOcopProduct")]
    public async Task<IActionResult> GetActiveOcopProduct()
    {
        var listOcopProduct = await _service.ListActiveAsync();
        var companies = await _companyService.ListAllAsync();
        var companyMap = companies.ToDictionary(c => c.Id, c => c);
        var response = listOcopProduct.Select(product =>
        {
            companyMap.TryGetValue(product.CompanyId, out var company);

            return new OcopProductResponse
            {
                Id = product.Id,
                CreatedAt = product.CreatedAt,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductImage = product.ProductImage,
                ProductPrice = product.ProductPrice,
                OcopTypeId = product.OcopTypeId,
                Status = product.Status,
                UpdateAt = product.UpdateAt,
                Sellocations = product.Sellocations,
                CompanyId = product.CompanyId,
                OcopPoint = product.OcopPoint,
                OcopYearRelease = product.OcopYearRelease,
                TagId = product.TagId,
                company = company != null
                    ? new CompanyDto
                    {
                        Id = company.Id,
                        Name = company.Name
                    }
                    : new CompanyDto
                    {
                        Id = string.Empty,
                        Name = "Unknown"
                    }
            };
        }).ToList();

        return this.ApiOk(response);
    }


    [HttpGet]
    [Route("GetOcopProductById/{id}", Name = "GetOcopProductById")]
    public async Task<IActionResult> GetOcopProductById(string id)
    {
        var ocopProduct = await _service.GetByIdAsync(id);
        return this.ApiOk(ocopProduct);
    }
    [HttpGet]
    [Route("GetOcopProductByOcopTypeId/{id}")]
    public async Task<IActionResult> GetOcopProductByOcopTypeId(string id)
    {
        var listOcopProduct = await _service.GetOcopProductByOcopTypeId(id);
        return this.ApiOk(listOcopProduct);
    }
    [HttpGet]
    [Route("GetOcopProductByCompanyId/{id}")]
    public async Task<IActionResult> GetOcopProductByCompanyId(string id)
    {
        var listOcopProduct = await _service.GetOcopProductByCompanyId(id);
        return this.ApiOk(listOcopProduct);
    }
    [HttpGet]
    [Route("CountOcopProducts")]
    public async Task<IActionResult> CountOcopProducts()
    {
        var countOcopProducts = await _service.CountAsync();
        return this.ApiOk(countOcopProducts);
    }
    [HttpPost]
    [Route("AddOcopProduct")]
    public async Task<IActionResult> AddOcopProduct([FromForm] CreateOcopProductRequest createOcopProductRequest)
    {
        if (createOcopProductRequest.ProductImageFile == null || createOcopProductRequest.ProductImageFile.Count == 0)
        {
            return this.ApiError("Ocop product attractions must have at least 1 photo");
        }
        var exitingOcopProduct = await _service.GetOcopProductByName(createOcopProductRequest.ProductName);
        if (exitingOcopProduct != null)
        {
            return this.ApiError("Ocop product name already exists.");
        }
        var imageFile = await _imageManagementOcopProductServices.AddImageOcopProduct(createOcopProductRequest.ProductImageFile);
        if (imageFile == null) { throw new NotFoundException("No valid image uploaded."); }

        var createOcopProduct = OcopProductMapper.Mapper.Map<OcopProduct>(createOcopProductRequest);
        
        createOcopProduct.Status = true;
        var ocopProducts = await _service.AddAsync(createOcopProduct);
        foreach (var item in imageFile)
        {
            await this._service.AddImageOcopProduct(ocopProducts.Id, item);
        }
        return CreatedAtRoute("GetOcopProductById", new { id = ocopProducts.Id }, this.ApiOk(ocopProducts));
    }
    [HttpPost]
    [Route("AddImageOcopProduct")]
    public async Task<IActionResult> AddImageOcopProduct([FromForm] AddImageRequest addImageRequest)
    {
        if (addImageRequest.imageFile == null && !addImageRequest.imageFile.Any())
        {
            return this.ApiError("Please upload at least one image.");
        }
        var linkImage = await _imageManagementOcopProductServices.AddImageOcopProduct(addImageRequest.imageFile);
        if (linkImage == null || !linkImage.Any())
        {
            return this.ApiError("Image upload failed.");
        }
        var ocopProduct = await _service.GetByIdAsync(addImageRequest.id);
        if (ocopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        if (ocopProduct.ProductImage == null)
        {
            ocopProduct.ProductImage = new List<string>();
        }
        ocopProduct.ProductImage.AddRange(linkImage);
        await _service.UpdateAsync(ocopProduct);
        return this.ApiOk(ocopProduct.ProductImage);
    }
    [HttpDelete]
    [Route("DeleteImageOcopProduct/{id}/{imageUrl}")]
    public async Task<IActionResult> DeleteImageOcopProduct(string id, string imageUrl)
    {
        var decodedImageUrl = Uri.UnescapeDataString(imageUrl);
        var ocopProduct = await _service.GetByIdAsync(id);
        if (ocopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        if (ocopProduct.ProductImage == null || !ocopProduct.ProductImage.Contains(decodedImageUrl))
        {
            return this.ApiError("Image URL not found in product.");
        }
        ocopProduct.ProductImage.Remove(decodedImageUrl);
        await _service.UpdateAsync(ocopProduct);
        return this.ApiOk("Image of ocop product deleted successfully.");
    }
    [HttpPut]
    [Route("UpdateOcopProduct")]
    public async Task<IActionResult> UpdateOcopProduct([FromBody] UpdateOcopProductRequest updateOcopProductRequest)
    {
        var existingProduct = await _service.GetByIdAsync(updateOcopProductRequest.Id);
        if (existingProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }

        existingProduct.ProductName = updateOcopProductRequest.ProductName;
        existingProduct.ProductDescription = updateOcopProductRequest.ProductDescription;
        existingProduct.ProductPrice = updateOcopProductRequest.ProductPrice;
        existingProduct.OcopTypeId = updateOcopProductRequest.OcopTypeId;
        existingProduct.CompanyId = updateOcopProductRequest.CompanyId;
        existingProduct.OcopPoint = updateOcopProductRequest.OcopPoint;
        existingProduct.OcopYearRelease = updateOcopProductRequest.OcopYearRelease;
        existingProduct.TagId = updateOcopProductRequest.TagId;
        if (updateOcopProductRequest.UpdateAt.HasValue)
        {
            existingProduct.UpdateAt = updateOcopProductRequest.UpdateAt.Value;
        }
        await _service.UpdateAsync(existingProduct);
        return this.ApiOk("Updated ocop product successfully.");
    }

    [HttpDelete]
    [Route("DeleteOcopProduct/{id}")]
    public async Task<IActionResult> DeleteOcopProduct(string id)
    {
        var ocopProduct = await _service.GetByIdAsync(id);
        if (ocopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        await _service.DeleteOcopProductAsync(id);
        return this.ApiOk("Ocop product deleted successfully.");
    }

    [HttpPut]
    [Route("RestoreOcopProduct/{id}")]
    public async Task<IActionResult> RestoreOcopProduct(string id)
    {
        var restoreOcopProduct = await _service.GetByIdAsync(id);
        if (restoreOcopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        await _service.RestoreOcopProductAsync(id);
        return this.ApiOk("Ocop product restored successfully.");
    }
    [HttpPost]
    [Route("AddSellLocation")]
    public async Task<IActionResult> AddSellLocation([FromBody] CreateSellLocationRequest sellLocation)
    {
        var ocopProduct = await _service.GetByIdAsync(sellLocation.Id);
        if (ocopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        var mapSellLocation = OcopProductMapper.Mapper.Map<SellLocation>(sellLocation);
        var maker = await _markerService.GetAsyns(p => p.Name == "Sell Location");
        if (maker == null)
        {
            throw new NotFoundException("Maker is not found.");
        }
        mapSellLocation.MarkerId = maker.Id;
        var addSellLocation = await _service.AddSellLocation(sellLocation.Id, mapSellLocation);
        return this.ApiOk(addSellLocation);
    }
    [HttpPut]
    [Route("UpdateSellLocation")]
    public async Task<IActionResult> UpdateSellLocation([FromBody] UpdateSellLocationRequest sellLocation)
    {
        var ocopProduct = await _service.GetByIdAsync(sellLocation.Id);
        if (ocopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        var mapSellLocation = OcopProductMapper.Mapper.Map<SellLocation>(sellLocation);
        var addSellLocation = await _service.UpdateSellLocation(sellLocation.Id, mapSellLocation);
        return this.ApiOk(addSellLocation);
    }
    [HttpDelete]
    [Route("DeleteSellLocation/{id}/{name}")]
    public async Task<IActionResult> DeleteSellLocation(string id, string name)
    {
        var ocopProduct = await _service.GetByIdAsync(id);
        if (ocopProduct == null)
        {
            throw new NotFoundException("Ocop product not found.");
        }
        if (!ocopProduct.Sellocations.Any(n => n.LocationName == name))
        {
            throw new NotFoundException("Sell location not found.");
        }

        var deleteSellLocation = await _service.DeleteSellLocation(id, name);
        return this.ApiOk("Sell location deleted successfully.");
    }

    /*
     * Wrapping endpoint with tagId , ocoptypeId , companyId
     */
    [HttpGet("get-lookup-product")]
    public async Task<IActionResult> LooksUpForProduct()
    {
        var result = await _service.LooksUpForProduct();
        if (result == null)
        {
            return this.ApiError("No product found for lookup.");
        }
        return this.ApiOk(result);
    }

    [HttpPost("import-product")]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    public async Task<IActionResult> ImportOcopProduct(IFormCollection form)
    {
        var json = form["products"];
        if (string.IsNullOrWhiteSpace(json))
        {
            return this.ApiError("No products to import.");
        }
        var productDtos = JsonConvert.DeserializeObject<List<CreateOcopProductRequest>>(json);
        if (productDtos == null || !productDtos.Any())
        {
            return this.ApiError("No products to import.");
        }
        foreach (var key in form.Files.Select(f => f.Name).Distinct())
        {
            // Key có dạng "productImages[0]", "productImages[1]", ...
            if (key.StartsWith("productImages["))
            {
                var indexText = key.Replace("productImages[", "").Replace("]", "");
                if (int.TryParse(indexText, out var index) && index < productDtos.Count)
                {
                    var files = form.Files.GetFiles(key);
                    productDtos[index].ProductImageFile = files.ToList();
                }
            }
        }

        int result = 0;

        foreach (var productDto in productDtos)
        {
            if (productDto.ProductImageFile == null || !productDto.ProductImageFile.Any())
            {
                return this.ApiError("Each product must have at least one image.");
            }
            var imageFiles = await _imageManagementOcopProductServices.AddImageOcopProduct(productDto.ProductImageFile);
            if (imageFiles == null || !imageFiles.Any())
            {
                return this.ApiError("Failed to upload product images.");
            }
            productDto.ProductImageFile = null; // Clear the file list after processing
            var ocopProduct = OcopProductMapper.Mapper.Map<OcopProduct>(productDto);
            ocopProduct.Status = true;
            var addedProduct = await _service.AddAsync(ocopProduct);
            foreach (var imageUrl in imageFiles)
            {
                await _service.AddImageOcopProduct(addedProduct.Id, imageUrl);
            }
            result++;
        }
        return this.ApiOk($"Successfully imported {result} products.");
    }

    // Analytics
    // format date: yyyy-mm-dd
    [HttpGet("analytics")]
    public async Task<IActionResult> GetProductAnalytics(
    [FromQuery] string timeRange = "month",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _service.GetProductAnalyticsAsync(timeRange, startDate, endDate);
            if (!analytics.Any()) throw new NotFoundException("No analytics data available.");
            return this.ApiOk(analytics);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // format date: yyyy-mm-dd
    [HttpGet("analytics-userdemographics")]
    public async Task<IActionResult> GetUserDemographics(
    [FromQuery] string timeRange = "month",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _service.GetUserDemographicsAsync(timeRange, startDate, endDate);
            if (!analytics.Any()) throw new NotFoundException("No analytics data available.");
            return this.ApiOk(analytics);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);

        }
    }

    // format date: yyyy-mm-dd
    [HttpGet("analytics-getTopProductsByInteractions")]
    public async Task<IActionResult> GetTopProductsByInteractions(int top = 5,
    string timeRange = "month",
    DateTime? startDate = null,
    DateTime? endDate = null)
    {
        try
        {
            var analytics = await _service.GetTopProductsByInteractionsAsync(top, timeRange, startDate, endDate);
            if (!analytics.Any()) throw new NotFoundException("No analytics data available.");
            return this.ApiOk(analytics);
        }catch(Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // format date: yyyy-mm-dd
    [HttpGet("analytics-getTopProductsByFavorites")]
    public async Task<IActionResult> GetTopProductsByFavorites(int top = 5,
    string timeRange = "month",
    DateTime? startDate = null,
    DateTime? endDate = null)
    {
        try
        {
            var analytics = await _service.GetTopProductsByFavoritesAsync(top, timeRange, startDate, endDate);
            if (!analytics.Any()) throw new NotFoundException("No analytics data available.");
            return this.ApiOk(analytics);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    // format date: yyyy-mm-dd
    [HttpGet("analytics-compareproducts")]
    public async Task<IActionResult> CompareProducts([FromQuery] IEnumerable<string> productIds,
    [FromQuery] string timeRange = "month",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _service.CompareProductsAsync(productIds, timeRange, startDate, endDate);
            if (!analytics.Any()) throw new NotFoundException("No analytics data available.");
            return this.ApiOk(analytics);
        }
        catch (Exception ex)
        {
            var errorDetails = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.ToString(),
                timeRange,
                startDate,
                endDate
            };
            return StatusCode(500, errorDetails);
        }
    }

    [HttpPost]
    [Route("GetOcopProductsByIds")]
    public async Task<IActionResult> GetOcopProductsByIds([FromBody] List<string> listId)
    {
        return this.ApiOk(await _service.GetOcopProductsByIds(listId));
    }

    [HttpGet]
    [Route("GetCurrentOcopProduct")]
    public async Task<IActionResult> GetCurrentOcopProduct()
    {
        var ocopProducts = await _service.GetCurrentOcopProduct();
        if (ocopProducts == null || !ocopProducts.Any())
        {
            return this.ApiError("No current OCOP products found.");
        }
        var companies = await _companyService.ListAllAsync();
        var companyMap = companies.ToDictionary(c => c.Id, c => c);
        var response = ocopProducts.Select(product =>
        {
            companyMap.TryGetValue(product.CompanyId, out var company);

            return new OcopProductResponse
            {
                Id = product.Id,
                CreatedAt = product.CreatedAt,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductImage = product.ProductImage,
                ProductPrice = product.ProductPrice,
                OcopTypeId = product.OcopTypeId,
                Status = product.Status,
                UpdateAt = product.UpdateAt,
                Sellocations = product.Sellocations,
                CompanyId = product.CompanyId,
                OcopPoint = product.OcopPoint,
                OcopYearRelease = product.OcopYearRelease,
                TagId = product.TagId,
                company = company != null
                    ? new CompanyDto
                    {
                        Id = company.Id,
                        Name = company.Name
                    }
                    : new CompanyDto
                    {
                        Id = string.Empty,
                        Name = "Unknown"
                    }
            };
        }).ToList();

        return this.ApiOk(response);
    }

    [HttpGet]
    [Route("GetOcopProductWithTypeById/{id}", Name = "GetOcopProductWithTypeById")]
    public async Task<IActionResult> GetOcopProductWithTypeById(string id)
    {
        var ocopProduct = await _service.GetByIdAsync(id);
        if (ocopProduct == null)
        {
            return NotFound($"Ocop product with id '{id}' not found.");
        }

        var company = await _companyService.GetByIdAsync(ocopProduct.CompanyId);

        var ocopType = await _ocopTypeService.GetByIdAsync(ocopProduct.OcopTypeId);

        var response = new OcopProductResponse
        {
            Id = ocopProduct.Id,
            CreatedAt = ocopProduct.CreatedAt,
            ProductName = ocopProduct.ProductName,
            ProductDescription = ocopProduct.ProductDescription,
            ProductImage = ocopProduct.ProductImage,
            ProductPrice = ocopProduct.ProductPrice,
            OcopTypeId = ocopProduct.OcopTypeId,
            Status = ocopProduct.Status,
            UpdateAt = ocopProduct.UpdateAt,
            Sellocations = ocopProduct.Sellocations,
            CompanyId = ocopProduct.CompanyId,
            OcopPoint = ocopProduct.OcopPoint,
            OcopYearRelease = ocopProduct.OcopYearRelease,
            TagId = ocopProduct.TagId,
            company = company != null
                ? new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name
                }
                : new CompanyDto
                {
                    Id = string.Empty,
                    Name = "Unknown"
                },
            ocopType = ocopType != null
                ? new OcopTypeDto
                {
                    Id = ocopType.Id,
                    Name = ocopType.OcopTypeName,
                }
                : new OcopTypeDto
                {
                    Id = "Unknown",
                    Name = "Unknown",
                }
        };
        return this.ApiOk(response);
    }
}
