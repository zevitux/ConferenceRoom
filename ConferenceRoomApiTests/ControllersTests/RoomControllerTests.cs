using ConferenceRoomApi.Controllers;
using ConferenceRoomApi.DTOs.Rooms;
using ConferenceRoomApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConferenceRoomApiTests.ControllersTests
{
    public class RoomControllerTests
    {
        private readonly Mock<IRoomService> _mockRoomService;
        private readonly Mock<ILogger<RoomController>> _mockLogger;
        private readonly RoomController _controller;

        public RoomControllerTests()
        {
            _mockRoomService = new Mock<IRoomService>();
            _mockLogger = new Mock<ILogger<RoomController>>();
            _controller = new RoomController(_mockRoomService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllRoomsShouldReturnOkWithListOfRooms()
        {
            //Arrange
            var rooms = new List<RoomResponseDto>
            {
                new RoomResponseDto
                {
                    Id = 1,
                    Name = "Room A",
                    Capacity = 10,
                    Equipment = new List<string> { "Pojector" }
                },
                new RoomResponseDto
                {
                    Id = 2,
                    Name = "Room B",
                    Capacity = 20,
                    Equipment = new List<string> { "Whiteboard" }
                }
            };

            _mockRoomService
                .Setup(x => x.GetAllRoomAsync())
                .ReturnsAsync(rooms);

            //Act
            var result = await _controller.GetAllRooms();

            //Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedRooms = Assert.IsType<List<RoomResponseDto>>(actionResult.Value);
            Assert.Equal(2, returnedRooms.Count);

            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetRoomByIdShouldRetunNotFoundWhenRoomDoesNotExist()
        {
            //Arrange
            int roomId = 1;

            _mockRoomService
                .Setup(x => x.GetRoomByIdAsync(roomId))
                .ReturnsAsync((RoomResponseDto?)null);

            //Act
            var result = await _controller.GetRoomById(roomId);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Room with ID {roomId} not found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateRoomShouldReturnCreatedAtActionWhenRoomIsCreated()
        {
            //Arrange
            var roomDto = new RoomCreateDto
            {
                Name = "Room C",
                Capacity = 15,
                Equipment = new List<string> { "Projector" }
            };
            var createdRoom = new RoomResponseDto
            {
                Id = 1,
                Name = roomDto.Name,
                Capacity = roomDto.Capacity,
                Equipment = roomDto.Equipment
            };

            _mockRoomService
                .Setup(x => x.CreateRoomAsync(roomDto))
                .ReturnsAsync(createdRoom);

            //Act
            var result = await _controller.CreateRoom(roomDto);

            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedRoom = Assert.IsType<RoomResponseDto>(actionResult.Value);
            Assert.Equal(createdRoom.Id, returnedRoom.Id);

            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAvailableRoomsShouldReturnOkWithListOfAvailableRooms()
        {
            //Arrange
            var start = DateTime.Now;
            var end = DateTime.Now.AddHours(2);
            var availableRooms = new List<RoomResponseDto>
            {
                new RoomResponseDto
                {
                    Id = 1,
                    Name = "Room A",
                    Capacity = 10,
                    Equipment = new List<string> { "Projector" }
                },
                new RoomResponseDto
                {
                    Id = 2,
                    Name = "Room B",
                    Capacity = 20,
                    Equipment = new List<string> { "Whiteboard" }
                }
            };

            _mockRoomService
                .Setup(x => x.GetAvailableRoomsAsync(start, end))
                .ReturnsAsync(availableRooms);

            //Act
            var result = await _controller.GetAvailableRooms(start, end);

            //Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedRooms = Assert.IsType<List<RoomResponseDto>>(actionResult.Value);
            Assert.Equal(2, returnedRooms.Count);
        }

        [Fact]
        public async Task GetAvailableRoomsShouldReturnInternalServerErrorOnException()
        {
            //Arrange
            var start = DateTime.Now;
            var end = DateTime.Now.AddHours(2);

            _mockRoomService
                .Setup(x => x.GetAvailableRoomsAsync(start, end))
                .ThrowsAsync(new Exception("Database error"));

            //Act
            var result = await _controller.GetAvailableRooms(start, end);

            //Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
            Assert.Equal("Internal server error", actionResult.Value);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting available rooms")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateRoomShouldReturnOkWithUpdateRoom()
        {
            //Arrange
            int roomId = 1;
            var roomUpdateDto = new RoomUpdateDto
            {
                Name = "Updated Room",
                Capacity = 15,
                Equipment = new List<string> { "Projector", "Whiteboard" }
            };
            var updatedRoom = new RoomResponseDto
            {
                Id = roomId,
                Name = roomUpdateDto.Name,
                Capacity = roomUpdateDto.Capacity,
                Equipment = roomUpdateDto.Equipment
            };

            _mockRoomService
                .Setup(x => x.UpdateRoomAsync(roomId, roomUpdateDto))
                .ReturnsAsync(updatedRoom);

            //Act
            var result = await _controller.UpdateRoom(roomId, roomUpdateDto);

            //Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedRoom = Assert.IsType<RoomResponseDto>(actionResult.Value);
            Assert.Equal(updatedRoom.Id, returnedRoom.Id);
            Assert.Equal(updatedRoom.Name, returnedRoom.Name);
            Assert.Equal(updatedRoom.Capacity, returnedRoom.Capacity);

            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateRoomShouldReturnBadRequestWhenModelStateIsInvalid()
        {
            //Arrange
            int roomId = 1;
            var roomUpdateDto = new RoomUpdateDto
            {
                Name = null,
                Capacity = -5,
                Equipment = null
            };

            _controller.ModelState.AddModelError("Name", "Required");
            _controller.ModelState.AddModelError("Capacity", "Invalid range");

            //Act
            var result = await _controller.UpdateRoom(roomId, roomUpdateDto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);

            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
        [Fact]
        public async Task DeleteRoomShouldReturnNoContentWhenRoomIsDeleted()
        {
            //Arrange

            int roomId = 1;

            _mockRoomService
                .Setup(x => x.DeleteRoomAsync(roomId))
                .ReturnsAsync(true);

            //Act
            var result = await _controller.DeleteRoom(roomId);

            //Assert
            Assert.IsType<NoContentResult>(result);

            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteRoomShouldReturnNotFoundWhenRoomDoesNotExist()
        {
            //Arrange
            int roomId = 1;

            _mockRoomService
                .Setup(x => x.DeleteRoomAsync(roomId))
                .ReturnsAsync(false);

            //Act
            var result = await _controller.DeleteRoom(roomId);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Room with ID {roomId} not found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
