namespace DayBuddy.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string[] Interests { get; set; } = [];
        public int Age { get; set; }
        public bool Premium { get; set; }
        public UserProfile(string name, string[] interests, int age, bool premium)
        {
            Name = name;
            Interests = interests;
            Age = age;
            Premium = premium;
        }
    }
}
