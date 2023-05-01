using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;

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

        public static void Save(int itemID, int userID, TypeOfInteraction typeOfInteraction, ApplicationDbContext context)
        {
            var interaction = context.Interactions.Where(i => i.ItemID == itemID && i.UserID == userID 
                    && i.type == typeOfInteraction).FirstOrDefault();
            if (interaction == null)
            {
                var newInteraction = new Interaction
                {
                    UserID = userID,
                    ItemID = itemID,
                    type = typeOfInteraction,
                    Last = DateTime.Now,
                    NumberOfInteractions = 1

                };
                context.Add(newInteraction);
            }
            else
            {
                interaction.NumberOfInteractions++;
                interaction.Last = DateTime.Now;
                context.Update(interaction);
            }
            context.SaveChanges();
        }

    }

    public enum TypeOfInteraction
    {
        Click,
        Seen
    }
}
