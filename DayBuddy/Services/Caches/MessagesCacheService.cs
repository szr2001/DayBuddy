using DayBuddy.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DayBuddy.Services.Caches
{
    public class MessagesCacheService
    {
        //use a dictionary of groupID and a list of messages
        private Dictionary<Guid, GroupMessageCache> localCache = new();

        public void InsertMessage(Guid groupId, BuddyMessage message)
        {
            CreateGroupCache(groupId);

            localCache[groupId].CacheMessages.Add(message);
        }

        public void CreateGroupCache(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId))
            {
                localCache.Add(groupId, new GroupMessageCache());
            }
        }

        public void SetGroupMessageCount(Guid groupId, int count)
        {
            CreateGroupCache(groupId);

            localCache[groupId].DbTotalMessages = count;
        }

        public List<BuddyMessage> GetAllCache()
        {
            return localCache.Values.SelectMany(GroupMessage => GroupMessage.CacheMessages).ToList();
        }

        public List<BuddyMessage> GetGroupCache(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return [];

            return localCache[groupId].CacheMessages;
        }

        public void RemoveCache(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return;

            localCache.Remove(groupId);
        }

        public void MarkCacheAsInsertedIntoDb(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return;
            localCache[groupId].DbTotalMessages += localCache[groupId].CacheMessages.Count;
            localCache[groupId].CacheMessages.Clear();
        }

        public void ClearAll()
        {
            localCache.Clear();
        }

        public int GetGroupDbMessageCount(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return 0;

            return localCache[groupId].DbTotalMessages;
        }

        public int GetGroupCacheSize(Guid groupId)
        {
            if (!localCache.ContainsKey(groupId)) return 0;

            return localCache[groupId].CacheMessages.Count;
        }
    }

    public class GroupMessageCache
    {
        public List<BuddyMessage> CacheMessages;
        public int DbTotalMessages;

        public GroupMessageCache()
        {
            CacheMessages = new();
        }
    }
}
