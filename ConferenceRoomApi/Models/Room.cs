using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.Models;

public class Room
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; }
    [Required, Range(0, 100)]
    public int Capacity { get; set; }

    [Required] 
    public List<string> Equipment { get; set; } = new();
    [Required] 
    public bool IsUsed { get; set; } = true;
    public List<Booking> Bookings { get; set; } = new();
}