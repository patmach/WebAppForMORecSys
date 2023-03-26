using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;
using Microsoft.AspNetCore.Identity;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Controllers
{

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Account> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<Account> userManager)
        {
        
            _context = context;
            _userManager = userManager;
            //MovielensLoader.LoadMovielensData(context);

        }

       
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", SystemParameters.Controller);
        }

        public async Task<IActionResult> UserBlockSettings()
        {
            return RedirectToAction("UserBlockSettings", SystemParameters.Controller);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}