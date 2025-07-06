// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TraVinhMaps.Application.External;

namespace TraVinhMaps.Application.Features.Review;
public class ImageManagementReviewServices
{
    private readonly ICloudinaryService _cloudinaryService;
    public ImageManagementReviewServices(ICloudinaryService cloudinaryService)
    {
        this._cloudinaryService = cloudinaryService;
    }
    public async Task<List<String>> AddImageReview(List<IFormFile> files)
    {
        var images = new List<String>();

        if (files == null || files.Count == 0)
        {
            return images;
        }
        if (files.Count > 5)
        {
            throw new InvalidOperationException("You can upload a maximum of 5 images.");
        }
        var allowedContentTypes = new[]
        {
            "image/png",
            "image/jpg",
            "image/jpeg",
            "application/octet-stream"
        };

        foreach (IFormFile file in files)
        {
            if (file == null || file.Length == 0)
            {
                continue;
            }
            var contentType = file.ContentType.ToLower().Split(';')[0].Trim();
            var fileName = file.FileName.ToLower();

            var isValidExtension =
                fileName.EndsWith(".jpg") ||
                fileName.EndsWith(".jpeg") ||
                fileName.EndsWith(".png");

            if (!isValidExtension || !allowedContentTypes.Contains(contentType))
            {
                throw new InvalidOperationException($"File '{file.FileName}' has invalid format.");
            }
            using (var stream = file.OpenReadStream())
            {
                var imageResult = await this._cloudinaryService.UploadImageAsync(file);
                images.Add(imageResult.SecureUrl.ToString());
            }
        }
        return images;
    }
}
