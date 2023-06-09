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
    /// <summary>
    /// Cache on blocked items by users
    /// </summary>
    public static class BlockedItemsCache
    {
        /// <summary>
        /// Cache object
        /// </summary>
        private static MemoryCache _cache = MemoryCache.Default;

        /// <summary>
        /// Expiration time of data in cache
        /// </summary>
        private static readonly TimeSpan _expiration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// </summary>
        /// <param name="userId">Id of user whose blocked items should be returned</param>
        /// <param name="context">Database context</param>
        /// <returns>Blocked items by user from cache if present or database</returns>
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

        /// <summary>
        /// Remove user's blocked items from cache
        /// </summary>
        /// <param name="userId">Id of user whose record should be removed from cache</param>
        public static void RemoveBlockedItemIdsForUser(string userId)
        {
            if (_cache.Contains(userId))
                _cache.Remove(userId);
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">Id of user whose blocked items should be returned</param>
        /// <param name="context">Database context</param>
        /// <returns>List of blocked items from database</returns>
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

