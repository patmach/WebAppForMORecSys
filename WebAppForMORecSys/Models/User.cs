using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
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

        public List<int> GetAllBlockedItems(IQueryable<Item> allItems)
        {
            if (recomputeBlocked.ContainsKey(Id) && recomputeBlocked[Id])
            {
                if (!BlockedItemIDs.ContainsKey(Id))
                {
                    BlockedItemIDs.Add(Id,ComputeAllBlockedItems(allItems));
                }
                else
                {
                    BlockedItemIDs[Id] = ComputeAllBlockedItems(allItems);
                }
                recomputeBlocked[Id] = false; 
            }
            return BlockedItemIDs.ContainsKey(Id) ? BlockedItemIDs[Id] : new List<int>();
        }

        private List<int> ComputeAllBlockedItems(IQueryable<Item> allItems)
        {
            if (SystemParameters.Controller == "Movies")
            {
                return this.ComputeAllBlockedMovies(allItems);
            }
            throw new NotImplementedException();
        }

        public static Dictionary<int, bool> recomputeBlocked = new Dictionary<int, bool>();
        public static Dictionary<int, List<int>> BlockedItemIDs = new Dictionary<int, List<int>>();

        public User()
        {

        }
    }

}
