using DayBuddy.Models;

namespace DayBuddy.Services
{
    public class UserCacheService
    {
        public int ActiveUsers { get; set; }

        private readonly Dictionary<string, string?> ActiveUsersAndLobbies = [];
        public void AddUser(string userId)
        {
            if (!ActiveUsersAndLobbies.ContainsKey(userId))
            {
                ActiveUsersAndLobbies.Add(userId, null);
                ActiveUsers++;
            }
        }
        
        public void RemoveUser(string userId)
        {
            if (ActiveUsersAndLobbies.ContainsKey(userId))
            {
                ActiveUsersAndLobbies.Remove(userId);
                ActiveUsers--;
            }
        }

        public void RemoveUserLobby(string userId)
        {
            if (ActiveUsersAndLobbies.ContainsKey(userId))
            {
                ActiveUsersAndLobbies[userId] = null;
            }
        }

        public void SetUserLobby(string userId, string lobbyId)
        {
            if (ActiveUsersAndLobbies.ContainsKey(userId))
            {
                ActiveUsersAndLobbies[userId] = lobbyId;
            }
        }

        public bool IsUserInLobby(string userId)
        {
            if (ActiveUsersAndLobbies.ContainsKey(userId))
            {
                if (ActiveUsersAndLobbies[userId] != null) return true;
            }

            return false;
        }

        public string GetUserLobby(string userId)
        {
            if (IsUserInLobby(userId))
            {
                return ActiveUsersAndLobbies[userId]!;
            }

            throw new Exception($"User lobby is null inside '{ToString}' for specified user when calling '{nameof(GetUserLobby)}'");
        }
    }
}
