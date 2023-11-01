using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Loggers;
using static System.Formats.Asn1.AsnWriter;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Represents user's rating of item
    /// </summary>
    public class Rating
    {
        /// <summary>
        /// Unique Id of rating
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of user that rated the item
        /// </summary>
        [ForeignKey("User")]
        public int UserID { get; set; }

        /// <summary>
        /// ID of item rated by the user
        /// </summary>
        [ForeignKey("Item")]
        public int ItemID { get; set; }

        /// <summary>
        /// Score of the rating
        /// </summary>
        public byte RatingScore { get; set; }

        /// <summary>
        /// Time when user rated the item
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// User that rated the item
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Item rated by the user
        /// </summary>
        public Item Item { get; set; }

        public Rating()
        {

        }

    }

    
}
