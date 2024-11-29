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

        public UserProfile ValidateUserProfile(UserProfile profile)
        {
            if (profile.Gender != null && !Genders.Contains(profile.Gender))
            {
                profile.Gender = Genders[0];
            }
            if (profile.Sexuality != null && !Sexualities.Contains(profile.Sexuality))
            {
                profile.Sexuality = Sexualities[0];
            }

            return profile;
        }
    }
}
