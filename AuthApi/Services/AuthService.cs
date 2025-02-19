using AuthApi.DTOs;
using AuthApi.Repositories;
using AuthApi.Helpers;
using AuthApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Serilog;

namespace AuthApi.Services;
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

        result.Success = true;
        result.Message = "UserCreatedSuccessfully";
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
            Log.Error($"Login user {loginDto.UserName} invalid username or password.");
            return result;
        }

        string token = GenerateToken(user);
        if (token == null)
        {
            result.Success = false;
            result.Message = "Failed to generate access token";
            Log.Error("Failed to generate access token.");
            return result;
        }

        var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenByUserIdAsync(user.UserId);
        string refreshTokenString;

        if (existingRefreshToken != null && existingRefreshToken.ExpiresAt > DateTime.UtcNow)
        {
            refreshTokenString = existingRefreshToken.Token;
        }
        else
        {
            var newRefreshToken = GenerateRefreshToken(user);
            if (newRefreshToken == null)
            {
                result.Success = false;
                result.Message = "Failed to generate refresh token";
                Log.Error("Failed to generate refresh token.");
                return result;
            }

            if (!await _refreshTokenRepository.AddRefreshTokenAsync(newRefreshToken))
            {
                result.Success = false;
                result.Message = "SaveDBFailed";
                Log.Error("Save refresh token into database failed.");
                return result;
            }

            refreshTokenString = newRefreshToken.Token;
        }
        result.Success = true;
        result.AccessToken = token;
        result.RefreshToken = refreshTokenString;
        return result;
    }


    public async Task<AuthResult> RefreshToken(string refreshToken)
    {
        var result = new AuthResult();
        var storedRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken);

        if (storedRefreshToken == null)
        {
            result.Success = false;
            result.Message = "RefreshTokenInvalid";
            Log.Error($"Invalid refresh token {refreshToken}");
            return result;
        }

        if (storedRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            result.Success = false;
            result.Message = "RefreshTokenExpired";
            Log.Error($"Expired refresh token {refreshToken}");
            await _refreshTokenRepository.RemoveRefreshTokenAsync(refreshToken);
            return result;
        }
        var user = await _userRepository.GetAppUserByIdAsync(storedRefreshToken.UserId);
        if (user == null)
        {
            result.Success = false;
            result.Message = "UserNotFound";
            Log.Error($"User not found for refresh token {refreshToken}");
            return result;
        }

        var newAccessToken = GenerateToken(user);
        if (newAccessToken == null)
        {
            result.Success = false;
            result.Message = "Failed to generate new tokens";
            Log.Error("Failed to generate new access token.");
            return result;
        }
        result.Success = true;
        result.RefreshToken = refreshToken;
        result.AccessToken = newAccessToken;
        return result;
    }


    private string? GenerateToken(AppUser appUser)
    {
        try
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTKeySecret")));
            var _TokenExpiryTimeInHour = Convert.ToInt32(Environment.GetEnvironmentVariable("JWTKeyTokenExpiryHour"));
            var claims = new List<Claim>
            {
            new Claim("nameid", appUser.UserId.ToString()),
            new Claim("email", appUser.Email),
            new Claim("unique_name", appUser.UserName),
            new Claim("scope", "student-api"),
            new Claim("scope", "course-api"),
            new Claim("scope", "enrollment-api"),
            new Claim("scope", "all-api"),
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
        catch (Exception ex)
        {
            Log.Error($"Error generating token: {ex.Message}");
            return null;
        }
    }

    private RefreshToken? GenerateRefreshToken(AppUser user)
    {
        try
        {
            var DayExpire = Environment.GetEnvironmentVariable("JWTKeyRefreshTokenExpiryDay");
            return new RefreshToken
            {
                //Token = Guid.NewGuid().ToString(), this 36 characters
                Token = GenerateSecureToken(), //
                UserId = user.UserId,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(DayExpire))
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Error generating refresh token: {ex.Message}");
            return null;
        }
    }

    private string GenerateSecureToken()
    {
        var randomNumber = new byte[48];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }


    public Task<bool> LogoutAll(int userId)
    {
        return _refreshTokenRepository.RemoveAllRefreshTokensByUserIdAsync(userId);
    }

}



