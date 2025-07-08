// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.SignalR;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.OcopProduct;
using TraVinhMaps.Application.Features.OcopProduct.Mappers;
using TraVinhMaps.Application.Features.OcopProduct.Models;
using TraVinhMaps.Application.Features.OcopType.Mappers;
using TraVinhMaps.Application.Features.OcopType.Models;
using TraVinhMaps.Application.Features.Review;
using TraVinhMaps.Application.Features.Review.Interface;
using TraVinhMaps.Application.Features.Review.Mappers;
using TraVinhMaps.Application.Features.Review.Models;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ImageManagementReviewServices _imageManagementReviewServices;
    private readonly IHubContext<DashboardHub> _hubContext;
    public ReviewController(IReviewService reviewService, IHubContext<DashboardHub> hubContext, ImageManagementReviewServices imageManagementReviewServices)
    {
        _reviewService = reviewService;
        _hubContext = hubContext;
        _imageManagementReviewServices = imageManagementReviewServices;
    }
    [HttpGet]
    [Route("GetAllReview")]
    public async Task<IActionResult> GetAllReview()
    {
        var listReview = await _reviewService.ListAllAsync();
        return this.ApiOk(listReview);
    }
    [HttpGet]
    [Route("GetReviewById/{id}", Name = "GetReviewById")]
    public async Task<IActionResult> GetReviewById(string id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        return this.ApiOk(review);
    }
    [HttpGet]
    [Route("FilterReviewsAsync", Name = "FilterReviewsAsync")]
    public async Task<IActionResult> FilterReviewsAsync(string? destinationId, int? rating, DateTime? startAt, DateTime? endAt)
    {
        try
        {
            var review = await _reviewService.FilterReviewsAsync(destinationId, rating, startAt, endAt);
            return this.ApiOk(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Fail detail",
                error = ex.ToString()
            });
        }
    }

    [HttpGet]
    [Route("CountReviews")]
    public async Task<IActionResult> CountReviews()
    {
        var countReviews = await _reviewService.CountAsync();
        return this.ApiOk(countReviews);
    }
    [HttpGet]
    [Route("GetLatestReviews")]
    public async Task<IActionResult> GetLatestReviews()
    {
        var listLatestReview = await _reviewService.GetLatestReviewsAsync();
        return this.ApiOk(listLatestReview);
    }
    [Authorize]
    [HttpPost("AddReview")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddReview([FromForm] CreateReviewRequest createReviewRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var imageFile = await _imageManagementReviewServices.AddImageReview(createReviewRequest.Images);
            if (imageFile == null) { throw new NotFoundException("No valid image uploaded."); }
            var review = await _reviewService.AddAsync(createReviewRequest, imageFile);
            await _hubContext.Clients.Group("admin").SendAsync("ReceiveFeedback", review.Id);
            await _hubContext.Clients.Group("super-admin").SendAsync("ReceiveFeedback", review.Id);
            return this.ApiOk(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing review", Error = ex.Message });
        }

    }
    [HttpDelete]
    [Route("DeleteReview/{id}")]
    public async Task<IActionResult> DeleteReview(string id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
        {
            throw new NotFoundException("Review not found.");
        }
        await _reviewService.DeleteAsync(review);
        return this.ApiOk("Review deleted successfully.");
    }
    [Authorize]
    [HttpPost("AddReply")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddReply([FromForm] CreateReplyRequest replyRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var review = await _reviewService.GetByIdAsync(replyRequest.Id);
            if (review == null)
                throw new NotFoundException("Review not found.");

            var addedReply = await _reviewService.AddReply(replyRequest.Id, replyRequest);

            await _hubContext.Clients.Group("admin").SendAsync("ReceiveReply", replyRequest.Id);
            await _hubContext.Clients.Group("super-admin").SendAsync("ReceiveReply", replyRequest.Id);

            return this.ApiOk(addedReply);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing reply", Error = ex.Message });
        }
    }
}
