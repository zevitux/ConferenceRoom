using ConferenceRoomApi.Data;
using ConferenceRoomApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomApi.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(AppDbContext context, ILogger<BookingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        try
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for Room {RoomId} by User {UserId}", booking.RoomId, booking.UserId);
            throw;
        }
    }
    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        try
        {
            return await _context.Bookings.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bookings");
            throw;
        }
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Booking {BookingId} was not found", bookingId);
                return;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Booking {BookingId} was deleted", bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling booking with ID {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<bool> ExistsConflictAsync(int roomId, DateTime start, DateTime end)
    {
        try
        {
            return await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                (start < b.EndDate && end > b.StartDate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking conflicts for Room {RoomId} between {Start} and {End}", roomId, start, end);
            throw;
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        try
        {
            return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking with ID {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<List<Booking>> GetBookingsByUserIdAsync(int userId)
    {
        try
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasFutureBookingsForRoomAsync(int roomId, DateTime referenceTime)
    {
        return await _context.Bookings
            .AnyAsync(b =>
                b.RoomId == roomId && 
                b.StartDate >= referenceTime);
    }
}