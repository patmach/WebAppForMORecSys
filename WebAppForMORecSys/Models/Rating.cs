using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using static System.Formats.Asn1.AsnWriter;

namespace WebAppForMORecSys.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }


        [ForeignKey("Item")]
        public int ItemID { get; set; }

        public byte RatingScore { get; set; }

        public DateTime Date { get; set; }

        public User User { get; set; }

        public Item Item { get; set; }

        public Rating()
        {

        }

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
    }
}
