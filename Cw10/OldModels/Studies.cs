using System.ComponentModel.DataAnnotations;

namespace CW10.Models
{
    public class Studies
    {
        [Required]
        public int IdStudy { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
