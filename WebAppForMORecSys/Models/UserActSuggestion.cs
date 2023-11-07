using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Acts suggested to user
    /// </summary>
    public class UserActSuggestion
    {
        /// <summary>
        /// Unique ID of the suggestion
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of type of the act that was suggested
        /// </summary>
        [ForeignKey("Act")]
        public int ActID { get; set; }

        /// <summary>
        /// ID of user to whom the action was suggested
        /// </summary>
        [ForeignKey("User")]
        public int UserID { get; set; }

        /// <summary>
        /// Number of times the action was suggested to the user
        /// </summary>
        public int NumberOfSuggestions { get; set; }

        /// <summary>
        /// User who has done the action
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Type of the act 
        /// </summary>
        public Act Act { get; set; }
    }
}
