using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;
using Microsoft.AspNetCore.Identity;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Settings;
using WebAppForMORecSys.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Drawing;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using WebAppForMORecSys.Helpers.JSONPropertiesHandlers;
using WebAppForMORecSys.Data.Cache;

namespace WebAppForMORecSys.Controllers
{
    /// <summary>
    /// Controller for the requests on pages and actions not based on domain
    /// </summary>
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
        }

       
        /// <summary>
        /// Redirects to used controller
        /// </summary>
        /// <returns>The main page of used controller</returns>
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", SystemParameters.DomainController);
        }

        /// <summary>
        /// Redirects to used controller
        /// </summary>
        /// <returns>The main page of management of user blocks</returns>
        public async Task<IActionResult> UserBlockSettings()
        {
            return RedirectToAction("UserBlockSettings", SystemParameters.DomainController);
        }

        /// <summary>
        /// Prepares data for the custom setting page and then return the page.
        /// </summary>
        /// <returns>Page where user can make custom changes to the system.</returns>
        public async Task<IActionResult> AppSettings()
        {
            MainViewModel viewModel = new MainViewModel();
            var rs = SystemParameters.GetRecommenderSystem(_context);
            var metrics = await (_context.Metrics.Include(m=> m.MetricVariants).Where(m => m.RecommenderSystemID == rs.Id)
                .ToListAsync());
            viewModel.User = GetCurrentUser();
            var blockedItems = BlockedItemsCache.GetBlockedItemIdsForUser(viewModel.User.Id.ToString(), _context);
            viewModel.UserRatings = await (from rating in _context.Ratings
                                                  where rating.UserID == viewModel.User.Id
                                                  select rating).ToListAsync();
            viewModel.Items = await _context.Items.Where(item=> !blockedItems.Contains(item.Id)).Take(5).ToListAsync();
            viewModel.SetMetricImportance(viewModel.User, metrics, new string[0], _context);
            viewModel.UsedVariants = viewModel.User.GetMetricVariants(_context, metrics.Select(m => m.Id).ToList());
            return View(viewModel);
        }

        /// <summary>
        /// Choose questions user can answer and loads page with user study form
        /// </summary>
        /// <returns>Page with user study form</returns>
        public async Task<IActionResult> Formular()
        {
            var user = GetCurrentUser();
            var actIDsDoneByUser = UserActCache.GetActs(user.Id.ToString(), _context);
            var questions = _context.Questions.Include(q=> q.QuestionsActs).Include(q => q.QuestionSection)
                .Where(q=> q.QuestionsActs.Select(qa=> qa.ActID).All(actid => actIDsDoneByUser.Contains(actid)));
            FormularViewModel viewModel = new FormularViewModel
            {
                CurrentUser = user,
                Questions = await questions.ToListAsync(),
                LastSectionID = user.GetLastSectionID()
            };
            viewModel.IsUserStudyAllowed(user, _context);
            return View(viewModel);
        }

        /// <summary>
        /// </summary>
        /// <returns>Page with manual how to participate in the user study</returns>
        public async Task<IActionResult> Manual()
        {
            return View();
        }

        /// <summary>
        /// </summary>
        /// <returns>Partial view with informed consent and aims and targets of the study</returns>
        public async Task<IActionResult> Consent()
        {
            return PartialView();
        }    

        

        /// <summary>
        /// </summary>
        /// <returns>Partial view with manual how to use metrics filter.</returns>
        public async Task<IActionResult> MetricsFilterHelp()
        {
            User user = GetCurrentUser();
            RecommenderSystem rs = SystemParameters.GetRecommenderSystem(_context);
            List<Metric> metrics = await _context.Metrics.Include(m => m.MetricVariants)
                .Where(m => m.RecommenderSystemID == rs.Id).ToListAsync();
            var dict = metrics.Zip(new List<int>(new int[metrics.Count]), (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v); //Dictionary only created for usage of existing viewmodel MetricsFilterViewModel
            var viewModel = new MetricsFilterViewModel
            {
                User = user,
                Metrics = dict
            };
            return PartialView(viewModel);
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

        

        

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Saves act done by user
        /// </summary>
        /// <param name="code">Code of the act</param>
        /// <returns>No Content</returns>
        public IResult AddAct(string code)
        {
            User user = GetCurrentUser();
            UserActCache.AddAct(user.Id.ToString(), code, _context);
            return Results.NoContent();
        }

        /// <summary>
        /// </summary>
        /// <returns>Information if user can start answering questions in user study.</returns>
        public bool CheckIfUserStudyAllowed()
        {
            var user = GetCurrentUser();
            return new FormularViewModel().IsUserStudyAllowed(user, _context);
        }

        /// <summary>
        /// Easy access to log files
        /// </summary>
        /// <param name="filename">name of the log file</param>
        /// <returns>Text file with logs</returns>
        public IActionResult GetLogsOfWebAppForMORecSys(string filename)
        {
            User user = GetCurrentUser();
            if(user.UserName != "log_master2")
            {
                return Content("Wrong username");
            }
            if (!System.IO.File.Exists("Logs/" + filename)) 
                return Content("No content exists");
            return File(System.IO.File.ReadAllBytes("Logs/" + filename), "application/CSV", System.IO.Path.GetFileName("Logs/" 
                + DateTime.Now.ToString("yyyy_MM_dd__HH__mm__ss") + filename));
        }


    }
} 