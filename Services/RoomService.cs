using ConferenceRoomApi.DTOs.Rooms;
using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using ConferenceRoomApi.Services.Interfaces;

namespace ConferenceRoomApi.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<RoomService> _logger;
    private static readonly List<string> _allowedEquipament = new() { "Projector", "Whiteboard", "VideoConference" };

    public RoomService(
        IRoomRepository roomRepository, 
        IBookingRepository bookingRepository,
        ILogger<RoomService> logger
        )
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }
    
    public async Task<List<RoomResponseDto>> GetAllRoomAsync()
    {
        try
        {
            var rooms = await _roomRepository.GetRoomsAsync();
            return rooms.Select(room => new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Equipment = new List<string>(room.Equipment)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rooms");
            throw;
        }
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(int id)
    {
        try
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);
            return room == null
                ? null
                : new RoomResponseDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Equipment = new List<string>(room.Equipment)
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room");
            throw;
        }
    }

    public async Task<List<RoomResponseDto?>> GetAvailableRoomsAsync(DateTime start, DateTime end)
    {
        try
        {
            ValidateTimeRange(start, end);

            var availableRooms = await _roomRepository.GetAvailableRoomsAsync(start, end);

            return availableRooms.Select(room => new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Equipment = new List<string>(room.Equipment)
            }).ToList<RoomResponseDto?>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available rooms");
            throw;
        }
    }

    public async Task<RoomResponseDto> CreateRoomAsync(RoomCreateDto dto)
    {
        try
        {
            ValidateEquipament(dto.Equipment);

            var room = new Room
            {
                Name = dto.Name,
                Capacity = dto.Capacity,
                Equipment = dto.Equipment.Distinct().ToList()
            };

            var createdRoom = await _roomRepository.CreateRoomAsync(room);

            return new RoomResponseDto()
            {
                Id = createdRoom.Id,
                Name = createdRoom.Name,
                Capacity = createdRoom.Capacity,
                Equipment = new List<string>(createdRoom.Equipment)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
            throw;
        }
    }

    public async Task<RoomResponseDto> UpdateRoomAsync(int id, RoomUpdateDto dto)
    {
        try
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);
            if (room == null)
                throw new KeyNotFoundException($"Room with id {id} not found");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                room.Name = dto.Name.Trim();

            if (dto.Capacity != room.Capacity)
            {
                await ValidateCapacityChange(id, dto.Capacity);
                room.Capacity = dto.Capacity;
            }

            if (dto.Equipment != null)
            {
                ValidateEquipament(dto.Equipment);
                room.Equipment = dto.Equipment.Distinct().ToList();
            }

            var uptadeRoom = await _roomRepository.UpdateRoomAsync(room);

            return new RoomResponseDto()
            {
                Id = uptadeRoom.Id,
                Name = uptadeRoom.Name,
                Capacity = uptadeRoom.Capacity,
                Equipment = new List<string>(uptadeRoom.Equipment)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating room {RoomId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        try
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);
            if (room == null)
                return false;

            if (await _bookingRepository.HasFutureBookingsForRoomAsync(id, DateTime.UtcNow))
                throw new InvalidOperationException("Room has future bookings");

            return await _roomRepository.DeleteRoomAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room {RoomId}", id);
            throw;
        }
    }

    #region Validation Helpers
    private void ValidateTimeRange(DateTime start, DateTime end)
    {
        if(start > end) throw new ArgumentException("Start date cannot be greater than end date");
        if(start < DateTime.UtcNow.AddMinutes(-5)) throw new ArgumentException("Start date cannot be in the past");
    }
    private void ValidateEquipament(List<string> equipament)
    {
        var invalid = equipament?.Except(_allowedEquipament).ToList();
        if(invalid?.Any() == true ) 
            throw new ArgumentException($"Invalid equipament: {string.Join(", ", invalid)}");
    }
    private async Task ValidateCapacityChange(int roomId, int newCapacity)
    {
        if (newCapacity < 0) throw new ArgumentException("Capacity cannot be negative");
        if(await _bookingRepository.HasFutureBookingsForRoomAsync(roomId, DateTime.UtcNow))
            throw new InvalidOperationException("Cannot change capacity with future bookings");
    }
    #endregion
}