using DayBuddy.Models;
using System.Text.RegularExpressions;

namespace DayBuddy.Services
{
    public class BuddyGroupCacheService
    {
        public int ActiveLobbiesCount { get { return users.Count; } }    
        
        private readonly Dictionary<string, string> users = [];
        private readonly Dictionary<string, List<string>> groups = [];
        public void AddUser(string userId, string groupId)
        {
            if (users.ContainsKey(userId))
            {
                throw new Exception($"userId already exist '{nameof(AddUser)}' inside {ToString}");
            }

            users.Add(userId, groupId);
            if (!groups.ContainsKey(groupId))
            {
                groups.Add(groupId, []);
            }
            groups[groupId].Add(userId);
        }

        public void RemoveGroup(string groupId)
        {
            if (!groups.ContainsKey(groupId))
            {
                throw new Exception($"groupId does not exist when calling '{nameof(RemoveGroup)}' inside {ToString}");
            }
            
            foreach(string userId in groups[groupId])
            {
                users.Remove(userId);
            }
            groups.Remove(groupId);
        }
        
        public int GetGroupMemberCount(string groupId)
        {
            if (!groups.ContainsKey(groupId))
            {
                throw new Exception($"groupId does not exist when calling '{nameof(GetGroupMemberCount)}' inside {ToString}");
            }
            
            return groups[groupId].Count;
        }

        public string[] GetUsersInGroup(string groupId) 
        {
            if (!groups.ContainsKey(groupId))
            {
                throw new Exception($"groupId does not exist when calling '{nameof(GetUsersInGroup)}' inside {ToString}");
            }

            return groups[groupId].ToArray();
        }

        public void RemoveUser(string userId)
        {
            if (!users.ContainsKey(userId))
            {
                throw new Exception($"userId does not exist '{nameof(RemoveUser)}' inside {ToString}");
            }
            
            string userGroup = users[userId];
            groups[userGroup].Remove(userId);
            if(groups.Count == 0)
            {
                groups.Remove(userGroup);
            }
            users.Remove(userId);
        }

        public bool IsUserInGroup(string userId)
        {
            return users.ContainsKey(userId) && users[userId] != null;
        }

        public string GetUserGroup(string userId)
        {
            if (!IsUserInGroup(userId))
            {
                throw new Exception($"User lobby is null inside '{ToString}' for specified user when calling '{nameof(GetUserGroup)}'");
            }

            return users[userId]!;
        }
    }
}
