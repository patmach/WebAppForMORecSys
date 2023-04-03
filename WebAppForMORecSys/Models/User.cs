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

        private List<int> BlockedItemIDs = null;

        public bool recomputeBlocked = true;

        public List<int> GetAllBlockedItems(IQueryable<Item> allItems)
        {
            if (recomputeBlocked)
            { 
                this.BlockedItemIDs = ComputeAllBlockedItems(allItems);
                recomputeBlocked = false;
            }
            return this.BlockedItemIDs;
        }

        private List<int> ComputeAllBlockedItems(IQueryable<Item> allItems)
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
