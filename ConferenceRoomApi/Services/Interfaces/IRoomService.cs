using ConferenceRoomApi.DTOs.Rooms;
using ConferenceRoomApi.Models;

namespace ConferenceRoomApi.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomResponseDto>> GetAllRoomAsync();
    Task<RoomResponseDto?> GetRoomByIdAsync(int id);
    Task<List<RoomResponseDto>> GetAvailableRoomsAsync(DateTime start, DateTime end);
    Task<RoomResponseDto> CreateRoomAsync(RoomCreateDto room);
    Task<RoomResponseDto> UpdateRoomAsync(int id, RoomUpdateDto room);
    Task<bool> DeleteRoomAsync(int id);
}