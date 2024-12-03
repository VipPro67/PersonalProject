using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AuthApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
            if (!result.Success)
            {
                Log.Error($"Failed to register user: {registerDto.UserName}", registerDto.UserName);
                return StatusCode(400, new ErrorResponse(400, "Registration failed", result.Message ?? "Unknown error"));
            }
            Log.Information($"User {registerDto.UserName} registered successfully", registerDto.UserName);
            return Ok(new SuccessResponse(200, result.Message ?? "Registration successful", result.AccessToken));
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            //Log current Accept-Language header for localization
            Log.Information($"Current Accept-Language header: {Request.Headers["Accept-Language"]}");
            var result = await _authService.Login(loginDto);
            if (!result.Success)
            {
                Log.Information($"Failed login attempt for user: {loginDto.UserName}", loginDto.UserName);
                return StatusCode(401, new ErrorResponse(401, "Login failed", result.Message ?? "Invalid credentials"));
            }
            Log.Information($"User {loginDto.UserName} logged in successfully", loginDto.UserName);
            return Ok(new SuccessResponse(200, result.Message ?? "Login successful", new
            {
                result.AccessToken,
                result.RefreshToken
            }));
        }
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshToken(refreshTokenDto.RefreshToken);
            if (!result.Success)
            {
                return StatusCode(401, new ErrorResponse(401, "Refresh token failed", result.Message ?? "Invalid refresh token"));
            }
            return Ok(new SuccessResponse(200, result.Message ?? "Login successful", new
            {
                result.AccessToken,
                result.RefreshToken
            }));
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = Request.Headers["X-UserId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ErrorResponse(401, "User not authenticated", null));
            }
            var result = await _authService.LogoutAll(int.Parse(userId));
            if (!result)
            {
                return StatusCode(500, new ErrorResponse(500, "Failed to log out users", null));
            }
            return Ok(new SuccessResponse(200, "Logged out successfully", null));
        }
    }
}