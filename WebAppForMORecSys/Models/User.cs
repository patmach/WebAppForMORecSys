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

        public string? UserChoices { get; set; }

        public List<UserMetric> UserMetricList { get; set; }

        public List<Rating> Ratings { get; set; }

        public List<Interaction> Interactions { get; set; }

        public Account account;
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
