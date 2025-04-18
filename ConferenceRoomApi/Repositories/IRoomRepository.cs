using ConferenceRoomApi.Models;

namespace ConferenceRoomApi.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetRoomsAsync();
    Task<Room?> GetRoomByIdAsync(int id);
    Task<Room> CreateRoomAsync(Room room);
    Task<Room> UpdateRoomAsync(Room room);
    Task<bool> DeleteRoomAsync(int id);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime start, DateTime end);
}