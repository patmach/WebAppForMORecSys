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
    /// <summary>
    /// Class that represents the user that is interacting with the item
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique ID of the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique username of the user
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Rules specified by user on what items should be blocked
        /// </summary>
        public string? JSONBlockRules { get; set; }

        /// <summary>
        /// User used filters are saved there (the metric importances)
        /// </summary>
        public string? JSONFilter { get; set; }

        /// <summary>
        /// Contains user custom settings of the system
        /// </summary>
        public string? UserChoices { get; set; }

        public List<UserMetricVariants> UserMetricList { get; set; }

        /// <summary>
        /// All user's ratings
        /// </summary>
        public List<Rating> Ratings { get; set; }

        /// <summary>
        /// All user's interactions
        /// </summary>
        public List<Interaction> Interactions { get; set; }

        /// <summary>
        /// Account of this user if he has one
        /// </summary>
        public Account account;

        /// <summary>
        /// User's answers on questions
        /// </summary>
        public List<UserAnswer> UserAnswers { get; set; }  

        /// <summary>
        /// User's actions
        /// </summary>
        public List<UserAct> UserActs { get; set; }

        /// <summary>
        /// Recompute all blocked items by query to the database
        /// </summary>
        /// <param name="allItems">Link to all items stored in the database</param>
        /// <returns>All blocked items by the user</returns>
        /// <exception cref="NotImplementedException">
        /// If the app is configured for domain whose blocked items computation is not implemente yet.
        /// </exception>
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
