using System.Transactions;
using ConferenceRoomApi.DTOs.Bookings;
using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using ConferenceRoomApi.Services.Interfaces;

namespace ConferenceRoomApi.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository bookingRepository, ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }
    public async Task<List<BookingResponseDto>> GetAllBookingsAsync()
    {
        try
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync();

            var bookingResponseDtos = bookings.Select(booking => new BookingResponseDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                UserId = booking.UserId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate
            }).ToList();

            return bookingResponseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error received all bookings");
            throw;
        }
    }

    public async Task<BookingResponseDto> CreateBookingAsync(BookingCreateDto bookingDto)
    {
        try
        {
            var booking = new Booking
            {
                RoomId = bookingDto.RoomId,
                UserId = bookingDto.UserId,
                StartDate = bookingDto.StartDate,
                EndDate = bookingDto.EndDate
            };

            var createdBooking = await _bookingRepository.CreateBookingAsync(booking);

            return new BookingResponseDto()
            {
                Id = createdBooking.Id,
                RoomId = createdBooking.RoomId,
                UserId = createdBooking.UserId,
                StartDate = createdBooking.StartDate,
                EndDate = createdBooking.EndDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for RoomId: {RoomId}, UserId: {UserId}", bookingDto.RoomId, bookingDto.UserId);
            throw;
        }
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            await _bookingRepository.CancelBookingAsync(bookingId);
            transaction.Complete();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking");
            throw;
        }
    }

    public Task<bool> ExistsConflictAsync(int roomId, DateTime start, DateTime end)
    {
        try
        {
            return _bookingRepository.ExistsConflictAsync(roomId, start, end);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error existing conflict");
            throw;
        }
    }

    public async Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId)
    {
        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return booking == null
                ? null
                : new BookingResponseDto
                {
                    Id = booking.Id,
                    RoomId = booking.RoomId,
                    UserId = booking.UserId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking");
            throw;
        }
    }

    public async Task<List<BookingResponseDto>> GetBookingsByUserIdAsync(int userId)
    {
        try
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);

            var bookingsResponseDtos = bookings.Select(booking => new BookingResponseDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                UserId = booking.UserId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate
            }).ToList();

            return bookingsResponseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings");
            throw;
        }
    }
}