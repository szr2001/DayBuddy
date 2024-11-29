using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Models
{
    public class UserProfile
    {
        [MaxLength(20, ErrorMessage = "Name too long")]
        [MinLength(5, ErrorMessage = "Name too short")]
        public string Name { get; set; }
        [MaxLength(5, ErrorMessage = "Maximum of 5 Interests")]
        public string[] Interests { get; set; } = [];
        [Range(18,150, ErrorMessage = "Age must be between 18 and 150")]
        public int Age { get; set; }
        public bool Premium { get; set; }
        public string Gender {  get; set; }
        public string Sexuality {  get; set; }
    }
}
