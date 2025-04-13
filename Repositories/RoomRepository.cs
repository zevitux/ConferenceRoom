using ConferenceRoomApi.Data;
using ConferenceRoomApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomApi.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoomRepository> _logger;

    public RoomRepository(AppDbContext context, ILogger<RoomRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        try
        {
            return await _context.Rooms.ToListAsync();
        }
        catch (Exception)
        {
            _logger.LogError("Error getting rooms");
            throw;
        }
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        try
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception)
        {
            _logger.LogError("Error getting room with id: {RoomId}", id);
            throw;
        }
    }

    public async Task<Room> CreateRoomAsync(Room room)
    {
        try
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            _context.Entry(room).State = EntityState.Detached;
            return room;
        }
        catch (Exception)
        {
            _logger.LogError("Error adding room");
            throw;
        }
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingRoom = await _context.Rooms.FindAsync(room.Id);
            if (existingRoom == null)
                throw new KeyNotFoundException("Room doesn't exist");

            _context.Entry(existingRoom).CurrentValues.SetValues(room);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _context.Entry(room).State = EntityState.Detached;
            return existingRoom;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error updating room with ID: {RoomId}", room.Id);
            throw;
        }
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (room == null) return false;
            
            if(room.Bookings?.Any() == true)
                _context.Bookings.RemoveRange(room.Bookings);
            
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error deleting room with ID: {RoomId}", id);
            throw;
        }
    }
    
    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime start, DateTime end)
    {
        return await _context.Rooms
            .Where(room => !room.Bookings.Any(b => b.StartDate < end && b.EndDate > start)
            ).ToListAsync();
    }
}