using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    
    [Required]
    [RegularExpression(@"^(Admin|User)$", ErrorMessage = "Role must be either 'Admin' or 'User'.")]
    public string Role { get; set; } // Admin and user
}