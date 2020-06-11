using System.ComponentModel.DataAnnotations;

namespace CW10.DTOs.Responses
{
    public class GetEntrollmentResponse
    {
        [Required]
        public int IdEnrollment { get; set; }
        [Required]
        public int Semester { get; set; }
        [Required]
        public string StartDate { get; set; }
        [Required]
        public int IdStudy { get; set; }
    }
}