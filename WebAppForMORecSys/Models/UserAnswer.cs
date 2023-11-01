using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
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


    }

    
}
