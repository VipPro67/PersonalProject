using AuthApi.DTOs;
using AuthApi.Services;
using AuthApi.Repositories;
using AuthApi.Models;
using FluentAssertions;

using Moq;
using AuthApi.Helpers;

namespace AuthApiTest.Services;
public class AuthServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly AuthService _authService;

    public AuthServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _authService = new AuthService(_mockUserRepository.Object, _mockRefreshTokenRepository.Object);
    }

    [Fact]
    public async Task Register_ValidCredentials_SuccessAuthResult()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWTKeySecret", "your_test_secret_key_123456789@:)");
        Environment.SetEnvironmentVariable("JWTKeyTokenExpiryHour", "1");
        Environment.SetEnvironmentVariable("JWTKeyValidAudience", "your_valid_audience_123456789@");
        Environment.SetEnvironmentVariable("JWTKeyRefreshTokenExpiryDay", "7");

        var registerDto = new RegisterDto
        {
            UserName = "newuser",
            Password = "Password123!",
            Email = "newuser@example.com",
            Address = "123 Main St",
            FullName = "New User"
        };
        var newUser = new AppUser
        {
            UserId = 1,
            UserName = "newuser",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Email = "newuser@example.com",
            FullName = "New User",
            PasswordHash = PasswordHelper.HashPassword("Password123!"),
        };
        _mockUserRepository.Setup(repo => repo.IsUserExistAsync(registerDto.Email, registerDto.UserName))
                           .ReturnsAsync(false);
        _mockUserRepository.Setup(repo => repo.CreateAppUserAsync(It.IsAny<AppUser>()))
                       .ReturnsAsync(newUser);
        _mockRefreshTokenRepository.Setup(repo => repo.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
        .ReturnsAsync(true);

        // Act
        var result = await _authService.Register(registerDto);

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_ExistingEmail_UserExistsMessage()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "existinguser",
            Password = "Password123!",
            Email = "existinguser@example.com",
            Address = "123 Main St",
            FullName = "Existing User"
        };
        _mockUserRepository.Setup(repo => repo.IsUserExistAsync(registerDto.Email, registerDto.UserName))
                           .ReturnsAsync(true);

        // Act
        var result = await _authService.Register(registerDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("UserExists");
    }
    [Fact]
    public async Task Register_UserCreationFails_SaveDBFailedMessage()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "newuser",
            Password = "Password123!",
            Email = "newuser@example.com",
            Address = "123 Main St",
            FullName = "New User"
        };

        _mockUserRepository.Setup(repo => repo.IsUserExistAsync(registerDto.Email, registerDto.UserName))
            .ReturnsAsync(false);

        _mockUserRepository.Setup(repo => repo.CreateAppUserAsync(It.IsAny<AppUser>()))
            .ReturnsAsync((AppUser)null);

        // Act
        var result = await _authService.Register(registerDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("SaveDBFailed");
    }

    
    [Fact]
    public async Task Login_ValidCredentials_SuccessAuthResult()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWTKeySecret", "your_test_secret_key_123456789@:)");
        Environment.SetEnvironmentVariable("JWTKeyTokenExpiryHour", "1");
        Environment.SetEnvironmentVariable("JWTKeyValidAudience", "your_valid_audience_123456789@");
        Environment.SetEnvironmentVariable("JWTKeyRefreshTokenExpiryDay", "7");
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Password123!"
        };

        var newUser = new AppUser
        {
            UserId = 1,
            UserName = "newuser",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Email = "newuser@example.com",
            FullName = "New User",
            PasswordHash = PasswordHelper.HashPassword("Password123!"),
        };
        _mockUserRepository.Setup(repo => repo.GetAppUserByUserNameAsync(loginDto.UserName))
            .ReturnsAsync(newUser);
        _mockRefreshTokenRepository.Setup(repo => repo.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).ReturnsAsync(true);

        // Act
        var result = await _authService.Login(loginDto);

        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNull();
        result.RefreshToken.Should().NotBeNull();
    }
    [Fact]
    public async Task Login_IncorrectPassword_InvalidMessage()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "wrongpassword"
        };
        var authResult = new AuthResult
        {
            Success = false,
            Message = "UsernameOrPasswordInvalid"
        };
        _mockUserRepository.Setup(repo => repo.GetAppUserByUserNameAsync(loginDto.UserName))
            .ReturnsAsync(new AppUser { UserName = loginDto.UserName, PasswordHash = PasswordHelper.HashPassword("correctpassword") });

        // Act
        var result = await _authService.Login(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("UsernameOrPasswordInvalid");
    }
    [Fact]
    public async Task Login_RefreshTokenCreationFails_SaveDBFailedMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWTKeySecret", "your_test_secret_key_123456789@:)");
        Environment.SetEnvironmentVariable("JWTKeyTokenExpiryHour", "1");
        Environment.SetEnvironmentVariable("JWTKeyRefreshTokenExpiryDay", "7");
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Password123!"
        };
        var user = new AppUser
        {
            UserId = 1,
            UserName = "testuser",
            PasswordHash = PasswordHelper.HashPassword("Password123!"),
            Email = "testuser@example.com",
        };
        _mockUserRepository.Setup(repo => repo.GetAppUserByUserNameAsync(loginDto.UserName)).ReturnsAsync(user);
        _mockRefreshTokenRepository.Setup(repo => repo.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).ReturnsAsync(false);

        // Act
        var result = await _authService.Login(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("SaveDBFailed");
    }

    [Fact]
    public async Task RefreshToken_ValidToken_SuccessAuthResult()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWTKeySecret", "your_test_secret_key_123456789@:)");
        Environment.SetEnvironmentVariable("JWTKeyTokenExpiryHour", "1");
        Environment.SetEnvironmentVariable("JWTKeyRefreshTokenExpiryDay", "7");
        var refreshToken = "valid_refresh_token";
        var storedRefreshToken = new RefreshToken
        {
            UserId = 1,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        var newUser = new AppUser
        {
            UserId = 1,
            UserName = "newuser",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Email = "newuser@example.com",
            FullName = "New User",
            PasswordHash = PasswordHelper.HashPassword("Password123!"),
        };
        _mockRefreshTokenRepository.Setup(repo => repo.GetRefreshTokenAsync(refreshToken))
            .ReturnsAsync(storedRefreshToken);
        _mockUserRepository.Setup(repo => repo.GetAppUserByIdAsync(storedRefreshToken.UserId)).ReturnsAsync(newUser);
        _mockRefreshTokenRepository.Setup(repo => repo.RemoveRefreshTokenAsync(refreshToken)).ReturnsAsync(true);
        _mockRefreshTokenRepository.Setup(repo => repo.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).ReturnsAsync(true);

        // Act
        var result = await _authService.RefreshToken(refreshToken);

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNull();
        result.RefreshToken.Should().NotBeNull();
    }
    [Fact]
    public async Task RefreshToken_ExpiredToken_InvalidOrExpiredMessage()
    {
        // Arrange
        var expiredRefreshToken = "expired_refresh_token";
        var storedRefreshToken = new RefreshToken
        {
            Token = expiredRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRefreshTokenRepository.Setup(repo => repo.GetRefreshTokenAsync(expiredRefreshToken))
            .ReturnsAsync(storedRefreshToken);

        // Act
        var result = await _authService.RefreshToken(expiredRefreshToken);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("RefreshTokenExpired");
    }
    [Fact]
    public async Task RefreshToken_TokenNotFound_InvalidOrExpiredMessage()
    {
        // Arrange
        var refreshToken = "non_existent_token";
        _mockRefreshTokenRepository.Setup(repo => repo.GetRefreshTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken)null);

        // Act
        var result = await _authService.RefreshToken(refreshToken);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("RefreshTokenInvalid");
    }
    [Fact]
    public async Task RefreshToken_UserNotFound_InvalidOrExpiredMessage()
    {
        // Arrange
        var validRefreshToken = "valid_refresh_token";
        var storedRefreshToken = new RefreshToken
        {
            Token = validRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UserId = 123
        };

        _mockRefreshTokenRepository.Setup(r => r.GetRefreshTokenAsync(validRefreshToken))
            .ReturnsAsync(storedRefreshToken);
        _mockUserRepository.Setup(r => r.GetAppUserByIdAsync(storedRefreshToken.UserId))
            .ReturnsAsync((AppUser)null);

        // Act
        var result = await _authService.RefreshToken(validRefreshToken);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("UserNotFound");
    }
    [Fact]
    public async Task RefreshToken_ValidRefreshToken_GenerateNewTokens()
    {
        // Arrange
        var validRefreshToken = "valid_refresh_token";
        var storedRefreshToken = new RefreshToken
        {
            Token = validRefreshToken,
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        var user = new AppUser
        {
            UserId = 1,
            UserName = "testuser",
            Email = "testuser@example.com"
        };

        _mockRefreshTokenRepository.Setup(repo => repo.GetRefreshTokenAsync(validRefreshToken))
            .ReturnsAsync(storedRefreshToken);
        _mockUserRepository.Setup(repo => repo.GetAppUserByIdAsync(storedRefreshToken.UserId))
            .ReturnsAsync(user);
        _mockRefreshTokenRepository.Setup(repo => repo.RemoveRefreshTokenAsync(validRefreshToken))
            .ReturnsAsync(true);
        _mockRefreshTokenRepository.Setup(repo => repo.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RefreshToken(validRefreshToken);

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

    }
    [Fact]
    public async Task LogoutAll_SuccessfullyRefreshTokens_True()
    {
        // Arrange
        int userId = 1;
        _mockRefreshTokenRepository.Setup(r => r.RemoveAllRefreshTokensByUserIdAsync(userId)).ReturnsAsync(true);

        // Act
        var result = await _authService.LogoutAll(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAll_FailedRemoveRefreshTokens_False()
    {
        // Arrange
        int userId = 1;
        _mockRefreshTokenRepository.Setup(r => r.RemoveAllRefreshTokensByUserIdAsync(userId)).ReturnsAsync(false);

        // Act
        var result = await _authService.LogoutAll(userId);

        // Assert
        result.Should().BeFalse();
    }
}
