﻿using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Messages")]
    public class BuddyMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Guid SenderId { get; set; }
        public Guid ChatLobbyId { get; set; }
        
        public BuddyMessage(string message, Guid userId, Guid chatLobbyId)
        {
            Id = Guid.NewGuid();
            Message = message;
            SenderId = userId;
            ChatLobbyId = chatLobbyId;
        }
    }
}
