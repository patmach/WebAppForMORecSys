using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Controllers
{
    public class UsersController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<Account> _userManager;


        public UsersController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index","Home");
        }

        public IActionResult Create(string userName, string returnUrl)
        {
            User user = new User { UserName = userName };
            _context.Add(user);
            _context.SaveChanges();
            if(returnUrl != null) 
                return LocalRedirect(returnUrl);
            return Ok();
        }


        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyUserName(Areas.Identity.Pages.Account.RegisterModel.InputModel input)
        {
            if (GetAll().Any(u=> u.UserName== input.UserName))
            {
                return Json($"User Name {input.UserName} is already in use.");
            }

            return Json(true);
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }
    }
}
