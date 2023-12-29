using System.ComponentModel.DataAnnotations;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Section of questions
    /// </summary>
    public class QuestionSection
    {
        /// <summary>
        /// Unique ID of the section
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the section
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Questions that belong to this section
        /// </summary>
        public List<Question> Questions { get; set; }
    }
}
