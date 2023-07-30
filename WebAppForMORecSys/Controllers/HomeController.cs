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
using WebAppForMORecSys.Cache;

namespace WebAppForMORecSys.Controllers
{
    [Authorize]
    public class HomeController : Controller
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
        /// Gets connection to db and UserManager, saves possible values of movie properties
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userManager">User manager for accesing acount the app communicates with</param>
        public HomeController(ApplicationDbContext context, UserManager<Account> userManager)
        {
        
            _context = context;
            _userManager = userManager;
            //MovielensLoader.LoadMovielensData(context);
        }

       
        /// <summary>
        /// Redirects to used controller
        /// </summary>
        /// <returns>The main page of used controller</returns>
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", SystemParameters.Controller);
        }

        /// <summary>
        /// Redirects to used controller
        /// </summary>
        /// <returns>The main page of management of user blocks</returns>
        public async Task<IActionResult> UserBlockSettings()
        {
            return RedirectToAction("UserBlockSettings", SystemParameters.Controller);
        }

        /// <summary>
        /// Prepares data for the custom setting page and then return the page.
        /// </summary>
        /// <returns>Page where user can make custom changes to the system.</returns>
        public async Task<IActionResult> AppSettings()
        {
            MainViewModel viewModel = new MainViewModel();
            var rs = SystemParameters.RecommenderSystem;
            var metrics = await (_context.Metrics.Include(m=> m.metricVariants).Where(m => m.RecommenderSystemID == rs.Id)
                .ToListAsync());
            viewModel.User = GetCurrentUser();
            var blockedItems = BlockedItemsCache.GetBlockedItemIdsForUser(viewModel.User.Id.ToString(), _context);
            viewModel.UserRatings = await (from rating in _context.Ratings
                                                  where rating.UserID == viewModel.User.Id
                                                  select rating).ToListAsync();
            viewModel.Items = _context.Items.Where(item=> !blockedItems.Contains(item.Id)).Take(5);
            viewModel.SetMetricImportance(viewModel.User, metrics, new string[0], _context);
            return View(viewModel);
        }


        public async Task<IActionResult> UserMetricSetting(int metricID)
        {
            var user = GetCurrentUser();
            var variants = _context.MetricVariants.Where(mv => mv.MetricID == metricID).ToList();
            var choosed = _context.UserMetricVariants.Include(um => um.MetricVariant).
                Where(um => um.UserID == user.Id && variants.Contains(um.MetricVariant)).FirstOrDefault();
            if(choosed != null)
            {
                variants.ForEach(v => {
                    if (v == choosed.MetricVariant)
                        v.DefaultVariant = true;
                    else v.DefaultVariant = false;
                });                
            }
            return PartialView(variants);
        }

        public IResult SaveUserMetricVariant(string variant)
        {
            User user = GetCurrentUser();
            var metricVariant = _context.MetricVariants.Include(mv=>mv.Metric).Where(mv=> mv.Code== variant)
                .FirstOrDefault();
            if (metricVariant ==  null)
            {
                return Results.BadRequest();
            }
            if (user == null)
            {
                return Results.Unauthorized();
            }
            UserMetricVariants.Save(user.Id, metricVariant, _context);
            return Results.NoContent();
        }

        /// <summary>
        /// Sets user choice of displaying metrics filter.
        /// </summary>
        /// <param name="metricsview">Chosen display of metrics filter</param>
        /// <returns>App settings page</returns>
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

        /// <summary>
        /// Sets user choice on how he want to add new block rules.
        /// </summary>
        /// <param name="addblockruleview">Chosen way to add new block rukes.</param>
        /// <returns>App settings page</returns>
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

        /// <summary>
        /// Sets user choice on what information he wants in explanations.
        /// </summary>
        /// <param name="explanationview">Chosen type of explanation.</param>
        /// <returns>App settings page</returns>
        public IActionResult SetExplanationView(int explanationview)
        {
            if ((explanationview < 0) || (explanationview >= Enum.GetValues(typeof(ExplanationView)).Length))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetExplanationView(explanationview);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("AppSettings");

        }

        /// <summary>
        /// Sets user choice on what type of score for metrics he wants to see.
        /// </summary>
        /// <param name="metricContributionScoreView">Chosen type of score for metrics</param>
        /// <returns>App settings page</returns>
        public IActionResult SetMetricContributionScoreView(int metricContributionScoreView)
        {
            if ((metricContributionScoreView < 0) || (metricContributionScoreView >= Enum.GetValues(typeof(MetricContributionScoreView)).Length))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetMetricContributionScoreView(metricContributionScoreView);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("AppSettings");

        }

        /// <summary>
        /// Sets user choice on colors that corresponds to metrics.
        /// </summary>
        /// <param name="color">Chosen colors</param>
        /// <returns>App settings page</returns>
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

        /// <summary>
        /// Saves new user interaction with an item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="type">Type of interaction</param>
        /// <returns>HTTP response without content</returns>
        public IResult SetInteraction(int id, TypeOfInteraction type)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            Interaction.Save(id, user.Id, type, _context);
            return Results.NoContent();
        }

        /// <summary>
        /// Saves new user block for an item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>HTTP response without content</returns>
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

        /// <summary>
        /// Cancel user block on an item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>HTTP response without content</returns>
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