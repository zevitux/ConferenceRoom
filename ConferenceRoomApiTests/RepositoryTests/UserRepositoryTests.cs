using ConferenceRoomApi.Models;
using ConferenceRoomApi.Data;
using ConferenceRoomApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConferenceRoomApiTests.RepositoryTests
{
    public class UserRepositoryTests
    {
        private readonly Mock<ILogger<UserRepository>> _mockLogger = new Mock<ILogger<UserRepository>>();

        private DbContextOptions<AppDbContext> GetInMemoryDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task GetAllUserAsyncShouldReturnAllUserWhenExits()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetAllUser");

            using (var context = new AppDbContext(options))
            {
                context.AddRange(
                    new User
                    {
                        Id = 1,
                        Name = "Rebeca",
                        Email = "rebeca@example.com",
                        PasswordHash = "1234",
                        Role = "Admin"
                    },
                    new User
                    {
                        Id = 2,
                        Name = "Eita",
                        Email = "eita@example.com",
                        PasswordHash = "12346",
                        Role = "User"
                    });
                await context.SaveChangesAsync();
            }

            //Act
            List<User> result;
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                result = await repository.GetAllUsersAsync();
            }

            //Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserByIdShouldReturnUserWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetUserById_Exists");
            const int userId = 1;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User
                {
                    Id = userId,
                    Name = "Rebeca",
                    Email = "rebeca@exmaple.com",
                    PasswordHash = "1234",
                    Role = "Admin"
                });
                await context.SaveChangesAsync();
            }

            //Act
            User? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                result = await repository.GetUserById(userId);
            }

            //Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserByIdShouldReturnNullWhenNotExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetUserById_NotExists");

            //Act
            User? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                result = await repository.GetUserById(999);
            }

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmailAsyncShouldReturnUserWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetUserByEmail");
            const string email = "exists@example.com";

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User
                {
                    Id = 1,
                    Name = "Rebeca",
                    Email = email,
                    PasswordHash = "1231234",
                    Role = "Admin"
                });
                await context.SaveChangesAsync();
            }

            //Act
            User? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                result = await repository.GetUserByEmailAsync(email);
            }

            //Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task AddAsyncShouldAddUserWithCorrectData()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("AddUser");
            var newUser = new User
            {
                Name = "Rebeca",
                Email = "newrebeca@example.com",
                PasswordHash = "1234",
                Role = "Admin"
            };

            //Act
            User result;
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                result = await repository.AddAsync(newUser);
            }

            //Assert
            using (var context = new AppDbContext(options))
            {
                var addedUser = await context.Users.FindAsync(result.Id);
                Assert.NotNull(addedUser);
                Assert.Equal(newUser.Name, addedUser.Name);
                Assert.Equal(newUser.Email, addedUser.Email);
                Assert.Equal(newUser.PasswordHash, addedUser.PasswordHash);
                Assert.Equal(newUser.Role, addedUser.Role);
            }
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateExistingUserWhenValidData()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("UpdateUser");
            var originalUser = new User
            {
                Id = 1,
                Name = "OriginalRebeca",
                Email = "originalrebecaTest@example",
                PasswordHash = "1234",
                Role = "Admin"
            };

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(originalUser);
                await context.SaveChangesAsync();
            }

            //Act
            var updatedUser = new User
            {
                Id = 1,
                Name = "UpdatedRebeca",
                Email = "updatedrebecaTest@example",
                PasswordHash = "123456",
                Role = "User"
            };
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                await repository.UpdateAsync(updatedUser);
            }

            //Assert
            using (var context = new AppDbContext(options))
            {
                var result = await context.Users.FindAsync(originalUser.Id);
                Assert.NotNull(result);
                Assert.Equal(updatedUser.Name, result.Name);
                Assert.Equal(updatedUser.Email, result.Email);
                Assert.Equal(updatedUser.PasswordHash, result.PasswordHash);
                Assert.Equal(updatedUser.Role, result.Role);
            }
        }

        [Fact]
        public async Task UpdateAsyncShouldThrowKeyNotFoundExceptionWhenUserNotExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("UpdateUser_NotExists");
            var nonExistingUser = new User
            {
                Id = 999,
                Name = "Rebeca",
                Email = "rebeca@example",
                PasswordHash = "123456",
                Role = "User"
            };

            //Act and Assert
            using (var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(nonExistingUser));
            }

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error updating user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsyncShouldLogErrorWhenExceptionOccurs()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("UpdateUser_Exception");
            var invalidUser = new User
            {
                Id = 1,
                Name = null!,
                Email = null!,
                PasswordHash = null!,
                Role = null!
            };
            
            //Act and Assert
            using(var context = new AppDbContext(options))
            {
                var repository = new UserRepository(context, _mockLogger.Object);
                await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(invalidUser));
            }

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error updating user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }
    }
}