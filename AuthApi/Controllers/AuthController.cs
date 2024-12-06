using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Resources;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Serilog;

namespace AuthApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly LocalizationHelper _localizationHelper;

        public AuthController(IAuthService authService, IStringLocalizer<Resource> localization)
        {
            _authService = authService;
            _localizationHelper = new LocalizationHelper(localization);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
            if (!result.Success)
            {
                return StatusCode(400, new ErrorResponse(400, _localizationHelper.GetComplexMessage(ResourceKey.Registration, ResourceKey.Failed), _localizationHelper.GetSimpleMessage(result.Message)));
            }
            Log.Information($"User {registerDto.UserName} registered successfully", registerDto.UserName);
            return Ok(new SuccessResponse(200, _localizationHelper.GetComplexMessage(ResourceKey.Registration, ResourceKey.Successful), new
            {
                result.AccessToken,
                result.RefreshToken
            }));
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.Login(loginDto);
            if (!result.Success)
            {
                return StatusCode(401, new ErrorResponse(401, _localizationHelper.GetComplexMessage(ResourceKey.Login, ResourceKey.Failed), _localizationHelper.GetSimpleMessage(result.Message)));
            }
            Log.Information($"User {loginDto.UserName} logged in successfully", loginDto.UserName);
            return Ok(new SuccessResponse(200, result.Message ?? _localizationHelper.GetComplexMessage(ResourceKey.Login, ResourceKey.Successful), new
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
                return StatusCode(401, new ErrorResponse(401, _localizationHelper.GetComplexMessage(ResourceKey.RefreshToken, ResourceKey.Failed), _localizationHelper.GetSimpleMessage(result.Message)));
            }
            Log.Information($"User get refreshed successfully");
            return Ok(new SuccessResponse(200, result.Message ?? _localizationHelper.GetComplexMessage(ResourceKey.RefreshToken, ResourceKey.Successful), new
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
                return Unauthorized(new ErrorResponse(401, _localizationHelper.GetComplexMessage(ResourceKey.User, ResourceKey.Unauthenticated), null));
            }
            var result = await _authService.LogoutAll(int.Parse(userId));
            if (!result)
            {
                return StatusCode(400, new ErrorResponse(400, _localizationHelper.GetComplexMessage(ResourceKey.Logout, ResourceKey.Failed), null));
            }
            Log.Information($"User {userId} logged out successfully");
            return Ok(new SuccessResponse(200, _localizationHelper.GetComplexMessage(ResourceKey.Logout, ResourceKey.Successful), null));
        }
    }
}
