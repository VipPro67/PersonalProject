using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.DTOs;
using AuthApi.Repositories;
using AuthApi.Helpers;
using AuthApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Serilog;

namespace AuthApi.Services

{
    public interface IAuthService
    {
        Task<AuthResult> Register(RegisterDto registerDto);
        Task<AuthResult> Login(LoginDto loginDto);
        Task<AuthResult> RefreshToken(string refreshToken);
        Task<bool> LogoutAll(int userId);

    }
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthResult> Register(RegisterDto registerDto)
        {
            var result = new AuthResult();
            if (await _userRepository.IsUserExistAsync(registerDto.Email, registerDto.UserName))
            {
                result.Success = false;
                result.Message = "UserExists";
                Log.Error($"User name or email already exists {registerDto.UserName} - {registerDto.Email}");
                return result;
            }
            AppUser newUser = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PasswordHash = PasswordHelper.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Address = registerDto.Address,
            };
            var saveUser = await _userRepository.CreateAppUserAsync(newUser);
            if (saveUser == null)
            {
                result.Success = false;
                result.Message = "SaveDBFailed";
                Log.Error($"Save user into database failed.");
                return result;
            }
            string accessToken = GenerateToken(saveUser);
            var refreshToken = GenerateRefreshToken(saveUser);
            if (!await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken))
            {
                result.Success = false;
                result.Message = "SaveDBFailed";
                Log.Error($"Save refresh token into database failed.");
                return result;
            }

            result.Success = true;
            result.AccessToken = accessToken;
            result.RefreshToken = refreshToken.Token;
            return result;
        }
        public async Task<AuthResult> Login(LoginDto loginDto)
        {
            var result = new AuthResult();
            var user = await _userRepository.GetAppUserByUserNameAsync(loginDto.UserName);
            if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                result.Success = false;
                result.Message = "UsernameOrPasswordInvalid";
                Log.Error($"Login user {loginDto.UserName} invalid username or password..");
                return result;
            }
            string token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken(user);
            if (!await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken))
            {
                result.Success = false;
                result.Message = "SaveDBFailed";
                Log.Error("Save refresh token into database failed.");
                return result;
            }
            result.Success = true;
            result.AccessToken = token;
            result.RefreshToken = refreshToken.Token;
            return result;
        }

        public async Task<AuthResult> RefreshToken(string refreshToken)
        {
            var result = new AuthResult();
            var storedRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken);

            if (storedRefreshToken == null || storedRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                result.Success = false;
                result.Message = "RefreshtokenInvalidOrExpired";
                Log.Error($"Invalid or expired refresh token {refreshToken}");
                return result;
            }

            var user = await _userRepository.GetAppUserByIdAsync(storedRefreshToken.UserId);
            if (user == null)
            {
                result.Success = false;
                result.Message = "RefreshtokenInvalidOrExpired";
                Log.Error($"User not found {storedRefreshToken.UserId}");
                return result;
            }

            var newAccessToken = GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            await _refreshTokenRepository.RemoveRefreshTokenAsync(refreshToken);
            await _refreshTokenRepository.AddRefreshTokenAsync(newRefreshToken);

            result.Success = true;
            result.AccessToken = newAccessToken;
            result.RefreshToken = newRefreshToken.Token;
            return result;
        }

        private string GenerateToken(AppUser appUser)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTKeySecret")));
            var _TokenExpiryTimeInHour = Convert.ToInt32(Environment.GetEnvironmentVariable("JWTKeyTokenExpiryHour"));
            var claims = new List<Claim>
            {
            new Claim("nameid", appUser.UserId.ToString()),
            new Claim("email", appUser.Email),
            new Claim("unique_name", appUser.UserName),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = Environment.GetEnvironmentVariable("JWTKeyValidIssuer"),
                Audience = Environment.GetEnvironmentVariable("JWTKeyValidAudience"),
                Expires = DateTime.UtcNow.AddHours(_TokenExpiryTimeInHour),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(AppUser user)
        {
            var DayExpire = Environment.GetEnvironmentVariable("JWTKeyRefreshTokenExpiryDay");
            return new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.UserId,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(DayExpire)),
            };
        }

        public Task<bool> LogoutAll(int userId)
        {
            return _refreshTokenRepository.RemoveAllRefreshTokensByUserIdAsync(userId);
        }
    }
}


