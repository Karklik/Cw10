using System.ComponentModel.DataAnnotations;

namespace CW10.DTOs.Requests
{
    public class PromoteStudentsRequest
    {
        [Required]
        public string Studies { get; set; }
        [Required]
        public int Semester { get; set; }
    }
}
