using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Controllers
{
    public class UsersController : Controller
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
        public UsersController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// There is no Index page for user - redirects to home page
        /// </summary>
        /// <returns>Home Page</returns>
        public IActionResult Index()
        {
            return RedirectToAction("Index","Home");
        }

        /// <summary>
        /// Createnew instance of user, called when account is created
        /// </summary>
        /// <param name="userName">Identificator of user that correspond to the username she/he is logging in with</param>
        /// <param name="returnUrl">To what page is user redirected after creating his instance</param>
        /// <returns>The page from returnUrl parameter or code 200</returns>
        public IActionResult Create(string userName, string returnUrl)
        {
            User user = new User { UserName = userName };
            _context.Add(user);
            _context.SaveChanges();
            user.SetRandomSettingsForNewUser();
            if(returnUrl != null) 
                return LocalRedirect(returnUrl);
            return Ok();
        }


        /// <summary>
        /// Checks if username is not already taken.
        /// </summary>
        /// <param name="input">Input model from the register form</param>
        /// <returns>Json response with result of the check</returns>
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyUserName(Areas.Identity.Pages.Account.RegisterModel.InputModel input)
        {
            if (GetAll().Any(u=> u.UserName== input.UserName))
            {
                return Json($"User Name {input.UserName} is already in use.");
            }

            return Json(true);
        }

        /// <summary>
        /// </summary>
        /// <returns>All users from database</returns>
        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }
    }
}
