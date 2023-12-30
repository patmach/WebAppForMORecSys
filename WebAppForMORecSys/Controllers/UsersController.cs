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
    /// Controller for the requests on actions related to the user
    /// </summary>
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
            var combinations = NewUserSetting.GetCombinationsForFirstSetting(_context);
            user.SetRandomSettingsForNewUser(_context);
            UserMetricVariants.SetRandomMetricVariants(user, combinations, _context);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("Manual", "Home");
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
        /// </summary>
        /// <returns>ID of all questions user hasn't answered</returns>
        public IActionResult GetNotAnsweredQuestions()
        {
            var user = GetCurrentUser();            
            var answered = _context.UserAnswers.Where(ua => ua.User.Id == user.Id).Select(ua => ua.QuestionID).ToList();
            var notAnswered = _context.Questions.Where(q => !answered.Contains(q.Id)).Select(q=> q.Id).ToList();
            return Json(notAnswered);
        }

        /*
        public IActionResult Debug()
        {
            //TODO DELETE
            User user = GetCurrentUser();
            var latinSquares = LatinSquaresForNewUser.GetLatinSquaresForFirstSetting(_context);
            user.SetRandomSettingsForNewUser(latinSquares, _context);
            UserMetricVariants.SetRandomMetricVariants(user, latinSquares, _context);
            return Ok();
        }
        */

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






        /// <summary>
        /// Sets user choice of displaying metrics filter.
        /// </summary>
        /// <param name="metricsview">Chosen display of metrics filter</param>
        /// <returns>App settings page</returns>
        public IActionResult SetMetricsView(int metricsview)
        {
            if ((metricsview < 0) || (metricsview >= Enum.GetValues(typeof(MetricsView)).Length))
                return RedirectToAction("AppSettings","Home");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetMetricsView(metricsview);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((MetricsView)metricsview).ToString());
            return RedirectToAction("AppSettings","Home", "Home");

        }

        /// <summary>
        /// Sets user choice on how he want to add new block rules.
        /// </summary>
        /// <param name="addblockruleview">Chosen way to add new block rukes.</param>
        /// <returns>App settings page</returns>
        public IActionResult SetAddBlockRuleView(int addblockruleview)
        {
            if ((addblockruleview < 0) || (addblockruleview >= Enum.GetValues(typeof(AddBlockRuleView)).Length))
                return RedirectToAction("AppSettings","Home");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetAddBlockRuleView(addblockruleview);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("AppSettings","Home");

        }

        /// <summary>
        /// Sets user choice on what information he wants in explanations.
        /// </summary>
        /// <param name="explanationview">Chosen type of explanation.</param>
        /// <returns>App settings page</returns>
        public IActionResult SetExplanationView(int explanationview)
        {
            if ((explanationview < 0) || (explanationview >= Enum.GetValues(typeof(ExplanationView)).Length))
                return RedirectToAction("AppSettings","Home");
            User user = GetCurrentUser();
            user.SetExplanationView(explanationview);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((ExplanationView)explanationview).ToString());
            return RedirectToAction("AppSettings","Home");

        }

        /// <summary>
        /// Sets user choice on what type of explanation of metrics share (s)he wants to see in item preview
        /// </summary>
        /// <param name="previewExplanationView">Chosen type of score for metrics</param>
        /// <returns>App settings page</returns>
        public IActionResult SetPreviewExplanationView(int previewExplanationView)
        {
            if ((previewExplanationView < 0) || (previewExplanationView >= Enum.GetValues(typeof(PreviewExplanationView)).Length))
                return RedirectToAction("AppSettings","Home");
            User user = GetCurrentUser();
            user.SetPreviewExplanationView(previewExplanationView);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((PreviewExplanationView)previewExplanationView).ToString());
            return RedirectToAction("AppSettings","Home");

        }

        /// <summary>
        /// Sets user choice on what type of score for metrics he wants to see.
        /// </summary>
        /// <param name="metricContributionScoreView">Chosen type of score for metrics</param>
        /// <returns>App settings page</returns>
        public IActionResult SetMetricContributionScoreView(int metricContributionScoreView)
        {
            if ((metricContributionScoreView < 0) || (metricContributionScoreView >= Enum.GetValues(typeof(MetricContributionScoreView)).Length))
                return RedirectToAction("AppSettings","Home");
            User user = GetCurrentUser();
            user.SetMetricContributionScoreView(metricContributionScoreView);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((MetricContributionScoreView)metricContributionScoreView).ToString());
            return RedirectToAction("AppSettings","Home");
        }

        /// <summary>
        /// Sets user choice on colors that corresponds to metrics.
        /// </summary>
        /// <param name="color">Chosen colors</param>
        /// <returns>App settings page</returns>
        public IActionResult SetMetricsColors(string[] metriccolor)
        {
            var rs = SystemParameters.GetRecommenderSystem(_context);
            if ((metriccolor == null) || (metriccolor.Length < 0)
                || (metriccolor.Length != _context.Metrics.Where(m => m.RecommenderSystemID == rs.Id).Count()))
                return RedirectToAction("AppSettings","Home");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetColors(metriccolor);
            _context.Update(user);
            _context.SaveChanges();
            AddAct("ColoursChanged");
            return RedirectToAction("AppSettings","Home");
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
        /// Saves new user block for an item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>HTTP response without content</returns>
        public IResult Hide(int id)
        {
            if (_context.Items.Where(m => m.Id == id).Count() == 0)
                return Results.NoContent();
            User user = GetCurrentUser();
            user.AddItemToBlackList(id);
            _context.Update(user);
            _context.SaveChanges();
            AddAct("MovieBlock");
            user.LogBlock("id", id.ToString());
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
            user.RemoveItemFromBlackList(id);
            _context.Update(user);
            _context.SaveChanges();
            user.LogUnblock("id", id.ToString());
            return Results.NoContent();
        }
    }
}
