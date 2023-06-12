using System.ComponentModel.DataAnnotations;

namespace nikhil.Models
{
    public class login
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
