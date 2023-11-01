using Microsoft.Build.Logging;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Loggers;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Represents interaction between user and item beside rating
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// Unique Id of interaction
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of user that interacted with item
        /// </summary>
        [ForeignKey("User")]
        public int UserID { get; set; }

        /// <summary>
        /// ID of item with which the interaction occur
        /// </summary>
        [ForeignKey("Item")]
        public int ItemID { get; set; }

        /// <summary>
        /// Type of interaction - seen, click
        /// </summary>
        public TypeOfInteraction type { get; set; }

        /// <summary>
        /// How many times the interaction between user and item occured
        /// </summary>
        public int NumberOfInteractions { get; set; }

        /// <summary>
        /// Last time this interaction occur
        /// </summary>
        public DateTime Last { get; set; }

        /// <summary>
        /// User that interacted with item
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Item with which the interaction occur
        /// </summary>
        public Item Item { get; set; }

        public Interaction() { }        

    }

    public enum TypeOfInteraction
    {
        Click,
        Seen
    }

   
}
