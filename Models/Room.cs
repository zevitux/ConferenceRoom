using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.Models;

public class Room
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; }
    [Required, MaxLength(100)]
    public int Capacity { get; set; }
    [Required]
    public string Equipaments { get; set; }
    [Required]
    public bool IsUsed { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}