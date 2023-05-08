using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;
using Microsoft.AspNetCore.Identity;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Settings;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebAppForMORecSys.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> AppSettings()
        {
            MainViewModel viewModel = new MainViewModel();
            var rs = SystemParameters.RecommenderSystem;
            var metrics = await (_context.Metrics.Where(m => m.RecommenderSystemID == rs.Id).ToListAsync());
            for (int i = 0; i < metrics.Count; i++)
            {
                viewModel.Metrics.Add(metrics[i], 100 / metrics.Count());
            }
            viewModel.CurrentUser = GetCurrentUser();
            return View(viewModel);
        }

        public IActionResult SetMetricsView(int metricsview)
        {
            if ((metricsview < 0) || (metricsview >= Enum.GetValues(typeof(MetricsView)).Length))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetMetricsView(metricsview);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("AppSettings");

        }

        public IActionResult SetAddBlockRuleView(int addblockruleview)
        {
            if ((addblockruleview < 0) || (addblockruleview >= Enum.GetValues(typeof(AddBlockRuleView)).Length))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetAddBlockRuleView(addblockruleview);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("AppSettings");

        }

        public IActionResult SetMetricsColors(string[] metriccolor)
        {
            var rs = SystemParameters.RecommenderSystem;
            if ((metriccolor == null) ||(metriccolor.Length < 0) 
                || (metriccolor.Length != _context.Metrics.Where(m=> m.RecommenderSystemID == rs.Id).Count()))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetColors(metriccolor);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("AppSettings");
        }

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

        public IResult Hide(int id)
        {
            if (_context.Items.Where(m => m.Id == id).Count() == 0)
                return Results.NoContent();
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddItemToBlackList(id);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult Show(int id)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveItemFromBlackList(id);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}