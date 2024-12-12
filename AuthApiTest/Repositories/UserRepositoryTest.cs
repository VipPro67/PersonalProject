using AuthApi.Models;
using AuthApi.Repositories;
using AuthApi.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AuthApiTest.Repositories;
public class UserRepositoryTest
{
    private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }
    private async Task<ApplicationDbContext> CreateContextAndSeedDatabase()
    {
        var options = CreateNewContextOptions();

        var context = new ApplicationDbContext(options);
        var listAppUsers = new List<AppUser>
            {
                new AppUser { UserId = 1, UserName = "testuser", PasswordHash = "some_thing_hash", Email = "testuser@example.com", Address = "123 Main St", FullName = "Test User" },
                new AppUser { UserId = 2, UserName = "testuser2", PasswordHash = "another_thing_hash", Email = "testuser2@example.com", Address = "456 Elm St", FullName = "Test User 2" },
                new AppUser { UserId = 3, UserName = "testuser3", PasswordHash = "yet_another_thing_hash", Email = "testuser3@example.com", Address = "789 Oak St", FullName = "Test User 3" }
            };

        await context.Users.AddRangeAsync(listAppUsers);
        await context.SaveChangesAsync();
        return context;
    }

    private async Task<UserRepository> CreateUserRepositoryWithSeededData(ApplicationDbContext context)
    {
        return new UserRepository(context);
    }

    [Fact]
    public async Task CreateAppUserAsync_ValidUser_User()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var newUser = new AppUser { UserName = "newuser", PasswordHash = "new_thing_hash", Email = "newuser@example.com", Address = "987 Maple St", FullName = "New User" };

        var savedUser = await userRepository.CreateAppUserAsync(newUser);
        savedUser.Should().NotBeNull();
        savedUser.Email.Should().Be("newuser@example.com");
        context.Users.Count().Should().Be(4);
    }

    public async Task CreateAppUserAsync_InvalidEmail_Null()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);
        var newUser = new AppUser
        {
            UserName = "newuser",
            PasswordHash = "new_thing_hash",
            Email = null,
            Address = "987 Maple St",
            FullName = "New User"
        };

        Func<Task> act = async () => { await userRepository.CreateAppUserAsync(newUser); };

        await act.Should().ThrowAsync<DbUpdateException>()
            .WithMessage("Required properties 'Email' are missing for the instance of entity type 'AppUser'.");
    }


    [Fact]
    public async Task UpdateAppUserAsync_ValidUser_User()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);
        var existingUser = await context.Users.FindAsync(1);
        existingUser.UserName = "updateduser";
        existingUser.PasswordHash = "updated_thing_hash";
        existingUser.Email = "updateduser@example.com";
        existingUser.Address = "654 Pine St";
        existingUser.FullName = "Updated User";

        // Act
        var savedUser = await userRepository.UpdateAppUserAsync(existingUser);

        // Assert
        savedUser.Should().NotBeNull();
        savedUser?.Email.Should().Be("updateduser@example.com");
        context.Users.Count().Should().Be(3);
    }

    [Fact]
    public async Task DeleteAppUserAsync_ExistentUserId()
    {
        // Arrange
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);
        var appUser = await context.Users.FindAsync(1);

        // Act
        await userRepository.DeleteAppUserAsync(appUser);

        // Assert
        context.Users.Count().Should().Be(2);
    }

    [Fact]
    public async Task DeleteAppUserAsync_NonExistentUserId()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);
        var appUser = new AppUser { UserId = -1, UserName = "nonexistentuser", PasswordHash = "nonexistent_thing_hash", Email = "nonexistentuser@example.com", Address = "999 Oak St", FullName = "Nonexistent User" };
        
        userRepository.DeleteAppUserAsync(appUser);

        context.Users.Count().Should().Be(3);
    }
    [Fact]
    public async Task UpdateAppUserAsync_InvalidEmail_Null()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var updatedUser = new AppUser { UserId = 1, UserName = "updateduser", PasswordHash = "updated_thing_hash", Email = null, Address = "654 Pine St", FullName = "Updated User" };
        Func<Task> act = async () => { await userRepository.CreateAppUserAsync(updatedUser); };
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
    [Fact]
    public async Task GetAppUserByIdAsync_ExistentUserId_User()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.GetAppUserByIdAsync(1);

        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.UserName.Should().Be("testuser");
        result.Email.Should().Be("testuser@example.com");
        result.Address.Should().Be("123 Main St");
        result.FullName.Should().Be("Test User");

    }

    // Repeat the same pattern for other test methods

    [Fact]
    public async Task GetAppUserByIdAsync_NonExistentUserId_Null()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.GetAppUserByIdAsync(-1);

        result.Should().BeNull();

    }

    [Fact]
    public async Task GetAppUserByUserNameAsync_ExistentUserName_User()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.GetAppUserByUserNameAsync("testuser");

        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.UserName.Should().Be("testuser");
        result.Email.Should().Be("testuser@example.com");
        result.Address.Should().Be("123 Main St");
        result.FullName.Should().Be("Test User");

    }

    [Fact]
    public async Task GetAppUserByUserNameAsync_NonExistentUserName_Null()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.GetAppUserByUserNameAsync("nonexistentuser");

        result.Should().BeNull();

    }

    [Fact]
    public async Task GetAppUserByEmailAsync_ExistentEmail_User()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.GetAppUserByEmailAsync("testuser@example.com");

        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.UserName.Should().Be("testuser");
        result.Email.Should().Be("testuser@example.com");
        result.Address.Should().Be("123 Main St");
        result.FullName.Should().Be("Test User");

    }

    [Fact]
    public async Task GetAppUserByEmailAsync_NonExistentEmail_Null()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.GetAppUserByEmailAsync("nonexistentuser@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task IsUserExistAsync_ExistentEmail_True()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.IsUserExistAsync("testuser@example.com", "somename");

        result.Should().BeTrue();

    }

    [Fact]
    public async Task IsUserExistAsync_NonExistentEmail_False()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.IsUserExistAsync("nonexistentuser@example.com", "somename");

        result.Should().BeFalse();
    }


    [Fact]
    public async Task IsUserExistAsync_ExistentUserName_True()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.IsUserExistAsync("someemail", "testuser");

        result.Should().BeTrue();

    }
    [Fact]
    public async Task IsUserExistAsync_NonExistentUserName_False()
    {
        var context = await CreateContextAndSeedDatabase();
        var userRepository = await CreateUserRepositoryWithSeededData(context);

        var result = await userRepository.IsUserExistAsync("someemail", "nonexistentuser");

        result.Should().BeFalse();
    }
}
