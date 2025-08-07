// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Features.Roles.Interface;
using TraVinhMaps.Application.Features.Users;
using TraVinhMaps.Application.Features.Users.Interface;
using TraVinhMaps.Application.Features.Users.Mappers;
using TraVinhMaps.Application.Features.Users.Models;
using TraVinhMaps.Domain.Entities;


namespace TraVinhMaps.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UploadImageUser _uploadImageUser;
    private readonly IRoleService _roleService;
    private readonly IHubContext<DashboardHub> _hubContext;


    public UsersController(IUserService userService, UploadImageUser uploadImageUser, IRoleService roleService, IHubContext<DashboardHub> hubContext)
    {
        _userService = userService;
        _uploadImageUser = uploadImageUser;
        _roleService = roleService;
        _hubContext = hubContext;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var list = await _userService.ListAllAsync();
            return Ok(list ?? new List<User>());
        }
        catch (Exception)
        {
            return Ok(new List<User>());
        }
        
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveUsers()
    {
        var user = await _userService.ListAsync(u => u.Status == true);
        return Ok(user ?? new List<User>());
    }

    [HttpGet("inActive")]
    public async Task<IActionResult> GetInActiveUsers()
    {
        var inActiveUser = await _userService.ListAsync(u => u.Status == false);
        return Ok(inActiveUser);
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User not found!");
        }
        return Ok(user);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCountUsers()
    {
        var countUser = await _userService.CountAsync();
        return Ok(countUser);
    }

    [HttpGet("count-active")]
    public async Task<IActionResult> GetCountActiveUsers()
    {
        var countUser = await _userService.CountAsync(u => u.Status == true && u.IsForbidden == false);
        return Ok(countUser);
    }

    [HttpGet("count-inActive")]
    public async Task<IActionResult> GetCountInActiveUsers()
    {
        var countUser = await _userService.CountAsync(u => u.Status == false && u.IsForbidden == true);
        return Ok(countUser);
    }

    [HttpGet("count/banned")]
    public async Task<IActionResult> GetBannedUsersCount()
    {
        var banUserCount = await _userService.CountAsync(u => u.IsForbidden == true);
        return Ok(banUserCount);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        var createUser = await _userService.AddAsync(user);
        return CreatedAtRoute(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromForm] UserRequest userRequest, IFormFile? imageFile)
    {
        var existingUser = await _userService.GetByIdAsync(userRequest.Id);
        if (existingUser == null)
        {
            throw new NotFoundException("User not found!");
        }

        var updatedUser = UserMapper.Mapper.Map(userRequest, existingUser);

        if (imageFile != null)
        {
            var avatarUrl = await _uploadImageUser.UploadImage(imageFile);
            updatedUser.Profile = updatedUser.Profile ?? new Profile();
            updatedUser.Profile.Avatar = avatarUrl;
        }

        await _userService.UpdateAsync(updatedUser);
        return Ok(updatedUser);
    }

    [HttpDelete("LockUser/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userService.DeleteUser(id);
        if (user != false)
        {
            // SignalR notification to update charts
            await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
            return Ok(user);
        }
        throw new BadRequestException("Failed to lock account");
    }

    [HttpPut("RestoreUser/{id}")]
    public async Task<IActionResult> RestoreUser(string id)
    {
        var user = await _userService.RestoreUser(id);
        // SignalR notification to update charts
        await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
        return Ok(user);
    }

    [Authorize]
    [HttpGet("get-profile-admin")]
    public async Task<IActionResult> GetProfileAdmin()
    {
        var sessionId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sessionId))
        {
            return this.ApiError("Session ID not found in token", HttpStatusCode.Unauthorized);
        }
        var result = await _userService.GetProfileAdmin(sessionId);
        if (result == null)
        {
            return this.ApiError("User not found", HttpStatusCode.NotFound);
        }
        return this.ApiOk(result, "Get profile admin successfully");
    }
    [Authorize(Roles = "admin, super-admin")]
    [HttpPost("update-profile-admin")]
    public async Task<IActionResult> UpdateProfileAdmin([FromForm] UpdateProfileAdminRequest request)
    {
        if (request == null)
        {
            return this.ApiError("Update fail", HttpStatusCode.BadRequest);
        }
        await _userService.UpdateProfileAdmin(request);
        return this.ApiOk("", "Update profile admin successfully");
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStatistics([FromQuery] string groupBy = "all", [FromQuery] string timeRange = "month")
    {
        var stats = await _userService.GetUserStatisticsAsync(groupBy, timeRange);
        return this.ApiOk(stats, "User statistics retrieved successfully");
    }

    [HttpGet("performance-tags")]
    public async Task<IActionResult> GetPerformanceByTags(
    [FromQuery(Name = "tags")] List<string>? tags,      
    [FromQuery] bool includeOcop = true,
    [FromQuery] bool includeDestination = true,
    [FromQuery] bool includeLocalSpecialty = true,
    [FromQuery] bool includeTips = true,
    [FromQuery] bool includeFestivals = true,
    [FromQuery] string timeRange = "month",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.GetPerformanceByTagAsync(
                tagNames: tags,          
                includeOcop,
                includeDestination,
                includeLocalSpecialty,
                includeTips,
                includeFestivals,
                timeRange,
                startDate,
                endDate,
                cancellationToken);

            return this.ApiOk(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Internal server error." });
        }
    }

    [Authorize]
    [HttpGet("user-profile")]
    public async Task<IActionResult> Profile()
    {
        var sessionId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sessionId))
        {
            return this.ApiError("Session ID not found in token", HttpStatusCode.Unauthorized);
        }
        var result = await _userService.GetUserProfile(sessionId);
        if (result == null)
        {
            return this.ApiError("User not found", HttpStatusCode.NotFound);
        }

        return this.ApiOk(result, "Return User succesfully");
    }

    [Authorize]
    [HttpPut("update-user-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request, IFormFile? imageFile)
    {
        var sessionId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sessionId))
        {
            return this.ApiError("Session ID not found in token", HttpStatusCode.Unauthorized);
        }
        var existingUser = await _userService.GetByIdAsync(sessionId);
        if (existingUser == null)
        {
            return this.ApiError("User not found", HttpStatusCode.NotFound);
        }
        if (existingUser.Profile == null)
        {
            existingUser.Profile = new Profile();
        }
        existingUser.Profile.FullName = request.FullName;
        existingUser.Profile.DateOfBirth = DateOnly.Parse(request.DateOfBirth);
        existingUser.Profile.PhoneNumber = request.PhoneNumber;
        existingUser.Profile.Address = request.Address;
        existingUser.Profile.Gender = request.Gender;
        existingUser.Email = request.Email;

        if (imageFile != null)
        {
            var avatarUrl = await _uploadImageUser.UploadImage(imageFile);
            // existingUser.Profile = existingUser.Profile ?? new Profile();
            existingUser.Profile.Avatar = avatarUrl;
        }
        await _userService.UpdateAsync(existingUser);
        var result = new UserProfileResponse
        {
            Fullname = existingUser.Profile?.FullName,
            DateOfBirth = existingUser.Profile?.DateOfBirth,
            Phone = existingUser.Profile?.PhoneNumber,
            Address = existingUser.Profile?.Address,
            Avatar = existingUser.Profile?.Avatar,
            Email = existingUser.Email,
            Gender = existingUser.Profile?.Gender,
            HassedPassword = existingUser.Password,
        };
        return this.ApiOk(result, "Update profile successfully");
    }

    [Authorize]
    [HttpGet]
    [Route("GetFavoriteList")]
    public async Task<IActionResult> getFavoriteList()
    {
        return await _userService.getFavoriteUserList() is List<Favorite> favoriteList
            ? this.ApiOk(favoriteList, "Get favorite list successfully")
            : this.ApiError("Failed to get favorite list", HttpStatusCode.NotFound);
    }

    [Authorize]
    [HttpPost]
    [Route("AddItemToFavoriteList")]
    public async Task<IActionResult> addItemToFavoriteList([FromBody] FavoriteRequest favoriteRequest)
    {
        if (await _userService.addItemToFavoriteList(favoriteRequest))
        {
            return this.ApiOk("", "Add item to favorite list successfully");
        }
        return this.ApiError("Failed to add item to favorite list", HttpStatusCode.BadRequest);
    }

    [Authorize]
    [HttpDelete]
    [Route("RemoveItemToFavoriteList/{id}")]
    public async Task<IActionResult> removeItemToFavoriteList(string id)
    {
        if (await _userService.removeItemToFavoriteList(id))
        {
            return this.ApiOk("", "Remove item to favorite list successfully");
        }
        return this.ApiError("Failed to remove item from favorite list", HttpStatusCode.BadRequest);
    }
}
