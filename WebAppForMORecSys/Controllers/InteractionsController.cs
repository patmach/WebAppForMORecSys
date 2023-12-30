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
    /// Controller for the requests on actions related to interactions
    /// </summary>
    public class InteractionsController : Controller
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
        public InteractionsController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Saves new user interaction with an item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="type">Type of interaction</param>
        /// <returns>HTTP response without content</returns>
        public IResult Save(int id, TypeOfInteraction type)
        {
            User user = GetCurrentUser();
            SaveMethods.SaveInteraction(id, user.Id, type, _context);
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
