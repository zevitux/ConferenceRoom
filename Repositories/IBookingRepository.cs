using ConferenceRoomApi.Models;

namespace ConferenceRoomApi.Repositories;

public interface IBookingRepository
{
    Task<Booking> CreateBookingAsync(Booking booking);
    Task CancelBookingAsync(int bookingId);
    Task<bool> ExistsConflictAsync(int roomId, DateTime start, DateTime end);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<List<Booking>> GetBookingsByUserIdAsync(int userId);
    Task<bool> HasFutureBookingsForRoomAsync(int roomId, DateTime referenceTime);
}