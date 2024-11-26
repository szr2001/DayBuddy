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
            if (!users.ContainsKey(userId))
            {
                users.Add(userId, groupId);
                if (!groups.ContainsKey(groupId))
                {
                    groups.Add(groupId, []);
                }

                groups[groupId].Add(userId);
            }
            throw new Exception($"userId already exist '{nameof(AddUser)}' inside {ToString}");
        }

        public void RemoveGroup(string groupId)
        {
            if (groups.ContainsKey(groupId))
            {
                foreach(string userId in groups[groupId])
                {
                    users.Remove(userId);
                }
                groups.Remove(groupId);
            }
            throw new Exception($"groupId does not exist when calling '{nameof(RemoveGroup)}' inside {ToString}");
        }

        public int GetGroupMemberCount(string groupId)
        {
            if (groups.ContainsKey(groupId))
            {
                return groups[groupId].Count;
            }
            throw new Exception($"groupId does not exist when calling '{nameof(GetGroupMemberCount)}' inside {ToString}");
        }

        public string[] GetUsersInGroup(string groupId) 
        {
            if (groups.ContainsKey(groupId))
            {
                return groups[groupId].ToArray();
            }
            throw new Exception($"groupId does not exist when calling '{nameof(GetUsersInGroup)}' inside {ToString}");
        }

        public void RemoveUser(string userId)
        {
            if (users.ContainsKey(userId))
            {
                string userGroup = users[userId];
                groups[userGroup].Remove(userId);
                if(groups.Count == 0)
                {
                    groups.Remove(userGroup);
                }
                users.Remove(userId);
            }
            throw new Exception($"userId does not exist '{nameof(RemoveUser)}' inside {ToString}");
        }

        public bool IsUserInGroup(string userId)
        {
            return users.ContainsKey(userId) && users[userId] != null;
        }

        public string GetUserGroup(string userId)
        {
            if (IsUserInGroup(userId))
            {
                return users[userId]!;
            }

            throw new Exception($"User lobby is null inside '{ToString}' for specified user when calling '{nameof(GetUserGroup)}'");
        }
    }
}
