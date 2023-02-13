using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;

namespace WebAppForMORecSys.Models
{
    public class Interaction
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Item")]
        public int ItemID { get; set; }

        public TypeOfInteraction type { get; set; }

        public int NumberOfInteractions { get; set; }

        public DateTime Last { get; set; }

        public User User { get; set; }

        public Item Item { get; set; }

        public Interaction() { }

    }

    public enum TypeOfInteraction
    {
        Click,
        Seen
    }
}
