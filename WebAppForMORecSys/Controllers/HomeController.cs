﻿using Microsoft.AspNetCore.Mvc;
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
            var rs = SystemParameters.GetRecommenderSystem(_context);
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

        /// <summary>
        /// Choose questions user can answer and loads page with user study form
        /// </summary>
        /// <returns>Page with user study form</returns>
        public async Task<IActionResult> Formular()
        {
            var user = GetCurrentUser();
            var actIDsDoneByUser = UserActCache.GetActs(user.Id.ToString(), _context);
            var questions = _context.Questions.Include(q=> q.QuestionsActs)
                .Where(q=> q.QuestionsActs.Select(qa=> qa.ActID).All(actid => actIDsDoneByUser.Contains(actid)));
            FormularViewModel viewModel = new FormularViewModel
            {
                CurrentUser = user,
                QuestionIDs = await questions.Select(q => q.Id).ToListAsync(),
                LastQuestionID = user.GetLastQuestionID()
            };
            viewModel.IsUserStudyAllowed(user, _context);
            return View(viewModel);
        }

        /// <summary>
        /// Choose question that are should be part of the list and returns partial view with these questions
        /// </summary>
        /// <param name="onlyNotAnswered">If set only questions user can answer and haven't yet answered are displayed.</param>
        /// <param name="specificIDs">If set only questions with these IDs user can answer are displayed.</param>
        /// <returns>Partial view with list of questions</returns>
        public async Task<IActionResult> ListOfQuestions(bool onlyNotAnswered, int[]? specificIDs)
        {
            var user = GetCurrentUser();
            var actIDsDoneByUser = UserActCache.GetActs(user.Id.ToString(), _context);
            var questions = _context.Questions.Include(q => q.QuestionsActs).Include(q => q.Answers)
                .Where(q => q.QuestionsActs.Select(qa => qa.ActID).All(actid => actIDsDoneByUser.Contains(actid)));
            if ((specificIDs != null) && (specificIDs.Length > 0))
            {
                questions = questions.Where(q => specificIDs.Contains(q.Id));
            }
            if (onlyNotAnswered)
            {
                var answered = _context.UserAnswers.Where(ua => (ua.UserID == user.Id)).Select(ua => ua.QuestionID);
                questions = questions.Where(q => !answered.Contains(q.Id));
            }
            
            var questionList = questions.ToList();
            questionList.ForEach(q => {
                q.UserAnswers = _context.UserAnswers.Where(ua => (ua.UserID == user.Id)
                                        && (ua.QuestionID == q.Id)).ToList();
            });
            user.SetLastQuestionID(questionList.LastOrDefault()?.Id ?? int.MaxValue);
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
            user.SetLastQuestionID(question.Id);
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
            UserAnswer.Save(user, questionID, answerID, value, text, _context);
            return Results.NoContent();
        }

        /// <summary>
        /// </summary>
        /// <returns>Partial view with manual how to use metrics filter.</returns>
        public async Task<IActionResult> MetricsFilterHelp()
        {
            User user = GetCurrentUser();
            RecommenderSystem rs = SystemParameters.GetRecommenderSystem(_context);
            List<Metric> metrics = await _context.Metrics.Include(m => m.metricVariants)
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
        /// Sets user choice on what type of explanation of metrics share (s)he wants to see in item preview
        /// </summary>
        /// <param name="previewExplanationView">Chosen type of score for metrics</param>
        /// <returns>App settings page</returns>
        public IActionResult SetPreviewExplanationView(int previewExplanationView)
        {
            if ((previewExplanationView < 0) || (previewExplanationView >= Enum.GetValues(typeof(PreviewExplanationView)).Length))
                return RedirectToAction("AppSettings");
            User user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }
            user.SetPreviewExplanationView(previewExplanationView);
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

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        
    }
} 