using ConferenceRoomApi.DTOs.Bookings;
using ConferenceRoomApi.Models;

namespace ConferenceRoomApi.Services.Interfaces;

public interface IBookingService
{
    Task<List<BookingResponseDto>> GetAllBookingsAsync();
    Task<BookingResponseDto> CreateBookingAsync(BookingCreateDto bookingDto);
    Task<bool> CancelBookingAsync(int bookingId);
    Task<bool> ExistsConflictAsync(int roomId, DateTime start, DateTime end);
    Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId);
    Task<List<BookingResponseDto>> GetBookingsByUserIdAsync(int userId);
}