using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Loggers;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// User answer for question
    /// </summary>
    public class UserAnswer
    {
        /// <summary>
        /// Unique ID of the user answer
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Link to question that has been answered
        /// </summary>
        [ForeignKey("Question")]
        public int QuestionID { get; set; }

        /// <summary>
        /// Link to user that has answered
        /// </summary>
        [ForeignKey("User")]
        public int UserID { get; set; }

        /// <summary>
        /// Set if type of the answer is AgreeScale
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// Set if type of the answer is Options
        /// </summary>
        [ForeignKey("Answer")]
        public int? AnswerID { get; set; }

        /// <summary>
        /// Set if type of the answer is Text
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Time when user answered
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Question that has been answered
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// User that has answered
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Set if type of the answer is Options
        /// </summary>
        public Answer? Answer { get; set; }


        /// <summary>
        /// Saves user's answer to a question
        /// </summary>
        /// <param name="user">User that answered</param>
        /// <param name="questionID">Question that was answered</param>
        /// <param name="answerID">If the answer is TypeOfAnswer.Option answer ID is saved as answer</param>
        /// <param name="value">If the answer is TypeOfAnswer.AgreeScale value is saved as answer</param>
        /// <param name="text">If the answer is TypeOfAnswer.Text text is saved as answer</param>
        /// <param name="context">Database context</param>
        public static void Save(User user, int questionID, int? answerID, int? value, string? text, ApplicationDbContext context)
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
            useranswer.GetLogger().Log($"{useranswer.UserID};{useranswer.QuestionID};" +
                $"{useranswer.Date.ToString(useranswer.GetLogger().format)};{useranswer.AnswerID.ToString() ?? ""};" +
                $"{useranswer.Value.ToString() ?? ""};{useranswer.Text ?? ""}") ;
        }

    }

    public static class UserAnswerExtension
    {

        /// <summary>
        /// For logging of every userAnswer to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/UserAnswers.txt");

        /// <summary>
        /// For logging of every act to file
        /// </summary>
        public static MyFileLogger GetLogger(this UserAnswer userAnswer) => logger;
    }
}
