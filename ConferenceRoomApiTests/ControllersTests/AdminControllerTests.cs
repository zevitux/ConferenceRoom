
using ConferenceRoomApi.Controllers;
using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ConferenceRoomApiTests.ControllersTests
{
    public class AdminControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _controller = new AdminController(_mockUserRepository.Object);
        }

        [Fact]
        public async Task GetAllUsersShouldReturnOkWithListOfUsers()
        {
            //Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Name = "Rebeca",
                    Email = "rebeca@example.com",
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Name = "Gta 8",
                    Email = "gta8@morralula.com",
                    Role = "User"
                }
            };

            _mockUserRepository
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            //Act
            var result = await _controller.GetAllUsers();

            //Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsType<List<User>>(actionResult.Value);
            Assert.Equal(2, returnedUsers.Count);
        }

        [Fact]
        public async Task GetUserByIdShouldReturnOkWithUser()
        {
            //Arrange
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Name = "Rebeca",
                Email = "rebeca@example.com",
                Role = "Admin"
            };

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            //Act
            var result = await _controller.GetUserById(userId);

            //Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(actionResult.Value);
            Assert.Equal(userId, returnedUser.Id);
        }

        [Fact]
        public async Task GetUSerByIdShouldReturnNotFoundWhenUserDoesNotExist()
        {
            //Arrange
            int userId = 1;

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))!
                .ReturnsAsync((User?)null);

            //Act
            var result = await _controller.GetUserById(userId);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddUSerShouldReturnCreatedAtActionWhenUserIsAdded()
        {
            //Arrange
            var user = new User
            {
                Id = 1,
                Name = "Rebeca",
                Email = "rebeca@example.com",
                Role = "User"
            };

            _mockUserRepository
                .Setup(x => x.AddAsync(user))
                .ReturnsAsync(user);

            //Act
            var result = await _controller.AddUser(user);

            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedUser = Assert.IsType<User>(actionResult.Value);
            Assert.Equal(user.Id, returnedUser.Id);
        }

        [Fact]
        public async Task UpdateUserShouldReturnOkWithUpdateUser()
        {
            //Arrange
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Name = "RebecaUpdated",
                Email = "updated@example.com",
                Role = "Admin"
            };

            _mockUserRepository
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(user);

            //Act
            var result = await _controller.UpdateUser(userId, user);

            //Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(actionResult.Value);
            Assert.Equal(userId, returnedUser.Id);
        }

        [Fact]
        public async Task UpdateUserShouldReturnBadRequestWhenIdMismatch()
        {
            //Arrange
            int userId = 1;
            var user = new User
            {
                Id = 2,
                Name = "Rebeca",
                Email = "rebeca@example.com",
                Role = "User"
            };

            //Act
            var result = await _controller.UpdateUser(userId, user);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUserShouldReturnNotFoundWhenUserDoesNotExist()
        {
            //Arrange
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Name = "Rebeca",
                Email = "rebeca@example.com",
                Role = "User"
            };

            _mockUserRepository
                .Setup(x => x.UpdateAsync(user))
                .ThrowsAsync(new KeyNotFoundException());

            //Act
            var result = await _controller.UpdateUser(userId, user);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
