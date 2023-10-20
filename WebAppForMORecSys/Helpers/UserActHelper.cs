using WebAppForMORecSys.Data;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// Class containing methods for working with UserAct class
    /// </summary>
    public static class UserActHelper
    {
        /// <summary>
        /// CheckUserActs
        /// </summary>
        public static Random rnd = new Random();

        /// <summary>
        /// Checks acts user hasn't done yet
        /// </summary>
        /// <param name="userID">user whose actions are checked</param>
        /// <param name="context">Database context</param>
        /// <param name="Request">HTTP Request (to get base address if it's the first call)</param>
        /// <returns>Suggestion of one of the not done acts</returns>
        public static string CheckUserActs(int userID, ApplicationDbContext context, HttpRequest Request)
        {
            UserActCache.SetSaveToDbTimer(context, Request);
            List<Act> actsNotDoneByUser = new List<Act>();
            List<int> actsDoneByUser = UserActCache.GetActs(userID.ToString(), context);
            int maxPriority = UserActCache.AllActs.Select(a => a.Priority).Max();
            foreach (var act in UserActCache.AllActs)
            {
                if (!actsDoneByUser.Contains(act.Id))
                {
                    for (int i = 0; i <= maxPriority - act.Priority; i++)
                    {
                        actsNotDoneByUser.Add(act);
                    }
                }
            }
            if (actsNotDoneByUser.Count == 0)
                return "";
            return actsNotDoneByUser[rnd.Next(actsNotDoneByUser.Count)].SuggestionText;
        }
    }
}
