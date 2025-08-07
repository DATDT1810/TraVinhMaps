// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Common.Extensions;
using TraVinhMaps.Application.Features.Admins.Interface;
using TraVinhMaps.Application.Features.Admins.Models;

namespace TraVinhMaps.Api.Controllers;

// Controller for managing admin users via API
[Route("api/[controller]")]
[ApiController]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    // Constructor - injects the admin service
    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // GET: api/Admins/all
    // Retrieves a list of all admin users
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAdmin()
    {
        var adminUsers = await _adminService.ListAllAsync();
        return Ok(adminUsers);
    }

    // GET: api/Admins/{id}
    // Retrieves details of a specific admin user by their ID
    [HttpGet("{id}", Name = "GetAdminById")]
    public async Task<IActionResult> GetAdminById(string id)
    {
        var admin = await _adminService.GetByIdAsync(id);
        return Ok(admin);
    }

    // POST: api/Admins/create
    // Creates a new admin user
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] AdminRequest request)
    {
        try
        {
            var createdAdmin = await _adminService.AddAsync(request);
            // Returns 201 Created with route to the new admin resource
            return CreatedAtRoute(nameof(GetAdminById), new { id = createdAdmin.Id }, createdAdmin);
        }
        catch (InvalidOperationException ex)
        {
            // Handles business logic error (e.g., email already exists)
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Handles validation error (e.g., invalid or missing fields)
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Handles any other unexpected error
            return StatusCode(500, new { message = "Failed to create admin" });
        }
    }

    // DELETE: api/Admins/LockAdmin/{id}
    // Locks (or soft deletes) an admin user by ID
    [HttpDelete("LockAdmin/{id}")]
    public async Task<IActionResult> DeleteAdmin(string id)
    {
        var admin = await _adminService.DeleteAdmin(id);
        if (admin != false)
        {
            return Ok(admin); // Successfully locked the admin
        }
        // Locking failed, return bad request
        throw new BadRequestException("Failed to lock admin");
    }

    // PUT: api/Admins/RestoreAdmin/{id}
    // Restores a previously locked/deactivated admin user
    [HttpPut("RestoreAdmin/{id}")]
    public async Task<IActionResult> RestoreAdmin(string id)
    {
        var admin = await _adminService.RestoreAdmin(id);
        return Ok(admin);
    }

    [Authorize]
    [HttpGet("setting-profile")]
    public async Task<IActionResult> GetSettingProfile()
    {
        // get the id of the authenticated user
        var authen = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // string authen = "6832df81e1226b4811255384"; // testing  
        if (string.IsNullOrEmpty(authen))
        {
            return this.ApiError("account is not allowed", HttpStatusCode.Unauthorized);
        }

        var account = await _adminService.GetByIdAsync(authen);
        if (account == null)
        {
            return this.ApiError("Account not found", HttpStatusCode.NotFound);
        }

        var result = new AdminSettingResponse
        {
            email = account.Email,
            phoneNumber = account.PhoneNumber,
            password = account.Password
        };

        return this.ApiOk<AdminSettingResponse>(result, "Get admin Settings succesfully");
    }

    [Authorize]
    [HttpGet("request-otp-update")]
    public async Task<IActionResult> RequestOtpUpdate([FromQuery] string identifier, [FromQuery] string? useFor)
    {
        //  var authen = "6832df81e1226b4811255384";
        var authen = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(authen))
        {
            return this.ApiError("account is not allowed", HttpStatusCode.Unauthorized);
        }
        RequestOtpUpdateType? requestOtpUpdate = null;

        /** useFor:
         * 1 -> step 1 : ChangeCurrentIdentifier - yêu cầu OTP để được đổi identifier hiện tại
         * 2 -> step 2 : UpdateToNewIdentifier - yêu cầu OTP để cập nhật identifier mới
         */
        switch (useFor?.ToLower())
        {
            case "1":
                requestOtpUpdate = RequestOtpUpdateType.ChangeCurrentIdentifier;
                break;
            case "2":
                requestOtpUpdate = RequestOtpUpdateType.UpdateToNewIdentifier;
                break;
            default:
                break; // No specific request type, will use default
        }

        var result = await _adminService.RequestOtpForUpdate(identifier, authen, requestOtpUpdate);
        if (result == null)
        {
            return this.ApiError("Failed to request OTP", HttpStatusCode.BadRequest);
        }

        return this.ApiOk(result, "Request OTP for update settings successfully");
    }

    [HttpGet]
    [Route("resend-otp-update-by-email")]
    public async Task<IActionResult> ResendOtpUpdateByEmail([FromQuery] string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return this.ApiError("Email is required", HttpStatusCode.BadRequest);
        }
        var context = Request.Headers["id"].ToString();
        if (string.IsNullOrEmpty(context))
        {
            return this.ApiError("Context is not provided", HttpStatusCode.BadRequest);
        }
        var result = await _adminService.ResendOtpForUpdate(identifier, context);
        if (result == null)
        {
            return this.ApiError("Failed to resend OTP", HttpStatusCode.BadRequest);
        }
        return this.ApiOk(result, "Resend OTP for update settings successfully");
    }

    [Authorize]
    [HttpPut("confirm-otp-update")]
    public async Task<IActionResult> ConfirmOtpUpdate([FromQuery] string otp)
    {
        var authen = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(authen))
        {
            return this.ApiError("account is not allowed", HttpStatusCode.Unauthorized);
        }

        var context = Request.Headers["id"].ToString();
        if (context == null || context.Length == 0)
        {
            return this.ApiError("Confirm is not corect", HttpStatusCode.BadRequest);
        }
        var result = await _adminService.ConfirmOtpUpdate(otp, context);
        if (result == false)
        {
            return this.ApiError("Failed to confirm OTP", HttpStatusCode.BadRequest);
        }
        return this.ApiOk(result, "Confirm OTP for update settings successfully");
    }

    [Authorize]
    [HttpPut("update-setting-admin")]
    public async Task<IActionResult> UpdateSettingAdmin([FromBody] UpdateAdminSettingRequest request)
    {
        var authen = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(authen))
        {
            return this.ApiError("account is not allowed", HttpStatusCode.Unauthorized);
        }

        if (request == null)
        {
            return this.ApiError("Request is null", HttpStatusCode.BadRequest);
        }
        var result = await _adminService.UpdateSetting(request, authen);
        if (result == false)
        {
            return this.ApiError("Failed to update setting", HttpStatusCode.BadRequest);
        }
        return this.ApiOk(result, "Update setting successfully");
    }

    [Authorize]
    [HttpPut("update-password-admin")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdateAdminPasswordRequest request)
    {
        var authen = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(authen))
        {
            return this.ApiError("account is not allowed", HttpStatusCode.Unauthorized);
        }
        // Validate the request object
        if (request == null)
        {
            return this.ApiError("Request is null", HttpStatusCode.BadRequest);
        }

        var result = await _adminService.UpdatePassword(request, authen);
        if (result == false)
        {
            return this.ApiError("Failed to update password", HttpStatusCode.BadRequest);
        }
        return this.ApiOk(result, "Update password successfully");
    }
}
