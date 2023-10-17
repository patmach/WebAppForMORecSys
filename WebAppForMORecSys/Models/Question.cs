using System.ComponentModel.DataAnnotations;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Question used in user study
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Unique ID of the question
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Text of the question
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Type of the answer
        /// </summary>
        [Required]
        public TypeOfAnswer AnswerType {  get; set; }

        /// <summary>
        /// List of answers if AnswerType is optional
        /// </summary>
        public List<Answer>? Answers { get; set; }

        /// <summary>
        /// List of acts this question depends on
        /// </summary>
        public List<QuestionAct> QuestionsActs { get; set; }

        /// <summary>
        /// Users answers on this question
        /// </summary>
        public List<UserAnswer> UserAnswers { get; set; }
    }

    public enum TypeOfAnswer
    {
        AgreeScale,
        Options,
        Text
    }
}
