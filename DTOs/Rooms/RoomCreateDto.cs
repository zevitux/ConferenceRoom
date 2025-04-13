using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.DTOs.Rooms;

public class RoomCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0, 100)]
    public int Capacity { get; set; }

    public List<string> Equipment { get; set; } = new();
}