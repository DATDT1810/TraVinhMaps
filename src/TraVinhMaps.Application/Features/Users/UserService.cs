// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using SharpCompress.Common;
using System.Security.Claims;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.Common.Extensions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.Users.Interface;
using TraVinhMaps.Application.Features.Users.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Domain.Specs;
using Microsoft.AspNetCore.Http;
using TraVinhMaps.Application.Features.Feedback.Mapper;
using TraVinhMaps.Application.Features.Feedback.Models;
using TraVinhMaps.Application.Features.Users.Mappers;
using TraVinhMaps.Application.Features.Destination.Interface;
using TraVinhMaps.Application.Features.Destination;

namespace TraVinhMaps.Application.Features.Users;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITouristDestinationService _touristDestinationService;
    private readonly IBaseRepository<Role> _roleRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUserRepository userRepository, ITouristDestinationService touristDestinationService, IBaseRepository<Role> roleRepository, ICloudinaryService cloudinaryService, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _cloudinaryService = cloudinaryService;
        _httpContextAccessor = httpContextAccessor;
        _touristDestinationService = touristDestinationService;
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.Password = HashingTokenExtension.HashToken(entity.Password);
        entity.IsForbidden = false;
        return await _userRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
    {
        return await _userRepository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<User, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        var userRole = (await _roleRepository.ListAllAsync(cancellationToken))
            .FirstOrDefault(r => r.RoleName.ToLower() == "user" && r.RoleStatus);

        if (userRole == null)
        {
            throw new NotFoundException("Role 'user' not found.");
        }

        return await _userRepository.CountAsync(
            u => u.RoleId == userRole.Id && u.Status && !u.IsForbidden,
            cancellationToken
        );
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteUser(string id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.DeleteUser(id, cancellationToken);
    }

    public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<User>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var userRole = (await _roleRepository.ListAllAsync(cancellationToken))
             .FirstOrDefault(r => r.RoleName.ToLower() == "user" && r.RoleStatus);
        if (userRole == null)
        {
            throw new NotFoundException("User role not found.");
        }
        return await _userRepository.ListAsync(u => u.RoleId == userRole.Id, cancellationToken);
    }

    public async Task<IEnumerable<User>> ListAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _userRepository.ListAsync(u => u.Status == true, cancellationToken);
    }

    public async Task<bool> RestoreUser(string id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.RestoreUser(id, cancellationToken);
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<AdminProfileResponse> GetProfileAdmin(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found!");
        }
        var role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        var adminProfile = new AdminProfileResponse
        {
            Id = user.Id,
            UserName = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Password = user.Password, // Consider hashing this before returning
            RoleName = role?.RoleName ?? "admin",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Avatar = user.Profile?.Avatar,
            IsForbidden = user.IsForbidden,
            Status = user.Status
        };
        return adminProfile;
    }

    public async Task UpdateProfileAdmin(UpdateProfileAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return;
        }
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found!");
        }
        if (request.Email != null)
        {
            var existingUser = await _userRepository.GetAsyns(u => u.Email == request.Email && u.Id != request.Id, cancellationToken);
            if (existingUser != null)
            {
                throw new BadRequestException("Email already exists!");
            }
            user.Email = request.Email;
        }
        if (request.PhoneNumber != null)
        {
            var existingUser = await _userRepository.GetAsyns(u => u.PhoneNumber == request.PhoneNumber && u.Id != request.Id, cancellationToken);
            if (existingUser != null)
            {
                throw new BadRequestException("Phone number already exists!");
            }
            user.PhoneNumber = request.PhoneNumber;
        }
        if (request.UserName != null)
        {
            user.Username = request.UserName;
        }
        if (request.Avartar != null)
        {
            var result = await _cloudinaryService.UploadImageAsync(request.Avartar);
            if (result == null || result.SecureUrl == null)
            {
                throw new BadRequestException("Failed to upload image!");
            }
            if (user.Profile == null)
            {
                user.Profile = new Profile(); // tạo mới nếu chưa có
            }
            user.Profile.Avatar = result.SecureUrl.ToString();
        }
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public Task<Pagination<User>> GetUsersAsync(UserSpecParams userSpecParams, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Dictionary<string, object>> GetUserStatisticsAsync(string groupBy, string timeRange, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserStatisticsAsync(groupBy, timeRange, cancellationToken);
    }

    public async Task<UserProfileResponse> GetUserProfile(string id, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (result == null)
        {
            throw new NotFoundException("User not found");
        }
        if (result.Profile == null)
        {
            result.Profile = new Profile();
            return new UserProfileResponse
            {
                Fullname = result.Profile.FullName ?? "",
                Avatar = result.Profile.Avatar ?? "",
                Email = result.Email ?? "",
                Address = result.Profile.Address ?? "",
                Gender = result.Profile.Gender ?? "",
                HassedPassword = result.Password ?? "",
                Phone = result.PhoneNumber ?? "",
                DateOfBirth = result.Profile.DateOfBirth
            };
        }
        return new UserProfileResponse
        {
            Fullname = result.Profile.FullName ?? "",
            Avatar = result.Profile.Avatar ?? "",
            Email = result.Email ?? "",
            Address = result.Profile.Address ?? "",
            Gender = result.Profile.Gender ?? "",
            HassedPassword = result.Password ?? "",
            Phone = result.PhoneNumber ?? "",
            DateOfBirth = result.Profile.DateOfBirth
        };
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetPerformanceByTagAsync(IEnumerable<string>? tagNames, bool includeOcop, bool includeDestination, bool includeLocalSpecialty, bool includeTips, bool includeFestivals, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
            if (startDate > DateTime.UtcNow)
                throw new ArgumentException("Start date cannot be in the future");
        }

        return await _userRepository.GetPerformanceByTagAsync(
            tagNames,
            includeOcop,
            includeDestination,
            includeLocalSpecialty,
            includeTips,
            includeFestivals,
            startDate,
            endDate,
            cancellationToken
        );
    }
    
    public async Task<List<Favorite>> getFavoriteUserList(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return await _userRepository.getFavoriteUserList(userId, cancellationToken);
    }

    public async Task<bool> addItemToFavoriteList(FavoriteRequest favoriteRequest, CancellationToken cancellationToken = default)
    {
        if (favoriteRequest == null)
            throw new ArgumentNullException(nameof(favoriteRequest));

        // Lấy userId từ ClaimsPrincipal (được gán bởi SessionAuthenticationHandler) qua IHttpContextAccessor
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        var favorite = UserMapper.Mapper.Map<FavoriteRequest, Favorite>(favoriteRequest);
        favorite.UpdateAt = DateTime.UtcNow;

        if(favorite.ItemType == "Destination")
        {
            var addFavoriteToDestination = await _touristDestinationService.PlusFavorite(favorite.ItemId);
            if(!addFavoriteToDestination)
            {
                throw new BadRequestException("Failed to add favorite to destination.");
            }
        }

        return await _userRepository.addItemToFavoriteList(userId, favorite, cancellationToken);
    }

    public async Task<bool> removeItemToFavoriteList(string itemId, CancellationToken cancellationToken = default)
    {
        if (itemId == null)
            throw new ArgumentNullException(nameof(itemId));

        // Lấy userId từ ClaimsPrincipal (được gán bởi SessionAuthenticationHandler) qua IHttpContextAccessor
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var destination = await _touristDestinationService.GetByIdAsync(itemId, cancellationToken);
        if (destination != null)
        {
            var minusFavoriteToDestination = await _touristDestinationService.MinusFavorite(destination.Id);
            if (!minusFavoriteToDestination)
            {
                throw new BadRequestException("Failed to remove favorite to destination.");
            }
        }

        return await _userRepository.removeItemToFavoriteList(userId, itemId, cancellationToken);
    }
}
