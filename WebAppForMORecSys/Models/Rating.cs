using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
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

        /// <summary>
        /// Saves new rating or updates the existing one
        /// </summary>
        /// <param name="itemID">ID of item with which the interaction occur</param>
        /// <param name="userID">ID of user that interacted with item</param>
        /// <param name="typeOfInteraction">Type of interaction</param>
        /// <param name="context">Database context</param>
        public static void Save(int itemID, int userID, byte score, ApplicationDbContext context)
        {
            var rating = context.Ratings.Where(r => r.ItemID == itemID && r.UserID == userID).FirstOrDefault();
            if (rating == null)
            {
                var newRating = new Rating
                {
                    UserID = userID,
                    ItemID = itemID,
                    RatingScore = score,
                    Date = DateTime.Now,
                };
                context.Add(newRating);
            }
            else
            {
                rating.RatingScore = score;
                rating.Date = DateTime.Now;
                context.Update(rating);
            }
            context.SaveChanges();
        }

        public static void Remove(int itemID, int userID,ApplicationDbContext context)
        {
            var rating = context.Ratings.Where(r => r.ItemID == itemID && r.UserID == userID).FirstOrDefault();
            if (rating != null)
            {
                context.Remove(rating);
                context.SaveChanges();
            }
            
        }
    }
}
