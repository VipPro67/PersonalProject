using System.Security.Claims;
using AuthApi.Attributes;
using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello Auth There!");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
            if (!result.Success)
            {
                return StatusCode(400, new ErrorResponse(400, "Registration failed", result.Message));
            }
            return Ok(new SuccessResponse(200, result.Message, result.Token));
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.Login(loginDto);
            if (!result.Success)
            {
                return StatusCode(401, new ErrorResponse(401, "Login failed", result.Message));
            }
            return Ok(new SuccessResponse(200, result.Message, result.Token));

        }

        [HttpGet("protected")]
        [JwtAuthorize]
        public IActionResult ProtectedEndpoint()
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var UserName = User.FindFirstValue(ClaimTypes.Name);
            var Email = User.FindFirstValue(ClaimTypes.Email);
            return Ok($"Hello User {UserId}! {UserName} {Email} You are authorized to access this endpoint.");
        }
    }
}
