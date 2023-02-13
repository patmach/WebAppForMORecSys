using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;

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
    }
}
