// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Common.Extensions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.Admins.Interface;
using TraVinhMaps.Application.Features.Admins.Models;
using TraVinhMaps.Application.Features.Roles.Interface;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Features.Admins;
public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IRoleService _roleService;
    private readonly IEmailSender _emailSender;
    private const string CacheKey = "otp_context:";
    private readonly ICacheService _cacheService;
    private readonly ISpeedSmsService _speedSmsService;
    private readonly IBaseRepository<Otp> _otpRepository;

    public AdminService(IAdminRepository adminRepository, IEmailSender emailSender, IRoleService roleService, ICacheService cacheService, ISpeedSmsService speedSmsService, IBaseRepository<Otp> otpRepository)
    {
        _adminRepository = adminRepository;
        _emailSender = emailSender;
        _roleService = roleService;
        _cacheService = cacheService;
        _speedSmsService = speedSmsService;
        _otpRepository = otpRepository;
    }

    public async Task<User> AddAsync(AdminRequest entity, CancellationToken cancellationToken = default)
    {
        // Get the admin role from the role service
        var adminRole = (await _roleService.ListAllAsync(cancellationToken))
            .FirstOrDefault(r => r.RoleName.ToLower() == "admin" && r.RoleStatus);
        if (adminRole == null)
        {
            throw new BadRequestException("Admin role not found.");
        }

        // Assign the admin role ID to the new entity
        entity.RoleId = adminRole.Id;

        // Create the admin user using the repository
        var admin = await _adminRepository.AddAsync(entity, cancellationToken);
        if (admin == null)
        {
            throw new BadRequestException("User not created succesfully");
        }

        //  Send email notification with the password
        await _emailSender.SendEmailPasswordAsync(
        admin.Email,
        "Welcome to TraVinhMaps - Admin Account Created",
        $"{admin.Username}|{admin.Email}|admin123@");
        return admin;
    }

    public async Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
    {
        return await _adminRepository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<User, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return await _adminRepository.CountAsync(predicate, cancellationToken);
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _adminRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAdmin(string id, CancellationToken cancellationToken = default)
    {
        // Get info
        var admin = await _adminRepository.GetByIdAsync(id, cancellationToken);
        if (admin == null)
            return false;

        // Delete
        var result = await _adminRepository.DeleteAdmin(id, cancellationToken);
        if (!result)
            return false;

        // Send email 
        var subject = "Your Admin Account Has Been Banned";
        var reason = "Violation of admin policies";

        await _emailSender.SendEmailBanedAdminAsync(
            admin.Email,
            subject,
            $"{admin.Username}|{reason}",
            cancellationToken
        );

        return true;
    }


    public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _adminRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<User>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var adminRole = (await _roleService.ListAllAsync())
        .FirstOrDefault(r => r.RoleName.ToLower() == "admin" && r.RoleStatus);

        if (adminRole == null)
        {
            throw new NotFoundException("Admin role not found.");
        }

        // Return all users with admin role
        return await _adminRepository.ListAsync(u => u.RoleId == adminRole.Id, cancellationToken);
    }

    public async Task<IEnumerable<User>> ListAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _adminRepository.ListAsync(predicate, cancellationToken);
    }

    public async Task<bool> RestoreAdmin(string id, CancellationToken cancellationToken = default)
    {
        // Get info 
        var admin = await _adminRepository.GetByIdAsync(id, cancellationToken);
        if (admin == null)
            return false;

        // Restore
        var result = await _adminRepository.RestoreAdmin(id, cancellationToken);
        if (!result)
            return false;

        // Send email
        var subject = "Your Admin Account Has Been Reactivated";

        await _emailSender.SendEmailUnbanAdminAsync(
            admin.Email,
            subject,
            admin.Username,
            cancellationToken
        );

        return true;
    }

    public async Task<string> RequestOtpForUpdate(string identifier, string authen, RequestOtpUpdateType? typeUpdate, CancellationToken cancellationToken = default)
    {
        var user = await _adminRepository.GetByIdAsync(authen, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        // Validate the identifier (email or phone number)
        var userExits = await _adminRepository.GetAsyns(u => u.Email == identifier || u.PhoneNumber == identifier, cancellationToken);
        switch (typeUpdate)
        {
            case RequestOtpUpdateType.ChangeCurrentIdentifier:
                if (userExits == null)
                {
                    throw new BadRequestException("Identifier does not belong to the current user.");
                }
                if (userExits.Id != user.Id)
                {
                    throw new BadRequestException("Identifier already exists for another user.");
                }
                break;
            case RequestOtpUpdateType.UpdateToNewIdentifier:
                if (userExits == null)
                {
                    break;
                }
                if (userExits.Id == user.Id)
                {
                    throw new BadRequestException("Identifier already exists for the current user.");
                }
                throw new BadRequestException("Identifier already exists for another user.");
            default:
                throw new ArgumentException("Invalid request type. Only ChangeCurrentIdentifier or UpdateToNewIdentifier is allowed.");
        }

        bool isEmail = System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        var otp = GenarateOtpExtension.GenerateOtp();
        if (isEmail)
        {
            await _emailSender.SendEmailAsync(identifier, "OTP Verification For TraVinhGo", otp, cancellationToken);
        }
        else
        {
            var message = $"Your OTP in TRAVINHGO is {otp}";
            await _speedSmsService.SendSMS(identifier, message);
        }
        // Store the OTP in cache with a 5-minute expiration time
        var otpEntity = new Otp
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            Identifier = identifier,
            CreatedAt = DateTime.UtcNow,
            IdentifierType = "verify update settings",
            HashedOtpCode = HashingExtension.HashWithSHA256(otp),
            ExpiredAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0
        };
        await _otpRepository.AddAsync(otpEntity, cancellationToken);

        var contextId = Guid.NewGuid().ToString();
        // Store the OTP context in cache with a 5-minute expiration time
        var key = CacheKey + contextId;
        await _cacheService.SetData(key, otpEntity.Id);

        return contextId;

    }

    public async Task<bool> ConfirmOtpUpdate(string otp, string context, CancellationToken cancellationToken = default)
    {
        var key = CacheKey + context;
        string? otpId = await _cacheService.GetData<string>(key);
        if (otpId == null)
        {
            throw new NotFoundException("OTP is not found");
        }
        var otpEntity = await _otpRepository.GetByIdAsync(otpId, cancellationToken);
        if (otpEntity == null)
        {
            throw new NotFoundException("OTP is not found");
        }
        if (otpEntity.IsUsed)
        {
            return false; // OTP has already been used, return false
        }
        if (otpEntity.ExpiredAt < DateTime.UtcNow)
        {
            otpEntity.IsUsed = true; // Mark the OTP as used
            return false; // OTP is expired, return false
        }
        if (otpEntity.AttemptCount >= 3)
        {
            otpEntity.IsUsed = true; // Lock the OTP after 3 failed attempts
            return false; // OTP is locked, return false
        }
        var hashedOtp = HashingExtension.HashWithSHA256(otp);
        if (hashedOtp != otpEntity.HashedOtpCode)
        {
            otpEntity.AttemptCount++;
            otpEntity.LastAttemptAt = DateTime.UtcNow;
            await _otpRepository.UpdateAsync(otpEntity, cancellationToken);
            return false; // OTP is incorrect, return false
        }

        // Mark the OTP as used
        otpEntity.IsUsed = true;
        await _otpRepository.UpdateAsync(otpEntity, cancellationToken);
        // Remove the OTP context from cache
        await _cacheService.RemoveData(key);
        return true;
    }

    public async Task<bool> UpdateSetting(UpdateAdminSettingRequest request, string authen, CancellationToken cancellationToken = default)
    {
        var user = await _adminRepository.GetByIdAsync(authen, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }
        // check the type is email or phone number or password
        switch (request.UpdateType.ToLower())
        {
            case "email":
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.UpdateValue, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return false; // Invalid email format
                }
                user.Email = request.UpdateValue;
                break;
            case "phone":
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.UpdateValue, @"^(\+\d{1,3}[- ]?)?\d{10,15}$")) // chech the
                {
                    throw new ArgumentException("Invalid phone number format.");
                }
                user.PhoneNumber = request.UpdateValue;
                break;
            default:
                throw new ArgumentException("Invalid update type. Only 'email' or 'phoneNumber' is allowed.");
        }
        // Update the user in the repository
        await _adminRepository.UpdateAsync(user, cancellationToken);
        return true; // Update successful
    }

    public async Task<bool> UpdatePassword(UpdateAdminPasswordRequest request, string authen, CancellationToken cancellationToken = default)
    {
        // Implement the logic to update the admin password here
        var user = await _adminRepository.GetByIdAsync(authen, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var hashedPassword = HashingExtension.HashWithSHA256(request.CurrentPassword);
        if (user.Password != hashedPassword)
        {
            return false;
        }

        user.Password = HashingExtension.HashWithSHA256(request.NewPassword);
        await _adminRepository.UpdateAsync(user, cancellationToken);
        return true; // Password updated successfully
    }

    public async Task<string> ResendOtpForUpdate(string identifier, string context, CancellationToken cancellationToken = default)
    {
        var key = CacheKey + context;
        var otp = await _cacheService.GetData<string>(key);
        if (otp == null)
        {
            throw new NotFoundException("OTP context not found.");
        }
        var otpEntity = await _otpRepository.GetByIdAsync(otp, cancellationToken);
        if (otpEntity == null)
        {
            throw new NotFoundException("OTP not found.");
        }

        await _cacheService.RemoveData(key); // Remove the old OTP context from cache   
        await _otpRepository.DeleteAsync(otpEntity, cancellationToken);
        // Generate a new OTP
        var code = GenarateOtpExtension.GenerateOtp();
        var isEmail = System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (isEmail)
        {
            await _emailSender.SendEmailAsync(identifier, "OTP Verification For TraVinhGo", otp, cancellationToken);
        }
        else
        {
            var message = $"Your OTP in TRAVINHGO is {otp}";
            await _speedSmsService.SendSMS(identifier, message);
        }
        var newOtp = new Otp
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            Identifier = identifier,
            CreatedAt = DateTime.UtcNow,
            IdentifierType = "verify update settings",
            HashedOtpCode = HashingExtension.HashWithSHA256(code),
            ExpiredAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0
        };
        await _otpRepository.AddAsync(newOtp, cancellationToken);
        // Store the new OTP context in cache with a 5-minute expiration time
        var newContextId = Guid.NewGuid().ToString();
        await _cacheService.SetData(CacheKey + newContextId, newOtp.Id);
        return newContextId;
    }
}
