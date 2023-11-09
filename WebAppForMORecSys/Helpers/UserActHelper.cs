using WebAppForMORecSys.Data;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.ViewModels;
using Microsoft.IdentityModel.Tokens;
using System.IO;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// Class containing methods for working with UserAct class
    /// </summary>
    public static class UserActHelper
    {
        /// <summary>
        /// Instance of random, used by FindUserActsTips
        /// </summary>
        public static Random rnd = new Random();

        /// <summary>
        /// Add user acts retrieved from main view model
        /// </summary>
        /// <param name="mainViewModel">View model of the main page</param>
        /// <param name="typeOfSearch">Specifies if user clicked the search for main search or in movie filter</param>
        /// <param name="context">Database context</param>
        public static void AddUserActsFromMainViewModel(MainViewModel mainViewModel, string typeOfSearch, ApplicationDbContext context)
        {
            if ((typeOfSearch == "Search") && (!mainViewModel.SearchValue.IsNullOrEmpty()))
            {
                UserActCache.AddAct(mainViewModel.User.Id.ToString(), "Search", context);
            }
            else if (typeOfSearch == "MovieFilter")
            {
                List<string> keys = new List<string> { "Director", "Actor", "ReleaseDateFrom", "ReleaseDateTo", "Genres" };
                if (keys.Any(k => mainViewModel.FilterValues.ContainsKey(k) 
                                    &&!mainViewModel.FilterValues[k].IsNullOrEmpty()))
                    UserActCache.AddAct(mainViewModel.User.Id.ToString(), "MovieFilter", context);
            }
        }


        /// <summary>
        /// Finds acts user hasn't done yet
        /// </summary>
        /// <param name="userID">user whose actions are checked</param>
        /// <param name="context">Database context</param>
        /// <param name="Request">HTTP Request (to get base address if it's the first call)</param>
        /// <returns>Suggestion of one of the not done acts</returns>
        public static string FindUserActTip(int userID, ApplicationDbContext context, HttpRequest Request,
            int ratingsCount = 0)
        {
            UserActCache.SetSaveToDbTimer(context, Request);
            List<double> weights = new List<double>();
            List<int> actsDoneByUser = UserActCache.GetActs(userID.ToString(), context);
            List<UserActSuggestion> userActSuggestions = context.UserActSuggestions.Where(uas => uas.UserID == userID)
                .ToList();
            int maxPriority = UserActCache.AllActs.Select(a => a.Priority).Max() + 1;
            foreach (var act in UserActCache.AllActs)
            {
                if (actsDoneByUser.Contains(act.Id))
                    weights.Add(0);
                else if ((act.Code == "RatedEnough") && (ratingsCount > 0))
                {
                    int numberOfSuggestions = userActSuggestions.Where(uas => uas.ActID == act.Id).FirstOrDefault()?.
                            NumberOfSuggestions ?? 0;
                    weights.Add(30d / ratingsCount / (double)Math.Pow(2, numberOfSuggestions)); 
                }
                else
                {
                    int numberOfSuggestions = userActSuggestions.Where(uas => uas.ActID == act.Id).FirstOrDefault()?.
                            NumberOfSuggestions ?? 0;
                    weights.Add((maxPriority - act.Priority) / (double)Math.Pow(2, numberOfSuggestions));
                }
            }
            double sum = weights.Sum();
            if (sum == 0)
                return "You have performed all actions.\nPlease start answer questions in the " +
                    "<a href=\"/Home/Formular\">User study form page</a>.";
            var randomValue = rnd.NextDouble() * sum;
            double partSum = 0;
            int count = 0;
            while (randomValue >= partSum)
            {
                partSum += weights[count];
                count++;
            }
            Act selectedAct = UserActCache.AllActs[count - 1];
            SaveMethods.SaveUserActSuggestion(selectedAct.Id, userID, context);
            return selectedAct.SuggestionText;
        }
    }
}
