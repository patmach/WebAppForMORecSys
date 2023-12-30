using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Data.Cache;
using WebAppForMORecSys.Helpers.JSONPropertiesHandlers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Controllers
{
    /// <summary>
    /// Controller for the requests on actions related to ratings
    /// </summary>
    public class RatingsController : Controller
    {
        /// <summary>
        /// Database context
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// User manager for accesing acount the app communicates with
        /// </summary>
        private readonly UserManager<Account> _userManager;

        /// <summary>
        /// Gets connection to db and UserManager
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userManager">User manager for accesing acount the app communicates with</param>
        public RatingsController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Saves new user rating for a movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <param name="score">Rating score</param>
        /// <returns>HTTP response without content</returns>
        public IResult Save(int id, byte score)
        {
            User user = GetCurrentUser();
            SaveMethods.SaveRating(id, user.Id, score, _context);
            int ratingsCount = _context.Ratings.Where(r => (r.UserID == user.Id) && (r.RatingScore > 5)).Count();
            if ((score > 5) && (ratingsCount == SystemParameters.MinimalPositiveRatings))
                return Results.Content("MinimalPositiveRatingsDone");
            return Results.NoContent();
        }

        /// <summary>
        /// Deletes user rating of a movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <returns>HTTP response without content</returns>
        public IResult Remove(int id)
        {
            User user = GetCurrentUser();
            SaveMethods.RemoveRating(id, user.Id, _context);
            return Results.NoContent();
        }

        /// <summary>
        /// </summary>
        /// <returns>Currently logged user that sent this request.</returns>
        private User GetCurrentUser()
        {
            var account = _userManager.GetUserAsync(User).Result;
            User user = null;
            if (account != null)
            {
                user = _context.Users.Where(u => u.UserName == account.UserName).FirstOrDefault();
            }
            return user;
        }
    }
}
