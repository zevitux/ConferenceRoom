using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomApi.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
