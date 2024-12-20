namespace DayBuddy.Models
{
    public class GroupMessage
    {
        public string SenderId { get; set; }
        public string Message { get; set; }
        
        public GroupMessage(string senderId, string message)
        {
            this.SenderId = senderId;
            this.Message = message;
        }
    }
}
