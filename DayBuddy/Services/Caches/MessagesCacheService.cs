using DayBuddy.Models;
using System.Collections.Generic;

namespace DayBuddy.Services.Caches
{
    public class MessagesCacheService
    {
        //use a dictionary of groupID and a list of messages
        private Dictionary<Guid,List<BuddyMessage>> localCache = new();

        public void InsertMessage(Guid groupId, BuddyMessage message)
        {
            if (!localCache.ContainsKey(groupId))
            {
                localCache.Add(groupId, new List<BuddyMessage>());
            }

            localCache[groupId].Add(message);
        }

        public List<BuddyMessage> GetAllCache()
        {
            return localCache.Values.SelectMany(list => list).ToList();
        }

        public List<BuddyMessage> GetGroupCache(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return [];

            return localCache[groupId];
        }

        public void ClearCache(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return;

            localCache[groupId].Clear();
        }

        public void ClearAll()
        {
            localCache.Clear();
        }

        public int GetCacheSize(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return 0;

            return localCache[groupId].Count;
        }
    }
}
