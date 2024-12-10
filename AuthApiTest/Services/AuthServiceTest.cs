using AuthApi.DTOs;
using AuthApi.Services;
using AuthApi.Repositories;
using AuthApi.Models;
using Moq;

namespace AuthApiTest.Services
{
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
    }
}
