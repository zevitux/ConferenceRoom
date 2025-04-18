using ConferenceRoomApi.Controllers;
using ConferenceRoomApi.DTOs.Bookings;
using ConferenceRoomApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ConferenceRoomApiTests.ControllersTests
{
    public class BookingControllerTests
    {
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly BookingController _controller;

        public BookingControllerTests()
        {
            _mockBookingService = new Mock<IBookingService>();
            _controller = new BookingController(_mockBookingService.Object);
        }

        [Fact]
        public async Task CreateBookingShouldReturnCreatedAtActionWhenBookingIsCreated()
        {
            // Arrange
            var bookingDto = new BookingCreateDto
            {
                RoomId = 1,
                UserId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1)
            };
            var createdBooking = new BookingResponseDto
            {
                Id = 1,
                RoomId = bookingDto.RoomId,
                UserId = bookingDto.UserId,
                StartDate = bookingDto.StartDate,
                EndDate = bookingDto.EndDate
            };
            
            _mockBookingService
                .Setup(x => x.CreateBookingAsync(bookingDto))
                .ReturnsAsync(createdBooking);

            // Act
            var result = await _controller.CreateBooking(bookingDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedBooking = Assert.IsType<BookingResponseDto>(actionResult.Value);
            Assert.Equal(createdBooking.Id, returnedBooking.Id);
        }

        [Fact]
        public async Task GetALlBookingsShouldReturnOkResultWithListOfBookings()
        {
            // Arrange
            var bookings = new List<BookingResponseDto>
            {
                new BookingResponseDto
                {
                    Id = 1,
                    RoomId = 1,
                    UserId = 1,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(1)
                },
                new BookingResponseDto
                {
                    Id = 2,
                    RoomId = 2,
                    UserId = 2,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(2)
                }
            };

            _mockBookingService
                .Setup(x => x.GetAllBookingsAsync())
                .ReturnsAsync(bookings);

            // Act
            var result = await _controller.GetAllBookings();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnedBookings = Assert.IsType<List<BookingResponseDto>>(actionResult.Value);
            Assert.Equal(2, returnedBookings.Count);
        }

        [Fact]
        public async Task GetBookingByIdShouldReturnNotFoundWhenBookingDoesNotExist()
        {
            //Arrange
            int bookingId = 1;

            _mockBookingService
                .Setup(x => x.GetBookingByIdAsync(bookingId))
                .ReturnsAsync((BookingResponseDto?)null);

            // Act
            var result = await _controller.GetBookingById(bookingId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CancelBookingShouldReturnNoContentWhenBookingIsCancelled()
        {
            // Arrange
            int bookingId = 1;
            _mockBookingService
                .Setup(x => x.CancelBookingAsync(bookingId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CancelBooking(bookingId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CancelBookingShouldReturnNotFoundWhenBookingDoesNotExist()
        {
            // Arrange
            int bookingId = 1;

            _mockBookingService
                .Setup(x => x.CancelBookingAsync(bookingId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CancelBooking(bookingId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
