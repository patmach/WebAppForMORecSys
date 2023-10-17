using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Act done by user
    /// </summary>
    public class UserAct
    {
        /// <summary>
        /// Unique ID of the action made by user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of type of the act 
        /// </summary>
        [ForeignKey("Act")]
        public int ActID { get; set; }

        /// <summary>
        /// ID of user who has done the action
        /// </summary>
        [ForeignKey("User")]
        public int UserID { get; set; }


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
