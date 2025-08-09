// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using TraVinhMaps.Application.External;

namespace TraVinhMaps.Application.Features.OcopProduct;
public class ImageManagementOcopProductServices
{
    private readonly ICloudinaryService _cloudinaryService;
    public ImageManagementOcopProductServices(ICloudinaryService cloudinaryService)
    {
        this._cloudinaryService = cloudinaryService;
    }
    public async Task<List<String>> AddImageOcopProduct(List<IFormFile> files)
    {
        List<String> images = new List<String>();
        foreach (IFormFile file in files)
        {
            if (file.Length == 0 || files.Count == 0)
            {
                return null;
            }
            using (var stream = file.OpenReadStream())
            {
                var imageResult = await this._cloudinaryService.UploadImageAsync(file);
                images.Add(imageResult.SecureUrl.ToString());
            }
        }
        return images;
    }

    public async Task<bool> DeleteImageOCOP(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return false;

        try
        {
            var uri = new Uri(imageUrl);
            var fileName = uri.Segments.Last();
            var publicId = System.IO.Path.GetFileNameWithoutExtension(fileName);

            var result = await _cloudinaryService.DeleteImageAsync(publicId);
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
