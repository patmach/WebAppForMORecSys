using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Data
{
    /// <summary>
    /// Contains methods that saves entities to the database
    /// </summary>
    public static class SaveMethods
    {
        /// <summary>
        /// Saves user's answer to a question
        /// </summary>
        /// <param name="user">User that answered</param>
        /// <param name="questionID">Question that was answered</param>
        /// <param name="answerID">If the answer is TypeOfAnswer.Option answer ID is saved as answer</param>
        /// <param name="value">If the answer is TypeOfAnswer.AgreeScale value is saved as answer</param>
        /// <param name="text">If the answer is TypeOfAnswer.Text text is saved as answer</param>
        /// <param name="context">Database context</param>
        public static void SaveUserAnswer(User user, int questionID, int? answerID, int? value, string? text, ApplicationDbContext context)
        {
            Question question = context.Questions.Include(q => q.UserAnswers).Where(q => q.Id == questionID).FirstOrDefault();
            var useranswer = question.UserAnswers.Where(ua => ua.UserID == user.Id).FirstOrDefault();
            bool isNew = useranswer == null;
            if (isNew)
                useranswer = new UserAnswer
                {
                    QuestionID = questionID,
                    UserID = user.Id
                };
            useranswer.Date = DateTime.Now;
            if (answerID.HasValue)
                useranswer.AnswerID = answerID.Value;
            if (value.HasValue)
                useranswer.Value = value.Value;
            if (!string.IsNullOrEmpty(text))
                useranswer.Text = text;
            if (isNew)
                context.Add(useranswer);
            else
                context.Update(useranswer);
            context.SaveChanges();
            useranswer.Log();
        }

        /// <summary>
        /// Saves new interaction or updates the existing one
        /// </summary>
        /// <param name="itemID">ID of item with which the interaction occur</param>
        /// <param name="userID">ID of user that interacted with item</param>
        /// <param name="typeOfInteraction">Type of interaction</param>
        /// <param name="context">Database context</param>
        public static void SaveInteraction(int itemID, int userID,
            TypeOfInteraction typeOfInteraction, ApplicationDbContext context)
        {
            var interaction = context.Interactions.Where(i => i.ItemID == itemID && i.UserID == userID
                    && i.type == typeOfInteraction).FirstOrDefault();
            if (interaction == null)
            {
                interaction = new Interaction
                {
                    UserID = userID,
                    ItemID = itemID,
                    type = typeOfInteraction,
                    Last = DateTime.Now,
                    NumberOfInteractions = 1

                };
                context.Add(interaction);
            }
            else
            {
                interaction.NumberOfInteractions++;
                interaction.Last = DateTime.Now;
                context.Update(interaction);
            }
            context.SaveChanges();
            interaction.Log();
        }

        /// <summary>
        /// Saves new rating or updates the existing one
        /// </summary>
        /// <param name="itemID">ID of rated item </param>
        /// <param name="userID">ID of user that rated with item</param>
        /// <param name="score">Score of the rating</param>
        /// <param name="context">Database context</param>
        public static void SaveRating(int itemID, int userID, byte score, ApplicationDbContext context)
        {
            var rating = context.Ratings.Where(r => r.ItemID == itemID && r.UserID == userID).FirstOrDefault();
            if (rating == null)
            {
                rating = new Rating
                {
                    UserID = userID,
                    ItemID = itemID,
                    RatingScore = score,
                    Date = DateTime.Now,
                };
                context.Add(rating);
            }
            else
            {
                rating.RatingScore = score;
                rating.Date = DateTime.Now;
                context.Update(rating);
            }
            context.SaveChanges();
            rating.Log();
        }

        /// <summary>
        /// Deletes rating
        /// </summary>
        /// <param name="itemID">ID of unrated item </param>
        /// <param name="userID">ID of user that unrated with item</param>
        /// <param name="context">Database context</param>
        public static void RemoveRating(int itemID, int userID, ApplicationDbContext context)
        {
            var rating = context.Ratings.Where(r => r.ItemID == itemID && r.UserID == userID).FirstOrDefault();
            if (rating != null)
            {
                context.Remove(rating);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Saves new rating or updates the existing one
        /// </summary>
        /// <param name="itemID">ID of rated item </param>
        /// <param name="userID">ID of user that rated with item</param>
        /// <param name="score">Score of the rating</param>
        /// <param name="context">Database context</param>
        public static void SaveUserActSuggestion(int actId, int userID, ApplicationDbContext context)
        {
            var suggestion = context.UserActSuggestions.Where(r => r.ActID == actId && r.UserID == userID).FirstOrDefault();
            if (suggestion == null)
            {
                suggestion = new UserActSuggestion
                {
                    UserID = userID,
                    ActID = actId,
                    NumberOfSuggestions = 1
                };
                context.Add(suggestion);
            }
            else
            {
                suggestion.NumberOfSuggestions++;
                context.Update(suggestion);
            }
            context.SaveChanges();
            suggestion.Log();
        }
    }
}
