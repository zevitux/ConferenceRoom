using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required, MinLength(6)]
    public string PasswordHash { get; set; }
    [Required]
    public string Role { get; set; } //Admin and user

    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}