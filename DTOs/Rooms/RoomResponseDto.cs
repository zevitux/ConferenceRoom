using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ConferenceRoomApi.DTOs.Rooms;

public class RoomResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public List<string> Equipment { get; set; }
}