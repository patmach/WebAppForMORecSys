using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string? JSONBlockRules { get; set; }

        public string? JSONFilter { get; set; }

        public string? SearchHistory { get; set; }

        public List<UserMetric> UserMetricList { get; set; }

        public List<Rating> Ratings { get; set; }

        public List<Interaction> Interactions { get; set; }

        public Account account;
        /*
        public List<int> GetAllBlockedItems(DbSet<Item> allItems)
        {
            if (recomputeBlocked.ContainsKey(Id) && recomputeBlocked[Id])
                UpdateBlockedItems(allItems);
            return BlockedItemIDs.ContainsKey(Id) ? BlockedItemIDs[Id] : new List<int>();
        }

        public void UpdateBlockedItems(DbSet<Item> allItems)
        {
            if (!BlockedItemIDs.ContainsKey(Id))
            {
                BlockedItemIDs.TryAdd(Id, new List<int>());
            }
            lock (BlockedItemIDs[Id]) { 
                var updatedBlackList = ComputeAllBlockedItems(allItems);
                BlockedItemIDs[Id] = updatedBlackList;
            }
            recomputeBlocked[Id] = false;
        }

        private List<int> ComputeAllBlockedItems(DbSet<Item> allItems)
        {
            if (SystemParameters.Controller == "Movies")
            {
                return this.ComputeAllBlockedMovies(allItems);
            }
            throw new NotImplementedException();
        }

        public static Dictionary<int, bool> recomputeBlocked = new Dictionary<int, bool>();
        public static Dictionary<int, List<int>> BlockedItemIDs = new Dictionary<int, List<int>>();
        */

        public IQueryable<Item> GetAllBlockedItems(DbSet<Item> allItems)
        {
            if (SystemParameters.Controller == "Movies")
            {
                return this.ComputeAllBlockedMovies(allItems);
            }
            throw new NotImplementedException();
        }
        public User()
        {

        }
    }

}
