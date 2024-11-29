using DayBuddy.Models;

namespace DayBuddy.Services
{
    public class UserProfileValidatorService
    {
        private readonly IConfiguration _config;

        public string[] Interests { get; private set; }
        public string[] Genders { get; private set; }
        public string[] Sexualities { get; private set; }
        public UserProfileValidatorService(IConfiguration config)
        {
            _config = config;
            Interests = _config.GetSection("Interests").Get<string[]>()!;
            Genders = _config.GetSection("Genders").Get<string[]>()!;
            Sexualities = _config.GetSection("Sexualities").Get<string[]>()!;
        }

        public bool IsProfileValid(UserProfile profile)
        {
            return true;
        }
    }
}
