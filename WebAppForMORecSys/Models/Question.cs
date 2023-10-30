using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Settings;
using static System.Net.Mime.MediaTypeNames;

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
        public TypeOfAnswer AnswerType { get; set; }

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

    public enum AgreeScale
    {
        StronglyAgree,
        Agree,
        Neutral,
        Disagree,
        StronglyDisagree
    }

    public static class AgreeScaleExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="agreeScale"></param>
        /// <returns>Name to display for the enum value</returns>
        public static string ToFriendlyString(this AgreeScale agreeScale)
        {
            switch (agreeScale)
            {
                case AgreeScale.StronglyDisagree:
                    return "Strongly disagree";
                case AgreeScale.Disagree:
                    return "Disagree";
                case AgreeScale.Neutral:
                    return "Neutral / Don't know";
                case AgreeScale.Agree:
                    return "Agree";
                case AgreeScale.StronglyAgree:
                    return "Strongly agree";
                default:
                    break;
            }
            return "";
        }
    }
}
