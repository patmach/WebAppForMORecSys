using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;

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

        public List<int> GetAllBlockedItems()
        {
            if (this.BlockedItemIDs == null)
            { 
                this.BlockedItemIDs = this.ComputeAllBlockedMovies();
            }
            return this.BlockedItemIDs;
        }

        public User()
        {

        }
    }

}
