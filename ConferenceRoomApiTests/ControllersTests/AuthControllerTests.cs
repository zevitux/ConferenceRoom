using ConferenceRoomApi.Controllers;
using ConferenceRoomApi.DTOs.Auth;
using ConferenceRoomApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConferenceRoomApiTests.ControllersTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterShouldReturnOkWhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Rebeca",
                Email = "rebeca@example.com",
                Password = "Password123!",
                Role = "Admin"
            };

            var response = new AuthResponseDto
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token",
            };

            _mockAuthService
                .Setup(x => x.RegisterAsync(registerDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<AuthResponseDto>(actionResult.Value);
            Assert.Equal(response.AccessToken, returnedResponse.AccessToken);
            Assert.Equal(response.RefreshToken, returnedResponse.RefreshToken);
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenModelStateIsInvalid()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "",
                Email = "invalid-rebeca-email",
                Password = "123",
                Role = "InvalidRole"
            };
            _controller.ModelState.AddModelError("Name", "Name is required.");
            _controller.ModelState.AddModelError("Email", "Email is invalid.");
            _controller.ModelState.AddModelError("Password", "Password is too short.");
            _controller.ModelState.AddModelError("Role", "Role must be either 'Admin' or 'User'.");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(4, _controller.ModelState.ErrorCount);
        }

        [Fact]
        public async Task LoginShouldReturnOkWhenLoginIsSuccessful()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "rebeca@example.com",
                Password = "Password123!"
            };

            var response = new AuthResponseDto
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token"
            };

            _mockAuthService
                .Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<AuthResponseDto>(actionResult.Value);
            Assert.Equal(response.AccessToken, returnedResponse.AccessToken);
        }

        [Fact]
        public async Task RefreshtokenShouldReturnOkWhenTokenIsRefresh()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto
            {
                AccessToken = "expired-token",
                RefreshToken = "valid-refresh-token"
            };

            var response = new AuthResponseDto
            {
                AccessToken = "new-token",
                RefreshToken = "new-refresh-token"
            };

            _mockAuthService
                .Setup(x => x.RefreshTokenAsync(refreshTokenDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<AuthResponseDto>(actionResult.Value);
            Assert.Equal(response.AccessToken, returnedResponse.AccessToken);
        }

        [Fact]
        public async Task RefreshTokenShouldReturnUnauthorizedWhenRefreshFails()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto
            {
                AccessToken = "expired-token",
                RefreshToken = "invalid-refresh-token"
            };

            const string expectedErrorMessage = "Invalid refresh token";

            _mockAuthService
                .Setup(x => x.RefreshTokenAsync(refreshTokenDto))
                .ThrowsAsync(new Exception("Invalid refresh token"));

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(expectedErrorMessage, unauthorizedResult.Value);
        }
    }
}
