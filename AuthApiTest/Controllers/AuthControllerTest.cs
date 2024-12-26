using AuthApi.Controllers;
using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Resources;
using AuthApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;

namespace AuthApiTest.Controllers;
public class AuthControllerTest
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IStringLocalizer<Resource>> _mockLocalization;
    private readonly AuthController _controller;

    public AuthControllerTest()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLocalization = new Mock<IStringLocalizer<Resource>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLocalization.Object);
    }

    [Fact]
    public async Task Register_ValidCredentials_OkResult()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Password = "Password123!",
            Email = "testuser@example.com",
            Address = "123 Main St",
            FullName = "Test User"
        };
        var authResult = new AuthResult
        {
            Success = true,
            Message = "RegistrationSuccessful",
            AccessToken = "test_token",
            RefreshToken = "test_refresh_token"
        };
        _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync(authResult);
        _mockLocalization.Setup(l => l[ResourceKey.RegistrationSuccessful]).Returns(new LocalizedString(ResourceKey.RegistrationSuccessful, "Registration successful."));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<SuccessResponse>().Subject;
        response.Status.Should().Be(200);
        response.Message.Should().Be("Registration successful.");
    }

    [Fact]
    public async Task Register_UserExists_BadRequestResult()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "existuser",
            Password = "Password123!",
            Email = "existuser@example.com",
            Address = "123 Main St",
            FullName = "Test User"
        };
        var authResult = new AuthResult
        {
            Success = false,
            Message = "UserExists"
        };
        _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync(authResult);
        _mockLocalization.Setup(l => l[ResourceKey.RegistrationFailed]).Returns(new LocalizedString(ResourceKey.RegistrationFailed, "Registration failed."));
        _mockLocalization.Setup(l => l[authResult.Message]).Returns(new LocalizedString("UserExists", "User name or email already exists."));
        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        var response = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        response.Status.Should().Be(400);
        response.Message.Should().Be("Registration failed.");
        response.Error.Should().Be("User name or email already exists.");
    }

    [Fact]
    public async Task Login_CorrectCredentials_OkResult()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Password123!"
        };
        var authResult = new AuthResult
        {
            Success = true,
            Message = "Login successful.",
            AccessToken = "test_token",
            RefreshToken = "test_refresh_token"
        };
        _mockAuthService.Setup(s => s.Login(loginDto)).ReturnsAsync(authResult);
        _mockLocalization.Setup(l => l[ResourceKey.LoginSuccessful]).Returns(new LocalizedString(ResourceKey.LoginSuccessful, "Login successful."));
        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<SuccessResponse>().Subject;
        response.Status.Should().Be(200);
        response.Message.Should().Be("Login successful.");
        response.Data.Should().NotBeNull();
        response.Data.Should().BeEquivalentTo(new
        {
            AccessToken = "test_token",
            RefreshToken = "test_refresh_token"
        });
    }

    [Fact]
    public async Task Login_IncorrectCredentials_UnauthorizedResult()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "incorrectpassword"
        };
        var authResult = new AuthResult
        {
            Success = false,
            Message = "UsernameOrPasswordInvalid"
        };
        _mockAuthService.Setup(s => s.Login(loginDto)).ReturnsAsync(authResult);
        _mockLocalization.Setup(l => l[ResourceKey.LoginFailed]).Returns(new LocalizedString(ResourceKey.LoginFailed, "Login failed."));
        _mockLocalization.Setup(l => l[authResult.Message]).Returns(new LocalizedString(authResult.Message, "Username or password is invalid."));
        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = (UnauthorizedObjectResult)result;
        var response = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        response.Status.Should().Be(401);
        response.Message.Should().Be("Login failed.");
        response.Error.Should().Be("Username or password is invalid.");
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_OkResult()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "valid_refresh_token"
        };
        var authResult = new AuthResult
        {
            Success = true,
            Message = "Login successful.",
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token"
        };
        _mockAuthService.Setup(s => s.RefreshToken(refreshTokenDto.RefreshToken)).ReturnsAsync(authResult);
        _mockLocalization.Setup(l => l[ResourceKey.LoginSuccessful]).Returns(new LocalizedString(ResourceKey.LoginSuccessful, "Login successful."));
        // Act
        var result = await _controller.RefreshToken(refreshTokenDto);
        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<SuccessResponse>().Subject;
        response.Status.Should().Be(200);
        response.Message.Should().Be("Login successful.");
        response.Data.Should().NotBeNull();
        response.Data.Should().BeEquivalentTo(new
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token"
        });
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_UnauthorizedResult()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "invalid_refresh_token"
        };
        var authResult = new AuthResult
        {
            Success = false,
            Message = "RefreshtokenInvalidOrExpired"
        };
        _mockAuthService.Setup(s => s.RefreshToken(refreshTokenDto.RefreshToken)).ReturnsAsync(authResult);
        _mockLocalization.Setup(l => l[ResourceKey.RefreshTokenFailed]).Returns(new LocalizedString(ResourceKey.RefreshTokenFailed, "Refresh token failed."));
        _mockLocalization.Setup(l => l["RefreshtokenInvalidOrExpired"]).Returns(new LocalizedString(authResult.Message, "The refresh token is invalid or has expired"));
        // Act
        var result = await _controller.RefreshToken(refreshTokenDto);
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = (UnauthorizedObjectResult)result;
        var response = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        response.Status.Should().Be(401);
        response.Message.Should().Be("Refresh token failed.");
        response.Error.Should().Be("The refresh token is invalid or has expired");
    }

    [Fact]
    public async Task LogoutAll_ValidUserId_OkResult()
    {
        // Arrange 
        var userId = "1";
        _mockAuthService.Setup(s => s.LogoutAll(int.Parse(userId))).ReturnsAsync(true);
        _mockLocalization.Setup(l => l[ResourceKey.LogoutSuccessful]).Returns(new LocalizedString(ResourceKey.LogoutSuccessful, "Logout successful."));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.ControllerContext.HttpContext.Request.Headers["X-UserId"] = userId;
        // Act 
        var result = await _controller.LogoutAll();
        // Assert 
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<SuccessResponse>().Subject;
        response.Status.Should().Be(200); response.Message.Should().Be("Logout successful.");
        response.Data.Should().BeNull();
    }
    [Fact]
    public async Task LogoutAll_NoUserId_UnauthorizedResult()
    {
        // Arrange 
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _mockLocalization.Setup(l => l[ResourceKey.UserUnauthenticated]).Returns(new LocalizedString(ResourceKey.UserUnauthenticated, "User is not authenticated."));

        // Act 
        var result = await _controller.LogoutAll();
        // Assert 
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = (UnauthorizedObjectResult)result;
        var response = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        response.Status.Should().Be(401);
        response.Message.Should().Be("User is not authenticated.");
    }
    [Fact]
    public async Task LogoutAll_LogoutFailed_BadRequestResult()
    {
        // Arrange 
        var userId = "1";
        _mockAuthService.Setup(s => s.LogoutAll(int.Parse(userId))).ReturnsAsync(false);
        _mockLocalization.Setup(l => l[ResourceKey.LogoutFailed]).Returns(new LocalizedString(ResourceKey.LogoutFailed, "Logout failed."));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.ControllerContext.HttpContext.Request.Headers["X-UserId"] = userId;
        // Act 
        var result = await _controller.LogoutAll();
        // Assert 
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        var response = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        response.Status.Should().Be(400);
        response.Message.Should().Be("Logout failed.");
    }
}
