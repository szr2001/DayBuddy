using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Models
{
    public class User
    {
        [Required]
        [MaxLength(20, ErrorMessage ="Name too long")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        [MaxLength(80, ErrorMessage ="Email too long")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Display(Name ="Repeat Password")]
        public string RepeatPassword { get; set; }
    }
}
