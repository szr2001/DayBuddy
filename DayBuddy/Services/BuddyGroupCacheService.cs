using System.Collections.Concurrent;

namespace DayBuddy.Services
{
    /// <summary>
    /// An In memory cache of users and their groups used by BuddyMatchHub for transporting messages to and from users
    /// </summary>
    public class BuddyGroupCacheService
    {
        public int ActiveLobbiesCount => groups.Count;

        private readonly ConcurrentDictionary<string, string> users = [];
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> groups = [];

        public void AddUser(string userId, string groupId)
        {
            if (!users.TryAdd(userId, groupId))
            {
                throw new InvalidOperationException($"User '{userId}' already exists.");
            }

            groups.AddOrUpdate
            (
                groupId,
                _ => new ConcurrentBag<string> { userId },
                (_, bag) => { bag.Add(userId); return bag; }
            );
        }

        public void RemoveGroup(string groupId)
        {
            if (!groups.TryRemove(groupId, out var members))
            {
                throw new KeyNotFoundException($"Group '{groupId}' does not exist.");
            }

            foreach (var userId in members)
            {
                users.TryRemove(userId, out _);
            }
        }

        public int GetGroupMemberCount(string groupId)
        {
            return groups.TryGetValue(groupId, out var members) ? members.Count : 0;
        }

        public string[] GetUsersInGroup(string groupId)
        {
            if (!groups.TryGetValue(groupId, out var members))
            {
                return Array.Empty<string>();
            }
            return members.ToArray();
        }

        public void RemoveUser(string userId)
        {
            if (!users.TryRemove(userId, out var groupId))
            {
                throw new KeyNotFoundException($"User '{userId}' does not exist.");
            }

            if (groups.TryGetValue(groupId, out var members))
            {
                members = new ConcurrentBag<string>(members.Where(u => u != userId));
                if (members.IsEmpty)
                {
                    groups.TryRemove(groupId, out _);
                }
            }
        }

        public bool IsUserInGroup(string userId) => users.ContainsKey(userId);

        public string GetUserGroup(string userId)
        {
            if (!users.TryGetValue(userId, out var groupId))
            {
                throw new KeyNotFoundException($"User '{userId}' is not part of any group.");
            }
            return groupId;
        }
    }
}