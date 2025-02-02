﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        /// ID of section that question belongs to
        /// </summary>
        [ForeignKey("QuestionSection")]
        public int QuestionSectionID { get; set; }

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


        /// <summary>
        /// Section that question belongs to
        /// </summary>
        public QuestionSection QuestionSection { get; set; }

    }

    /// <summary>
    /// Variants of answer types
    /// </summary>
    public enum TypeOfAnswer
    {
        AgreeScale,
        Options,
        Text
    }

    /// <summary>
    /// Used likert scale
    /// </summary>
    public enum AgreeScale
    {
        StronglyAgree,
        Agree,
        Neutral,
        Disagree,
        StronglyDisagree
    }

    /// <summary>
    /// Extension class with ToFriendlyString for enum AgreeScale
    /// </summary>
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
