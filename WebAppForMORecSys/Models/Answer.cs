using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Answer for questions with type of answer options in user study
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// Unique ID of the answer
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Text of the answer
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Link to question that can be answered with this answer
        /// </summary>
        [ForeignKey("Question")]
        public int QuestionID { get; set; }

        /// <summary>
        /// Check if answer contains image for better understanding
        /// </summary>
        public bool? HasImage { get; set; }

        /// <summary>
        /// Question that can be answered with this answer
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// List of all answers with this answer
        /// </summary>
        public List<UserAnswer>? UserAnswers { get; set; }
    }
}
