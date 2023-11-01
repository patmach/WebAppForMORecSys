using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Data;

namespace WebAppForMORecSys.Models.ViewModels
{
    /// <summary>
    /// View model for the formular page
    /// </summary>
    public class FormularViewModel
    {
        /// <summary>
        /// IDs of questions user can answer
        /// </summary>
        public List<Question> Questions { get; set; }

        /// <summary>
        /// User for whom the page is loaded
        /// </summary>
        public User CurrentUser { get; set; }

        /// <summary>
        /// True if user has done all acts that are needed for participating in user study
        /// </summary>
        public bool UserDoneAllNeededActs { get; set; }

        /// <summary>
        /// Suggestions of all acts required for formular allowance
        /// </summary>
        public List<string> NotDoneRequiredActsSuggestions { get; set; }

        /// <summary>
        /// True if user has done all acts that are needed for participating in user study
        /// </summary>
        public int LastSectionID { get; set; } = -1;

        /// <summary>
        /// Message to be shown in alert window
        /// </summary>
        public string Message;

        /// <summary>
        /// Check if user can start answering questions in user study.
        /// Saves suggestion of acts that user needs to do before the form is allowed
        /// </summary>
        /// <param name="user">User that went to formular page</param>
        /// <param name="context">Database context</param>
        /// <returns>Information if user has permission to start user study</returns>
        public bool IsUserStudyAllowed(User user, ApplicationDbContext context)
        {
            var actIDsDoneByUser = UserActCache.GetActs(user.Id.ToString(), context);
            var neededActs = UserActCache.AllActs.Where(a => a.Priority == 1);
            UserDoneAllNeededActs = neededActs.All(na=> actIDsDoneByUser.Contains(na.Id));
            if (!UserDoneAllNeededActs)
            {
                NotDoneRequiredActsSuggestions = neededActs.Where(na => !actIDsDoneByUser.Contains(na.Id))
                    .Select(na => na.SuggestionText).ToList();
            }
            return UserDoneAllNeededActs;
        }
    }
}
