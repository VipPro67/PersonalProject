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

namespace AuthApi.Services

{
    public interface IAuthService
    {
        Task<AuthResult> Register(RegisterDto registerDto);
        Task<AuthResult> Login(LoginDto loginDto);

    }
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AuthResult> Register(RegisterDto registerDto)
        {
            var result = new AuthResult();
            if (await _userRepository.IsUserExistAsync(registerDto.Email, registerDto.UserName))
            {
                result.Success = false;
                result.Message = "User already exists! Please choose a different username or email.";
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
                result.Message = "User registered failed!";
                return result;
            }
            string token = GenerateToken(saveUser);
            result.Success = true;
            result.Message = "User registered successfully!";
            result.Token = token;
            return result;
        }
        public async Task<AuthResult> Login(LoginDto loginDto)
        {
            var result = new AuthResult();
            var user = await _userRepository.GetAppUserByUserNameAsync(loginDto.UserName);
            if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
            { result.Success = false; result.Message = "Invalid username or password."; return result; }
            string token = GenerateToken(user);
            result.Success = true; result.Message = "Login successful!";
            result.Token = token; return result;
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
    }
}


