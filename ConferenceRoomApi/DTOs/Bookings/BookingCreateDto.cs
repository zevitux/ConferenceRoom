using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.DTOs.Bookings;

public class BookingCreateDto 
{ 
    [Required] 
    public int RoomId { get; set; } 
    [Required] public int UserId { get; set; } 
    [Required] public DateTime StartDate { get; set; } 
    [Required] public DateTime EndDate { get; set; } 
}