namespace DayBuddy.Models
{
    public class GroupChat
    {
        public UserProfile BuddyProfile { get; set; }
        public GroupMessage[] Messages { get; set; }
        
        public GroupChat(UserProfile buddyProfile, GroupMessage[] messages)
        {
            BuddyProfile = buddyProfile;
            Messages = messages;
        }
    }
}
