// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TraVinhMaps.Api.Extensions;
using TraVinhMaps.Api.Hubs;
using TraVinhMaps.Application.Features.Auth.Interface;
using TraVinhMaps.Application.Features.Auth.Models;

namespace TraVinhMaps.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;
    private readonly IHubContext<DashboardHub> _hubContext;
    public AuthController(IAuthServices authServices, IHubContext<DashboardHub> hubContext)
    {
        _authServices = authServices;
        _hubContext = hubContext;
    }

    #region Request Authentication for User endpoint
    /*
    * Endpoint for handle request authentication with Fluter App
    */
    [HttpPost("request-email-authen")]
    public async Task<IActionResult> AuthenWithEmail(string email)
    {
        if (email == null)
        {
            return this.ApiError("Email is required", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.AuthenWithEmail(email);
        var response = new
        {
            token = result
        };
        return this.ApiOk(JsonConvert.SerializeObject(response), "Email authentication requested successfully");
    }
    /*
     * Endpoint for handle request authentication with Fluter App
     */
    [HttpPost("request-phonenumber-authen")]
    public async Task<IActionResult> AuthenWithPhoneNumber(string phoneNumber)
    {
        if (phoneNumber == null)
        {
            return this.ApiError("Phone number is required", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.AuthenWithPhoneNumber(phoneNumber);
        var response = new
        {
            token = result
        };
        return this.ApiOk(JsonConvert.SerializeObject(response)
        , "Phone number authentication requested successfully");
    }

    #endregion

    [HttpPost("confirm-otp-authen")]
    public async Task<IActionResult> Confirm(string otp)
    {
        var id = Request.Headers["id"].ToString();
        if (string.IsNullOrEmpty(id))
        {
            return this.ApiError("ID is required", HttpStatusCode.Unauthorized);
        }
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        string? device = Request.Headers["device"].ToString();
        var result = await _authServices.VerifyOtp(id, otp, device, ip);
        if (result == null)
        {
            return this.ApiError("Invalid OTP", HttpStatusCode.Unauthorized);
        }
        // Send SignalR for dashboard
        await _hubContext.Clients.Group("admin").SendAsync("ChartAnalytics");
            
        return this.ApiOk(JsonConvert.SerializeObject(result), "OTP verified successfully");
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var sessionId = User.Claims.FirstOrDefault(c => c.Type == "sessionId")?.Value;

        if (string.IsNullOrEmpty(sessionId))
        {
            return this.ApiError("Session ID not found in token", HttpStatusCode.Unauthorized);
        }
        await _authServices.Logout(sessionId);
        return this.ApiOk("Logout successfully");
    }

    [HttpGet("get-active-session")]
    [Authorize(Roles = "admin")]
    [Authorize(Roles = "super-admin")]
    public async Task<IActionResult> GetAllTheActiveSession()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return this.ApiError("User ID not found in token", HttpStatusCode.Unauthorized);
        }
        var sessions = await _authServices.GetAllSessionUser(userId);
        return this.ApiOk(sessions, "Active sessions retrieved successfully");
    }

    [HttpPost("refresh-otp")]
    public async Task<IActionResult> RefreshOtp(string item)
    {
        if (string.IsNullOrEmpty(item))
        {
            return this.ApiError("Item need require", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.RefreshOtp(item);
        var response = new
        {
            token = result
        };
        return this.ApiOk(JsonConvert.SerializeObject(response), "OTP refreshed successfully");
    }

    #region Authentication Admin endpoint feature

    [HttpPost("login-admin")]
    public async Task<IActionResult> LoginAdmin([FromBody] AuthAdminRequest request)
    {
        var result = await _authServices.AuthenAdminWithCredentials(request.Identifier, request.Password);
        if (string.IsNullOrEmpty(result))
        {
            return this.ApiError(result, HttpStatusCode.BadRequest);
        }
        return this.ApiOk(result, "Login successfully");
    }

    [HttpPost("login-email-authen-admin")]
    public async Task<IActionResult> AuthenWithEmailAdmin(string email)
    {
        if (email == null)
        {
            return this.ApiError("Email is required", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.AuthenWithEmailAdmin(email);
        var response = new
        {
            token = result
        };
        return this.ApiOk(JsonConvert.SerializeObject(response), "Email authentication requested successfully");
    }

    [HttpPost("confirm-otp-admin")]
    public async Task<IActionResult> ConfirmOtpAdmin(string otp)
    {
        var id = Request.Headers["id"].ToString();
        if (string.IsNullOrEmpty(id))
        {
            return this.ApiError("ID is required", HttpStatusCode.Unauthorized);
        }
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        string? device = Request.Headers["device"].ToString();
        var result = await _authServices.VerifyOtpAdmin(id, otp, device, ip);
        if (result == null)
        {
            return this.ApiError("Invalid OTP", HttpStatusCode.Unauthorized);
        }
        return this.ApiOk(JsonConvert.SerializeObject(result), "OTP verified successfully");
    }
    #endregion

    #region Forget password 
    // endpoint for handle reset password  
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgetPassword(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return this.ApiError("Identifier is required", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.ForgetPassword(identifier);
        if (string.IsNullOrEmpty(result))
        {
            return this.ApiError(result, HttpStatusCode.BadRequest);
        }
        return this.ApiOk(result, "Request forget password is handle successfully");
    }
    // endpoint for handle confirm new password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPassword)
    {
        if (string.IsNullOrEmpty(resetPassword.Identifier) || string.IsNullOrEmpty(resetPassword.NewPassword))
        {
            return this.ApiError("Identifier and new password are required", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.ResetPassword(resetPassword.Identifier, resetPassword.NewPassword);
        if (!result)
        {
            return this.ApiError("Reset password failed", HttpStatusCode.BadRequest);
        }
        return this.ApiOk("Reset password sucessfull");
    }
    // endpoint for handle confirm otp forget
    [HttpPost("confirm-otp-forgot-password")]
    public async Task<IActionResult> RefreshOtpForgotPassword(string otpCode)
    {
        var id = Request.Headers["id"].ToString();
        if (string.IsNullOrEmpty(id))
        {
            return this.ApiError("ID is required", HttpStatusCode.BadRequest);
        }
        if (string.IsNullOrEmpty(otpCode))
        {
            return this.ApiError("otp is required", HttpStatusCode.BadRequest);
        }
        var result = await _authServices.VerifyOtpForResetPassword(id, otpCode);
        var response = new
        {
            token = result
        };
        return this.ApiOk(JsonConvert.SerializeObject(response), "OTP refreshed successfully");
    }
    #endregion

    // endpoint for handle refresh token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Headers["refreshToken"].FirstOrDefault();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return this.ApiError("Session ID and refresh token are required", HttpStatusCode.Unauthorized);
        }
        var result = await _authServices.RefreshToken(refreshToken);
        if (result == null)
        {
            return this.ApiError("Invalid session or refresh token", HttpStatusCode.Unauthorized);
        }
        return this.ApiOk(JsonConvert.SerializeObject(result), "Token refreshed successfully");
    }
}

