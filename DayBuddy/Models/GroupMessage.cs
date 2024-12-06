namespace DayBuddy.Models
{
    public class GroupMessage
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        
        public GroupMessage(string sender, string message)
        {
            this.Sender = sender;
            this.Message = message;
        }
    }
}
