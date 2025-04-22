using ConferenceRoomApi.Data;
using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConferenceRoomApiTests.RepositoryTests
{
    public class BookingRepositoryTests
    {
        private readonly Mock<ILogger<BookingRepository>> _mockLogger = new Mock<ILogger<BookingRepository>>();
        private DbContextOptions<AppDbContext> GetInMemoryDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName : dbName)
                .Options;
        }

        [Fact]
        public async Task CreateBookingAsyncShouldAddBookingWithCorrectData()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("CreateBooking");
            var newBooking = new Booking
            {
                Id = 1,
                RoomId = 1,
                UserId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = "Confirmed"
            };
            
            //Act
            Booking result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.CreateBookingAsync(newBooking);
            }
            
            //Assert
            Assert.NotNull(result);
            Assert.Equal(newBooking.RoomId, result.RoomId);
            Assert.Equal(newBooking.UserId, result.UserId);
        }

        [Fact]
        public async Task GetAllBookingsAsyncShouldReturnAllBookingsWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetAllBookings");
            using (var context = new AppDbContext(options))
            {
                context.Bookings.AddRange(new Booking
                    {
                        Id = 1,
                        RoomId = 1,
                        UserId = 1,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddHours(2),
                        Status = "Confirmed"
                    },
                    new Booking
                    {
                        Id = 2,
                        RoomId = 2,
                        UserId = 2,
                        StartDate = DateTime.UtcNow.AddHours(1),
                        EndDate = DateTime.UtcNow.AddHours(3),
                        Status = "Confirmed"
                    });
                await context.SaveChangesAsync();
            }
            
            //Act
            List<Booking> result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.GetAllBookingsAsync();
            }
            
            //Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CancelBookingAsyncShouldRemoveBookingWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("CancelBooking");
            var bookingToDelete = new Booking
            {
                Id = 1,
                RoomId = 1,
                UserId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(2),
                Status = "Confirmed"
            };
            using (var context = new AppDbContext(options))
            {
                context.Bookings.Add(bookingToDelete);
                await context.SaveChangesAsync();
            }
            
            //Act
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                await repository.CancelBookingAsync(1);
            }
            
            //Assert
            using (var context = new AppDbContext(options))
            {
                var result = await context.Bookings.FindAsync(1);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task ExistsConflictAsyncShouldReturnTrueWhenConflictExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("ExistsConflict");
            var existingBooking = new Booking
            {
                Id = 1,
                RoomId = 1,
                UserId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(2),
                Status = "Confirmed"
            };

            using (var context = new AppDbContext(options))
            {
                context.Bookings.Add(existingBooking);
                await context.SaveChangesAsync();
            }
            
            //Act
            bool result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context,  _mockLogger.Object);
                result = await repository.ExistsConflictAsync(existingBooking.RoomId,
                    existingBooking.StartDate.AddMinutes(-30), 
                    existingBooking.EndDate.AddMinutes(30));
            }
            
            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsConflictAsyncShouldReturnFalseWhenNoConflictExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("ExistsConflict_NoConflict");
            var existingBooking = new Booking
            {
                Id = 1,
                RoomId = 1,
                UserId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(2),
                Status = "Confirmed"
            };
            using (var context = new AppDbContext(options))
            {
                context.Bookings.Add(existingBooking);
                await context.SaveChangesAsync();
            }
            
            //Act
            bool result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context,  _mockLogger.Object);
                result = await repository.ExistsConflictAsync(existingBooking.RoomId,
                    existingBooking.StartDate.AddMinutes(1),
                    existingBooking.EndDate.AddHours(3));
            }
            
            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetBookingByIdAsyncShouldReturnBookingWhenExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetBookingById");
            var existingBooking = new Booking
            {
                Id = 1,
                RoomId = 1,
                UserId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(2),
                Status = "Confirmed"
            };
            using (var context = new AppDbContext(options))
            {
                context.Bookings.Add(existingBooking);
                await context.SaveChangesAsync();
            }
            
            //Act
            Booking? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.GetBookingByIdAsync(existingBooking.Id);
            }
            
            //Assert
            Assert.NotNull(result);
            Assert.Equal(existingBooking.Id, result.Id);
        }

        [Fact]
        public async Task GetBookingByIdAsyncShouldReturnNullWhenNotExists()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetBookingById_NotExists");
            const int bookingId = 999;
            
            //Act
            Booking? result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.GetBookingByIdAsync(bookingId);
            }
            
            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBookingByUserIdAsyncShouldReturnUserBookings()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("GetBookingByUserId");
            var userId = 1;
            using (var context = new AppDbContext(options))
            {
                context.Bookings.AddRange(
                    new Booking
                    {
                        Id = 1, RoomId = 1, UserId = userId, StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddHours(2), Status = "Confirmed"
                    },
                    new Booking
                    {
                        Id = 2, RoomId = 2, UserId = userId, StartDate = DateTime.UtcNow.AddHours(1),
                        EndDate = DateTime.UtcNow.AddHours(3), Status = "Confirmed"
                    },
                    new Booking
                    {
                        Id = 3, RoomId = 3, UserId = 2, StartDate = DateTime.UtcNow.AddHours(2),
                        EndDate = DateTime.UtcNow.AddHours(4), Status = "Confirmed"
                    }
                );
                await context.SaveChangesAsync();
            }
            
            //Act
            List<Booking> result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.GetBookingsByUserIdAsync(userId);
            }
            
            //Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task HasFutureBookingsForRoomAsyncShouldReturnTrueWhenFutureBookingsExist()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions("HasFutureBookings");
            var roomId = 1;
            var futureBooking = new Booking
            {
                Id = 1,
                RoomId = roomId,
                UserId = 1,
                StartDate = DateTime.UtcNow.AddHours(1),
                EndDate = DateTime.UtcNow.AddHours(2),
                Status = "Confirmed"
            };

            using (var context = new AppDbContext(options))
            {
                context.Bookings.Add(futureBooking);
                await context.SaveChangesAsync();
            }
            
            //Act
            bool result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.HasFutureBookingsForRoomAsync(roomId, DateTime.UtcNow);
            }
            
            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasFutureBookingsForRoomAsyncShouldReturnFalseWhenNoFutureBookingsExist()
        {
            //Arrange
            var options = GetInMemoryDbContextOptions("HasFutureBookings_NoFutureBookings");
            var roomId = 1;
            var pastBooking = new Booking
            {
                Id = 1,
                RoomId = roomId,
                UserId = 1,
                StartDate = DateTime.UtcNow.AddHours(-2),
                EndDate = DateTime.UtcNow.AddHours(-1),
                Status = "Confirmed"
            };

            using (var context = new AppDbContext(options))
            {
                context.Bookings.Add(pastBooking);
                await context.SaveChangesAsync();
            }
            
            //Act
            bool result;
            using (var context = new AppDbContext(options))
            {
                var repository = new BookingRepository(context, _mockLogger.Object);
                result = await repository.HasFutureBookingsForRoomAsync(roomId, DateTime.UtcNow);
            }
            
            //Assert
            Assert.False(result);
        }
    }
}