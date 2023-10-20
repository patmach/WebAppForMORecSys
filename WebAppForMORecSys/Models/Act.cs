using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Type of act that is saved
    /// </summary>
    public class Act
    {
        /// <summary>
        /// Unique ID of the act
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique code of the act
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Priority of the act
        /// </summary>
        [Required]
        public int Priority { get; set; }

        /// <summary>
        /// Text of message suggesting this act
        /// </summary>
        [Required]
        public string SuggestionText { get; set; }

        /// <summary>
        /// List of questions that depends on this act
        /// </summary>
        public List<QuestionAct> QuestionsActs { get; set; }

        /// <summary>
        /// List of actions of this type of act done by users
        /// </summary>
        public List<UserAct> UserActs { get; set; }
    }
}
