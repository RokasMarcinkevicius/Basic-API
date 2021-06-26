using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tamro.Controllers.DTOs;
using Tamro.Models;
using Xunit;

namespace Tamro.Tests
{
    [Collection("Integration Tests")]
    public class UsersControllerTests : IClassFixture<UsersControllerTests.DbSetup>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        // This class provides setup before all class tests, then clean-up after
        [Collection("Integration Tests")]
        public class DbSetup : IDisposable
        {
            private RosterDbContext _dbContext;

            public DbSetup(WebApplicationFactory<Startup> factory)
            {
                // This fetches the same single lifetime instantiation used by Controller classes
                _dbContext = factory.Services.GetRequiredService<RosterDbContext>();

                // Seed in-memory database with some data needed for tests
                var user = new User
                {
                    Id = 1,
                    Name = "Rokas",
                    Surname = "Marcinkevičius",
                    Email = "Rokas.m97@gmail.com",
                    Phone = "+37061957933"
                };
                _dbContext.User.Add(user);
                var user2 = new User
                {
                    Id = 2,
                    Name = "Michael",
                    Surname = "Scott",
                    Email = "Mscott@gmail.com",
                    Phone = "+37012345678"
                };
                _dbContext.User.Add(user2);
                var user3 = new User
                {
                    Id = 3,
                    Name = "Dwight",
                    Surname = "Schrute",
                    Email = "Dschrute@gmail.com",
                    Phone = "+37012345678"
                };
                _dbContext.User.Add(user3);
                var user4 = new User
                {
                    Id = 4,
                    Name = "Jim",
                    Surname = "Halpert",
                    Email = "Jhalpert@gmail.com",
                    Phone = "+37012345678"
                };
                _dbContext.User.Add(user4);
                _dbContext.SaveChanges();
            }

            public void Dispose()
            {
                var users = _dbContext.User.ToArray();
                _dbContext.User.RemoveRange(users);
                _dbContext.SaveChanges();
            }
        }

        public UsersControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetUser_ReturnsSuccessAndUser()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/users/1");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            var responseUser = JsonSerializer.Deserialize<UserDTO>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            Assert.NotNull(responseUser);
            Assert.Equal(1, responseUser.Id);
            Assert.Equal("Rokas", responseUser.Name);
            Assert.Equal("Marcinkevičius", responseUser.Surname);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/users/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsSuccessAndUsers()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/users");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            var responseUsers = JsonSerializer.Deserialize<IEnumerable<UserDTO>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            Assert.NotNull(responseUsers);
            Assert.Equal(4, responseUsers.Count());
            Assert.Contains(responseUsers, user => user.Id == 1);
            Assert.Contains(responseUsers, user => user.Id == 2);
        }

        [Fact]
        public async Task CreateUser_ReturnsSuccessNewUserAndLocationHeader()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userDto = new UserDTO
            {
                Id = 5,
                Name = "Gina",
                Surname = "Linetti"
            };
            var content = new StringContent(JsonSerializer.Serialize(userDto,
                new JsonSerializerOptions{IgnoreNullValues = true}), Encoding.UTF8, "application/json");

            try
            {
                // Act
                var response = await client.PostAsync("/users", content);

                // Assert
                response.EnsureSuccessStatusCode();
                Assert.NotNull(response.Content);
                var responseUser = JsonSerializer.Deserialize<UserDTO>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
                Assert.NotNull(responseUser);
                Assert.Equal(userDto.Id, responseUser.Id);
                Assert.Equal(userDto.Name, responseUser.Name);
                Assert.Equal(new Uri(client.BaseAddress, "/users/5"), response.Headers.Location);
            }
            finally
            {
                // Clean-up (so it doesn't mess up other tests)
                var dbContext = _factory.Services.GetRequiredService<RosterDbContext>();
                var user = await dbContext.User.FindAsync(userDto.Id);
                dbContext.User.Remove(user);
                await dbContext.SaveChangesAsync();
            }
        }

        [Theory]
        [InlineData(1, "Rosa", "Diaz", HttpStatusCode.Conflict)]  // Id already exists
        [InlineData(3, null, null, HttpStatusCode.BadRequest)]      // missing (null) Name
        [InlineData(3, "", "", HttpStatusCode.BadRequest)]        // missing (empty) Name
        public async Task CreateUser_ReturnsErrorCode(int id, string name, string surname, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient();
            var userDto = new UserDTO
            {
                Id = id,
                Name = name,
                Surname = surname,
            };
            var content = new StringContent(JsonSerializer.Serialize(userDto,
                new JsonSerializerOptions{IgnoreNullValues = true}), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/users", content);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userDto = new UserDTO
            {
                Id = 2,
                Name = "Amy",
                Surname = "Santiago"
            };
            var content = new StringContent(JsonSerializer.Serialize(userDto,
                new JsonSerializerOptions{IgnoreNullValues = true}), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync("/users/2", content);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [InlineData(2, 999, "Jake", "Peralta", HttpStatusCode.BadRequest)] // url and dto Id's don't match
        [InlineData(2, 2, null,  null, HttpStatusCode.BadRequest)]           // missing (null) Name
        [InlineData(2, 2, "",  "", HttpStatusCode.BadRequest)]             // missing (empty) Name
        [InlineData(999, 999, "Jake", "Peralta", HttpStatusCode.NotFound)] // User not found
        public async Task UpdateUser_ReturnsErrorCode(int urlId, int dtoId, string name, string surname, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient();
            var userDto = new UserDTO
            {
                Id = dtoId,
                Name = name,
                Surname = surname
            };
            var content = new StringContent(JsonSerializer.Serialize(userDto,
                new JsonSerializerOptions{IgnoreNullValues = true}), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync($"/users/{urlId}", content);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);

        }

        [Fact]
        public async Task DeleteUser_ReturnsSuccessAndUser()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dbContext = _factory.Services.GetRequiredService<RosterDbContext>();
            var user = new User
            {
                Id = 6,
                Name = "Jan",
                Surname = "Levinson"
            };
            dbContext.User.Add(user);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.DeleteAsync("/users/6");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            var responseUser = JsonSerializer.Deserialize<UserDTO>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            Assert.NotNull(responseUser);
            Assert.Equal(user.Id, responseUser.Id);
            Assert.Equal(user.Name, responseUser.Name);
            Assert.Equal(user.Surname, responseUser.Surname);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/users/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
