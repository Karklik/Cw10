using System.ComponentModel.DataAnnotations;

namespace CW10.DTOs.Responses
{
    public class LoginResponse
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
