using System.ComponentModel.DataAnnotations;

namespace CW10.DTOs.Responses
{
    public class ErrorResponse
    {
        [Required]
        public string Message { get; set; }
    }
}
