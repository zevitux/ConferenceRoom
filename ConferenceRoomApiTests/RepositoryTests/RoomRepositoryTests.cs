using ConferenceRoomApi.Data;
using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConferenceRoomApiTests.RepositoryTests
{
    public class RoomRepositoryTests
    {
        private readonly Mock<ILogger<RoomRepository>> _mockLogger = new Mock<ILogger<RoomRepository>>();

        private DbContextOptions<AppDbContext> GetInMemoryDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task GetRoomsAsyncShouldReturnAllRoomsWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetRooms");
            using(var context = new AppDbContext(options))
            {
                context.Rooms.AddRange(new Room 
                { 
                    Id = 1,
                    Name = "Room A",
                    Capacity = 10,
                    Equipment = new List<string> { "Projector", "Whiteboard" },
                    IsUsed = false
                });
                context.Rooms.Add(new Room 
                { 
                    Id = 2, 
                    Name = "Room B",
                    Capacity = 20,
                    Equipment = new List<string> { "Whiteboard" },
                    IsUsed = true
                });
                await context.SaveChangesAsync();
            }

            //Act
            List<Room> result;
            using(var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.GetRoomsAsync();
            }

            //Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetRoomByIdAsyncShouldReturnRoomWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetRoomById");
            const int roomId = 1;

            using (var context = new AppDbContext(options))
            {
                context.Rooms.Add(new Room
                {
                    Id = roomId,
                    Name = "Room A",
                    Capacity = 10,
                    Equipment = new List<string> { "Projector", "Whiteboard" },
                    IsUsed = false
                });
                await context.SaveChangesAsync();
            }
            //Act
            Room? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.GetRoomByIdAsync(roomId);
            }
            //Assert
            Assert.NotNull(result);
            Assert.Equal(roomId, result.Id);
        }

        [Fact]
        public async Task GetRoomByIdAsyncShouldReturnNullWhenNotExists()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions("GetRoomById_NotExists");

            // Act
            Room? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.GetRoomByIdAsync(999);
            }

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateRoomAsyncShouldReturnAddRoomWithCorrectData()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("CreateRoom");
            var newRoom = new Room
            {
                Name = "Room A",
                Capacity = 10,
                Equipment = new List<string> { "Projector", "Whiteboard" },
                IsUsed = false
            };

            //Act
            Room result;
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.CreateRoomAsync(newRoom);
            }

            //Assert
            Assert.NotNull(result);
            Assert.Equal(newRoom.Name, result.Name);
        }

        [Fact]
        public async Task UpdateRoomAsyncShouldUpdateExistingRoomWhenValidData()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("UpdateRoom");
            var originalRoom = new Room
            {
                Id = 1,
                Name = "Room A",
                Capacity = 10,
                Equipment = new List<string> { "Projector", "Whiteboard" },
                IsUsed = false
            };

            using (var context = new AppDbContext(options))
            {
                context.Rooms.Add(originalRoom);
                await context.SaveChangesAsync();
            }

            //Act
            var updatedRoom = new Room
            {
                Id = 1,
                Name = "Room A Updated",
                Capacity = 15,
                Equipment = new List<string> { "Projector", "Whiteboard", "TV" },
                IsUsed = true
            };
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                await repository.UpdateRoomAsync(updatedRoom);
            }

            //Assert
            using(var context = new AppDbContext(options))
            {
                var room = await context.Rooms.FindAsync(1);
                Assert.NotNull(room);
                Assert.Equal(updatedRoom.Name, room.Name);
                Assert.Equal(updatedRoom.Capacity, room.Capacity);
                Assert.Equal(updatedRoom.Equipment, room.Equipment);
                Assert.Equal(updatedRoom.IsUsed, room.IsUsed);
            }
        }

        [Fact]
        public async Task UpdateRoomAsyncShouldThrowKeyExceptionWhenRoomNotExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("UpdateRoom_NotExists");
            var nonExistingRoom = new Room
            {
                Id = 999,
                Name = "Room A Nie",
                Capacity = 15,
                Equipment = new List<string> { "Projector", "Whiteboard", "TV" },
                IsUsed = true
            };

            //Act and Assert
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateRoomAsync(nonExistingRoom));
            }

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error updating room")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Fact]
        public async Task DeleteRoomAsyncShouldRemoveRoomWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("DeleteRoom");
            const int roomId = 1;
            var roomToDelete = new Room
            {
                Id = roomId,
                Name = "Room A",
                Capacity = 10,
                Equipment = new List<string> { "Projector", "Whiteboard" },
                IsUsed = false
            };
            using (var context = new AppDbContext(options))
            {
                context.Rooms.Add(roomToDelete);
                await context.SaveChangesAsync();
            }

            //Act
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                await repository.DeleteRoomAsync(roomId);
            }

            //Assert
            using (var context = new AppDbContext(options))
            {
                var result = await context.Rooms.FindAsync(roomId);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task DeleteRoomAsyncShouldReturnFalseWhenRoomNotExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("DeleteRoom_NotExists");
            const int roomId = 999;

            //Act
            bool result;
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.DeleteRoomAsync(roomId);
            }

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAvailableRoomsAsyncShouldReturnRoomsWithoutBookings()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetAvailableRooms");
            var now = DateTime.UtcNow;

            var room1 = new Room
            {
                Id = 1,
                Name = "Room A",
                Capacity = 10,
                Equipment = new List<string> { "Projector", "Whiteboard" },
                IsUsed = false
            };

            var room2 = new Room
            {
                Id = 2,
                Name = "Room B",
                Capacity = 20,
                Equipment = new List<string> { "Whiteboard" },
                IsUsed = true
            };

            var booking1 = new Booking
            {
                Id = 1,
                RoomId = 1,
                StartDate = now,
                EndDate = now.AddHours(2),
                Status = "Confirmed"
            };

            using (var context = new AppDbContext(options))
            {
                context.Rooms.AddRange(room1, room2);
                context.Bookings.Add(booking1);
                await context.SaveChangesAsync();
            } 

            var start = now.AddHours(1);
            var end = now.AddHours(3);

            //Act
            List<Room> result;
            using (var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.GetAvailableRoomsAsync(start, end);
            }

            //Assert
            Assert.Single(result);
            Assert.Contains(result, r => r.Name == "Room B");
        }

        [Fact]
        public async Task GetAvailableRoomsAsyncShouldReturnAllRoomsWhenNoBookings()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetAvailableRooms_NoBookings");

            var room1 = new Room
            {
                Id = 1,
                Name = "Room A",
                Capacity = 10,
                Equipment = new List<string> { "Projector", "Whiteboard" },
                IsUsed = false
            };
            var room2 = new Room
            {
                Id = 2,
                Name = "Room B",
                Capacity = 20,
                Equipment = new List<string> { "Whiteboard" },
                IsUsed = true
            };

            using (var context = new AppDbContext(options))
            {
                context.Rooms.AddRange(room1, room2);
                await context.SaveChangesAsync();
            }

            var start = DateTime.UtcNow;
            var end = DateTime.UtcNow.AddHours(1);

            //Act
            List<Room> result;
            using(var context = new AppDbContext(options))
            {
                var repository = new RoomRepository(context, _mockLogger.Object);
                result = await repository.GetAvailableRoomsAsync(start, end);
            }

            //Assert
            Assert.Equal(2, result.Count);
        }
    }
}
