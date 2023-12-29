
using System.Text;
using WebAppForMORecSys.Loggers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Helpers
{

    /// <summary>
    /// Contains properties and methods to log interaction
    /// </summary>
    public static class InteractionLogExtensions
    {
        /// <summary>
        /// For logging of every interaction to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/Interactions.txt");

        /// <summary>
        /// Log interaction
        /// </summary>
        public static void Log(this Interaction interaction)
        {
            logger.Log($"{interaction.UserID};{interaction.ItemID};{interaction.type};" +
            $"{interaction.Last.ToString(logger.DateFormat)}");
        }
    }

    /// <summary>
    /// Contains properties and methods to log rating
    /// </summary>
    public static class RatingLogExtension
    {

        /// <summary>
        /// For logging of every rating to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/Ratings.txt");

        /// <summary>
        /// Log rating
        /// </summary>
        public static void Log(this Rating rating) {
            logger.Log($"{rating.UserID};{rating.ItemID};{rating.RatingScore};" +
                $"{rating.Date.ToString(logger.DateFormat)}");
        }
    }

    /// <summary>
    /// Contains properties and methods to log user answer
    /// </summary>
    public static class UserAnswerLogExtension
    {

        /// <summary>
        /// For logging of every userAnswer to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/UserAnswers.txt");

        /// <summary>
        /// Log UserAnswer
        /// </summary>
        public static void Log(this UserAnswer userAnswer)
        {
            logger.Log($"{userAnswer.UserID};{userAnswer.QuestionID};" +
                $"{userAnswer.Date.ToString(logger.DateFormat)};{userAnswer.AnswerID.ToString() ?? ""};" +
                $"{userAnswer.Value.ToString() ?? ""};{userAnswer.Text ?? ""}");
        }
    }

    /// <summary>
    /// Contains properties and methods to log user act
    /// </summary>
    public static class UserActSuggestionLogExtension
    {

        /// <summary>
        /// For logging of every rating to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/UserActSuggestions.txt");

        /// <summary>
        /// Log rating
        /// </summary>
        public static void Log(this UserActSuggestion userActSuggestion)
        {
            logger.Log($"{userActSuggestion.UserID};{userActSuggestion.ActID};{userActSuggestion.NumberOfSuggestions};" +
                $"{DateTime.Now.ToString(logger.DateFormat)}");
        }
    }

    /// <summary>
    /// Contains properties and methods to log setting of user jsonparams
    /// </summary>
    public static class UserJSONParamsLogExtension
    {

        /// <summary>
        /// For logging of every block to file
        /// </summary>
        private static MyFileLogger loggerBlock = new MyFileLogger("Logs/Blocks.txt");

        /// <summary>
        /// For logging of every unblock to file
        /// </summary>
        private static MyFileLogger loggerUnblock = new MyFileLogger("Logs/Unblocks.txt");

        /// <summary>
        /// Log UserAnswer
        /// </summary>
        public static void LogBlock(this User user, string property, string value)
        {
            loggerBlock.Log($"{user.Id},{property},{value},{DateTime.Now.ToString(loggerBlock.DateFormat)}");
        }

        public static void LogUnblock(this User user, string property, string value)
        {
            loggerUnblock.Log($"{user.Id},{property},{value},{DateTime.Now.ToString(loggerUnblock.DateFormat)}");
        }
    }

    /// <summary>
    /// Contains properties and methods to log every query to recommender made by user
    /// </summary>
    public static class RecommenderQueryLogExtensions
    {
        /// <summary>
        /// For logging of every interaction to file
        /// </summary>
        private static MyFileLogger loggerQuery = new MyFileLogger("Logs/RecommenderQueries.txt");

        /// <summary>
        /// Log interaction
        /// </summary>
        public static void Log(this MainViewModel mainViewModel, List<string> mvCodes)
        {
            var messageSB = new StringBuilder();
            foreach (var code in mvCodes)
            {
                messageSB.Append(code).Append(";");
            }
            foreach (var value in mainViewModel.Metrics.Values)
            {
                messageSB.Append(value).Append(";");
            }
            messageSB.Append(mainViewModel.User.GetMetricsView().ToFriendlyString()).Append(";");
            messageSB.Append(mainViewModel.User.Id).Append(";");
            messageSB.Append(DateTime.Now.ToString(loggerQuery.DateFormat));
            loggerQuery.Log(messageSB.ToString());
        }
    }
}
