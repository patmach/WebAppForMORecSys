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
    /// Controller for the requests on actions related to ratings
    /// </summary>
    public class QuestionsController : Controller
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
        public QuestionsController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<IActionResult> Index(int id)
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
