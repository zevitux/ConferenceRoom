using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.DTOs.Rooms;

public class RoomUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [Range(0, 100)]
    public int Capacity { get; set; }
    public List<string>? Equipment { get; set; }
}