﻿
using WebAppForMORecSys.Loggers;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers
{
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
            $"{interaction.Last.ToString(logger.format)}");
        }
    }

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
                $"{rating.Date.ToString(logger.format)}");
        }
    }

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
                $"{userAnswer.Date.ToString(logger.format)};{userAnswer.AnswerID.ToString() ?? ""};" +
                $"{userAnswer.Value.ToString() ?? ""};{userAnswer.Text ?? ""}");
        }
    }


    public static class UserLogExtension
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
            loggerBlock.Log($"{user.Id},{property},{value},{DateTime.Now.ToString(loggerBlock.format)}");
        }

        public static void LogUnblock(this User user, string property, string value)
        {
            loggerUnblock.Log($"{user.Id},{property},{value},{DateTime.Now.ToString(loggerUnblock.format)}");
        }
    }

}
