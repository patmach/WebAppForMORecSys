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
using System.Text;
using System.Drawing;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IO;

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
            /*
            var questions = _context.Questions;
            var answers = _context.Answers;
            using(var sw  = new StreamWriter("Questions.txt"))
            {
                foreach (var question in questions)
                {
                    sw.WriteLine(question.Text);
                    if(question.AnswerType == TypeOfAnswer.AgreeScale)
                    {
                        sw.WriteLine("\tStrongly agree......Strongly disagree");
                    }
                    else if(question.AnswerType == TypeOfAnswer.Options)
                    {
                        var currentAnswers = answers.Where(a => a.QuestionID == question.Id);
                        foreach (var answer in currentAnswers)
                        {
                            sw.WriteLine("\t" + answer.Text);
                        }
                    }
                    else
                    {
                        sw.WriteLine("\tText");
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                }
            }*/
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
        /// Choose question that are should be part of the list and returns partial view with these questions
        /// </summary>
        /// <param name="onlyNotAnswered">If set only questions user can answer and haven't yet answered are displayed.</param>
        /// <param name="specificIDs">If set only questions with these IDs that user can answer are displayed.</param>
        /// <param name="sectionID">If set only questions from this section that user can answer are displayed.</param>
        /// <returns>Partial view with list of questions</returns>
        public async Task<IActionResult> ListOfQuestions(bool onlyNotAnswered, int[]? specificIDs, int? sectionID)
        {
            var user = GetCurrentUser();
            var actIDsDoneByUser = UserActCache.GetActs(user.Id.ToString(), _context);
            var questions = _context.Questions.Include(q => q.QuestionsActs).Include(q => q.Answers).Include(q => q.QuestionSection)
                .Where(q => q.QuestionsActs.Select(qa => qa.ActID).All(actid => actIDsDoneByUser.Contains(actid)));
            if ((specificIDs != null) && (specificIDs.Length > 0))
            {
                questions = questions.Where(q => specificIDs.Contains(q.Id));
            }
            if (onlyNotAnswered)
            {
                var answered = _context.UserAnswers.Where(ua => (ua.UserID == user.Id)).Select(ua => ua.QuestionID);
                questions = questions.Where(q => !answered.Contains(q.Id));
                user.SetLastSectionID(int.MaxValue);
            }
            if (sectionID.HasValue)
            {
                questions = questions.Where(q => q.QuestionSectionID == sectionID.Value);
                user.SetLastSectionID(sectionID.Value);
            }            
            var questionList = questions.ToList();
            questionList.ForEach(q => {
                q.UserAnswers = _context.UserAnswers.Where(ua => (ua.UserID == user.Id)
                                        && (ua.QuestionID == q.Id)).ToList();
            });
            
            _context.Update(user);
            _context.SaveChanges();
            return PartialView(questionList);
        }

        /// <summary>
        /// </summary>
        /// <param name="id">Id of the question</param>
        /// <returns>Partial view with one question</returns>
        public async Task<IActionResult> Question(int id)
        {
            var user = GetCurrentUser();
            Question question = _context.Questions.Include(q => q.Answers).Where(q => q.Id == id).FirstOrDefault();
            question.UserAnswers = _context.UserAnswers.Where(ua => (ua.UserID == user.Id)
                                        && (ua.QuestionID == question.Id)).ToList();
            _context.Update(user);
            _context.SaveChanges();
            return PartialView(question);
        }


        /// <summary>
        /// Saves user's answer to a question
        /// </summary>
        /// <param name="questionID">Question that was answered</param>
        /// <param name="answerID">If the answer is TypeOfAnswer.Option answer ID is saved as answer</param>
        /// <param name="value">If the answer is TypeOfAnswer.AgreeScale value is saved as answer</param>
        /// <param name="text">If the answer is TypeOfAnswer.Text text is saved as answer</param>
        /// <returns>No content</returns>
        public async Task<IResult> SaveAnswer(int questionID, int? answerID, int? value, string? text)
        {
            var user = GetCurrentUser();
            SaveMethods.SaveUserAnswer(user, questionID, answerID, value, text, _context);
            return Results.NoContent();
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
        /// <param name="metricID">ID of metric whose variant will be selected</param>
        /// <returns>Partial view where user can select which metric variant he wants to use.</returns>
        public async Task<IActionResult> UserMetricSetting(int metricID)
        {
            var user = GetCurrentUser();
            var variants = _context.MetricVariants.Include(mv => mv.Metric).Where(mv => mv.MetricID == metricID).ToList();
            var choosed = _context.UserMetricVariants.Include(um => um.MetricVariant).
                Where(um => um.UserID == user.Id && variants.Contains(um.MetricVariant)).FirstOrDefault();
            if(choosed != null)
            {
                variants.ForEach(v => {
                    if (v.Id == choosed.MetricVariant.Id)
                        v.DefaultVariant = true;
                    else v.DefaultVariant = false;
                });
            }
            return PartialView(variants);
        }

        /// <summary>
        /// Saves variant of the metric that user has selected to use
        /// </summary>
        /// <param name="variant">Code of selected metric variant</param>
        /// <returns>No Content - if everything goes well</returns>
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
            //AddAct(metricVariant.Code);
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
            AddAct(((MetricsView)metricsview).ToString());
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
            user.SetExplanationView(explanationview);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((ExplanationView)explanationview).ToString());
            return RedirectToAction("AppSettings");

        }

        /// <summary>
        /// Sets user choice on what type of explanation of metrics share (s)he wants to see in item preview
        /// </summary>
        /// <param name="previewExplanationView">Chosen type of score for metrics</param>
        /// <returns>App settings page</returns>
        public IActionResult SetPreviewExplanationView(int previewExplanationView)
        {
            if ((previewExplanationView < 0) || (previewExplanationView >= Enum.GetValues(typeof(PreviewExplanationView)).Length))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            user.SetPreviewExplanationView(previewExplanationView);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((PreviewExplanationView)previewExplanationView).ToString());
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
            user.SetMetricContributionScoreView(metricContributionScoreView);
            _context.Update(user);
            _context.SaveChanges();
            AddAct(((MetricContributionScoreView)metricContributionScoreView).ToString());
            return RedirectToAction("AppSettings");

        }

        /// <summary>
        /// Sets user choice on colors that corresponds to metrics.
        /// </summary>
        /// <param name="color">Chosen colors</param>
        /// <returns>App settings page</returns>
        public IActionResult SetMetricsColors(string[] metriccolor)
        {
            var rs = SystemParameters.GetRecommenderSystem(_context);
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
            AddAct("ColoursChanged");
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
        /// Saves new user rating for a movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <param name="score">Rating score</param>
        /// <returns>HTTP response without content</returns>
        public IResult Rate(int id, byte score)
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
        public IResult Unrate(int id)
        {
            User user = GetCurrentUser();
            SaveMethods.RemoveRating(id, user.Id, _context);
            return Results.NoContent();
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
            SaveMethods.SaveInteraction(id, user.Id, type, _context);
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
            if(user.UserName != "log_master")
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