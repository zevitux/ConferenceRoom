using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.Models;

public class Booking
{
    [Key] public int Id { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public string Status { get; set; } //Confirmed and Canceled
}