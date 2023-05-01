using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using WebAppForMORecSys.Controllers;
using WebAppForMORecSys.Data;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Cache
{
    public static class BlockedItemsCache
    {
        private static MemoryCache _cache = MemoryCache.Default;
        private static readonly TimeSpan _expiration = TimeSpan.FromMinutes(10);
        
        public static List<int> GetBlockedItemIdsForUser(string userId,ApplicationDbContext context)
        {
            if (_cache.Contains(userId))
            {
                return (List<int>)_cache.Get(userId);
            }
            var blockedItemIds = GetBlockedItemIdsFromDatabase(userId, context);
            _cache.Set(userId, blockedItemIds, new CacheItemPolicy { SlidingExpiration = _expiration });

            return blockedItemIds;
        }

        public static void RemoveBlockedItemIdsForUser(string userId)
        {
            if (_cache.Contains(userId))
                _cache.Remove(userId);
        }


        private static List<int> GetBlockedItemIdsFromDatabase(string userId, ApplicationDbContext context)
        {
            int id = -1;
            if(!int.TryParse(userId, out id))
                return new List<int>();
            User user = context.Users.Where(u => u.Id == id).FirstOrDefault();
            if (user == null)
                return new List<int>(); 
            return user.GetAllBlockedItems(context.Items).Select(item => item.Id).ToList();
        }
    }
}

