using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.MinimalApi;
using NuGet.Packaging;
using System.Text;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Settings;

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
        public bool UserStudyAllowed { get; set; }

        /// <summary>
        /// Suggestions of all acts required for formular allowance
        /// </summary>
        public List<string> NotDoneRequiredActsSuggestions { get; set; } = new List<string>();

        /// <summary>
        /// Information about the number of required acts completed
        /// </summary>
        public string RequiredInformation { get; set; } = "";

        /// <summary>
        /// Suggestions of all acts required for formular allowance
        /// </summary>
        public List<string> NotDoneNeededActsSuggestions { get; set; } = new List<string>();

        /// <summary>
        /// Information about the number of needed acts completed
        /// </summary>
        public string NeededInformation { get; set; } = "";

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
            bool required = DoneAllRequiredActs(user, actIDsDoneByUser, context);
            bool minimal = DoneMinimumNumberOfActs(user, actIDsDoneByUser, context);
            UserStudyAllowed = required && minimal;
            return UserStudyAllowed;
        }

        /// <summary>
        /// Checks if user done all required acts, sets informational messages
        /// </summary>
        /// <param name="user">User that went to formular page</param>
        /// <param name="actIDsDoneByUser">IDs of all acts already performed by the user</param>
        /// <param name="context">Database context</param>
        /// <returns>user done all required acts</returns>
        private bool DoneAllRequiredActs(User user, List<int> actIDsDoneByUser, ApplicationDbContext context)
        {
            var requiredActs = UserActCache.AllActs.Where(a => a.Priority == 1);
            bool required = requiredActs.All(na => actIDsDoneByUser.Contains(na.Id));
            if (!required)
            {
                NotDoneRequiredActsSuggestions.AddRange(requiredActs.Where(na => !actIDsDoneByUser.Contains(na.Id))
                    .Select(na => na.SuggestionText));
                RequiredInformation = $"You have completed {requiredActs.Where(na => actIDsDoneByUser.Contains(na.Id)).Count()}" +
                    $" / {requiredActs.Count()} required actions."; 


            }
            return required;
        }

        /// <summary>
        /// Checks if user done minimum numbers of not required acts, sets informational messages
        /// </summary>
        /// <param name="user">User that went to formular page</param>
        /// <param name="actIDsDoneByUser">IDs of all acts already performed by the user</param>
        /// <param name="context">Database context</param>
        /// <returns>user done minimum numbers of not required acts</returns>
        private bool DoneMinimumNumberOfActs(User user, List<int> actIDsDoneByUser, ApplicationDbContext context)
        {
            if (user.FirstRecommendationTime == null)
            {
                NeededInformation = $"You'll be able to perform the actions after you'll rate " +
                    $"positively atleast {SystemParameters.MinimalPositiveRatings} movies and get first recommendations.";
                return false;
            }
            var userTime = DateTime.Now - user.FirstRecommendationTime;      
            var timeNormalized = userTime.HasValue ? Math.Min(userTime.Value.TotalMinutes, 30) / 30d : 0;
            var secondPriorityActs = UserActCache.AllActs.Where(a => a.Priority == 2).GroupBy(a=> a.TypeOfAct);
            var thirdPriorityActs = UserActCache.AllActs.Where(a => a.Priority == 3).GroupBy(a => a.TypeOfAct);
            double secondPriorityDone = 0.0;
            double thirdPriorityDone = 0.0;
            StringBuilder neededInfromationSB = new StringBuilder();
            string s;
            foreach (var group in secondPriorityActs)
            {
                secondPriorityDone += checkGroup(group, actIDsDoneByUser, out s) ? 1 : 0;
                neededInfromationSB.AppendLine(s);
            }
            foreach (var group in thirdPriorityActs)
            {
                thirdPriorityDone += checkGroup(group, actIDsDoneByUser, out s) ? 1 : 0;
                neededInfromationSB.AppendLine(s);
            }
            secondPriorityDone /= secondPriorityActs.Count();
            thirdPriorityDone /= thirdPriorityActs.Count();
            bool allowed = Math.Max(Math.Max(timeNormalized + 1.2 * secondPriorityDone - 1 , 0) 
                + 0.8 * thirdPriorityDone - 1, 0) > 0; //Modified Łukasiewicz norm
            if (!allowed)
            {
                var allNeededActs = secondPriorityActs.SelectMany(group => group)
                    .Where(a=> !actIDsDoneByUser.Contains(a.Id)).ToList();
                allNeededActs.AddRange(thirdPriorityActs.SelectMany(group => group)
                    .Where(a => !actIDsDoneByUser.Contains(a.Id)));
                NotDoneNeededActsSuggestions.AddRange(allNeededActs.OrderBy(a => a.Priority)
                    .Select(a => a.SuggestionText + "\n(Group of actions: " + a.TypeOfAct + ")"));
            }
            NeededInformation = neededInfromationSB.ToString();
            return allowed;
        }

        private bool checkGroup(IGrouping<string, Act> group, List<int> actIDsDoneByUser, out string neededInfromation)
        {
            bool allDone = group.All(a => actIDsDoneByUser.Contains(a.Id));
            var neededInfromationSB = new StringBuilder();
            if (!allDone)
            {
                neededInfromationSB.Append("You have completed ");
                neededInfromationSB.Append(group.Where(a => actIDsDoneByUser.Contains(a.Id)).Count());
                neededInfromationSB.Append($" / {group.Count()} actions from group of actions {group.Key}.");
            }
            neededInfromation = neededInfromationSB.ToString();
            return allDone;
        }
    }
}
