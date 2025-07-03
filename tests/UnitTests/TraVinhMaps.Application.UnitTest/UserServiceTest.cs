// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Text;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Moq;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.Features.Users;
using TraVinhMaps.Application.Features.Users.Models;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.UnitTest;

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IBaseRepository<Domain.Entities.Role>> _mockRoleRepository;
    private readonly Mock<ICloudinaryService> _mockCloudinaryService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly UserService _userService;

    public UserServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IBaseRepository<Domain.Entities.Role>>();
        _mockCloudinaryService = new Mock<ICloudinaryService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _userService = new UserService(
            _mockUserRepository.Object,
            _mockRoleRepository.Object,
            _mockCloudinaryService.Object,
            _mockHttpContextAccessor.Object
        );
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithNullRequest_ShouldReturnWithoutException()
    {
        // Act & Assert
        await _userService.UpdateProfileAdmin(null);

        // Verify no repository calls were made
        _mockUserRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new UpdateProfileAdminRequest { Id = "non-existent-id" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _userService.UpdateProfileAdmin(request));
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithDuplicateEmail_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = "user-123";
        var email = "existing@example.com";
        var request = new UpdateProfileAdminRequest
        {
            Id = userId,
            Email = email
        };

        var user = new User
        {
            Id = userId,
            Email = "old@example.com",
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id"
        };

        var existingUser = new User
        {
            Id = "another-user",
            Email = email,
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.GetAsyns(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _userService.UpdateProfileAdmin(request));
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithDuplicatePhoneNumber_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = "user-123";
        var phoneNumber = "+84123456789";
        var request = new UpdateProfileAdminRequest
        {
            Id = userId,
            PhoneNumber = phoneNumber
        };

        var user = new User
        {
            Id = userId,
            PhoneNumber = "old-number",
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id"
        };

        var existingUser = new User
        {
            Id = "another-user",
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository.Setup(r => r.GetAsyns(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _userService.UpdateProfileAdmin(request));
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithValidUsername_ShouldUpdateUsername()
    {
        // Arrange
        var userId = "user-123";
        var newUsername = "newUsername";
        var request = new UpdateProfileAdminRequest
        {
            Id = userId,
            UserName = newUsername
        };

        var user = new User
        {
            Id = userId,
            Username = "oldUsername",
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Capture the updated user
        User capturedUser = null;
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateProfileAdmin(request);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal(newUsername, capturedUser.Username);
        Assert.NotNull(capturedUser.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithAvatar_ShouldUploadAndUpdateProfileAvatar()
    {
        // Arrange
        var userId = "user-123";
        var avatarUrl = "https://cloudinary.com/image.jpg";

        // Create mock file
        var content = "mock file content";
        var fileName = "test.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(stream.Length);
        formFile.Setup(f => f.OpenReadStream()).Returns(stream);
        formFile.Setup(f => f.ContentDisposition).Returns($"form-data; name=\"file\"; filename=\"{fileName}\"");

        var request = new UpdateProfileAdminRequest
        {
            Id = userId,
            Avartar = formFile.Object
        };

        var user = new User
        {
            Id = userId,
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id",
            Profile = null // User has no profile yet
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Setup cloudinary response
        var uploadResult = new ImageUploadResult
        {
            SecureUrl = new Uri(avatarUrl)
        };
        _mockCloudinaryService.Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(uploadResult);

        // Capture the updated user
        User capturedUser = null;
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateProfileAdmin(request);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotNull(capturedUser.Profile);
        Assert.Equal(avatarUrl, capturedUser.Profile.Avatar);
        Assert.NotNull(capturedUser.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProfileAdmin_WithAllFields_ShouldUpdateAllFields()
    {
        // Arrange
        var userId = "user-123";
        var newEmail = "new@example.com";
        var newPhone = "+84987654321";
        var newUsername = "newUsername";

        var request = new UpdateProfileAdminRequest
        {
            Id = userId,
            Email = newEmail,
            PhoneNumber = newPhone,
            UserName = newUsername
        };

        var user = new User
        {
            Id = userId,
            Email = "old@example.com",
            PhoneNumber = "old-number",
            Username = "oldUsername",
            CreatedAt = DateTime.UtcNow,
            RoleId = "role-id",
            Profile = new Profile()
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Setup repository to return null for GetAsyns (no duplicate email or phone)
        _mockUserRepository.Setup(r => r.GetAsyns(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Capture the updated user
        User capturedUser = null;
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateProfileAdmin(request);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal(newEmail, capturedUser.Email);
        Assert.Equal(newPhone, capturedUser.PhoneNumber);
        Assert.Equal(newUsername, capturedUser.Username);
        Assert.NotNull(capturedUser.UpdatedAt);
    }
    [Fact]
    public async Task GetProfileAdmin_WithValidId_ShouldReturnAdminProfile()
    {
        // Arrange
        var userId = "admin123";
        var roleId = "role123";
        var roleName = "super-admin";

        var user = new User
        {
            Id = userId,
            Username = "adminuser",
            Email = "admin@example.com",
            PhoneNumber = "1234567890",
            Password = "hashedpassword",
            RoleId = roleId,
            CreatedAt = new DateTime(2023, 1, 1),
            UpdatedAt = new DateTime(2023, 2, 1),
            Status = true,
            IsForbidden = false,
            Profile = new Profile
            {
                Avatar = "https://example.com/avatar.jpg"
            }
        };

        var role = new Domain.Entities.Role
        {
            Id = roleId,
            RoleName = roleName,
            CreatedAt = DateTime.Now,
            RoleStatus = true
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRoleRepository.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _userService.GetProfileAdmin(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("adminuser", result.UserName);
        Assert.Equal("admin@example.com", result.Email);
        Assert.Equal("1234567890", result.PhoneNumber);
        Assert.Equal("hashedpassword", result.Password);
        Assert.Equal(roleName, result.RoleName);
        Assert.Equal(new DateTime(2023, 1, 1), result.CreatedAt);
        Assert.Equal(new DateTime(2023, 2, 1), result.UpdatedAt);
        Assert.Equal("https://example.com/avatar.jpg", result.Avatar);
        Assert.False(result.IsForbidden);
        Assert.True(result.Status);
    }

    [Fact]
    public async Task GetProfileAdmin_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "nonexistent";

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _userService.GetProfileAdmin(userId));

        Assert.Equal("User not found!", exception.Message);
    }

    [Fact]
    public async Task GetProfileAdmin_WithValidIdButNoRole_ShouldDefaultToAdminRole()
    {
        // Arrange
        var userId = "admin123";
        var roleId = "role123";

        var user = new User
        {
            Id = userId,
            Username = "adminuser",
            Email = "admin@example.com",
            PhoneNumber = "1234567890",
            Password = "hashedpassword",
            RoleId = roleId,
            CreatedAt = new DateTime(2023, 1, 1),
            Status = true
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRoleRepository.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Role)null);

        // Act
        var result = await _userService.GetProfileAdmin(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin", result.RoleName); // Default role name
    }

    [Fact]
    public async Task GetProfileAdmin_WithValidIdButNoProfile_ShouldReturnNullAvatar()
    {
        // Arrange
        var userId = "admin123";
        var roleId = "role123";
        var roleName = "admin";

        var user = new User
        {
            Id = userId,
            Username = "adminuser",
            Email = "admin@example.com",
            PhoneNumber = "1234567890",
            Password = "hashedpassword",
            RoleId = roleId,
            CreatedAt = new DateTime(2023, 1, 1),
            Profile = null // No profile
        };

        var role = new Domain.Entities.Role
        {
            Id = roleId,
            RoleName = roleName,
            CreatedAt = DateTime.Now,
            RoleStatus = true
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRoleRepository.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _userService.GetProfileAdmin(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Avatar); // Avatar should be null
    }
}
