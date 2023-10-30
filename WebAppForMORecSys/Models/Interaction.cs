using Microsoft.Build.Logging;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
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

        /// <summary>
        /// Saves new interaction or updates the existing one
        /// </summary>
        /// <param name="itemID">ID of item with which the interaction occur</param>
        /// <param name="userID">ID of user that interacted with item</param>
        /// <param name="typeOfInteraction">Type of interaction</param>
        /// <param name="context">Database context</param>
        public static void Save(int itemID, int userID,
            TypeOfInteraction typeOfInteraction, ApplicationDbContext context)
        {
            var interaction = context.Interactions.Where(i => i.ItemID == itemID && i.UserID == userID
                    && i.type == typeOfInteraction).FirstOrDefault();
            if (interaction == null)
            {
                interaction = new Interaction
                {
                    UserID = userID,
                    ItemID = itemID,
                    type = typeOfInteraction,
                    Last = DateTime.Now,
                    NumberOfInteractions = 1

                };
                context.Add(interaction);
            }
            else
            {
                interaction.NumberOfInteractions++;
                interaction.Last = DateTime.Now;
                context.Update(interaction);
            }
            context.SaveChanges();
            interaction.GetLogger().Log($"{interaction.UserID};{interaction.ItemID};{interaction.type};" +
                $"{interaction.Last.ToString(interaction.GetLogger().format)}");
        }

    }

    public enum TypeOfInteraction
    {
        Click,
        Seen
    }

    public static class InteractionExtensions
    {
        /// <summary>
        /// For logging of every interaction to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/Interactions.txt");

        /// <summary>
        /// For logging of every interactio to file
        /// </summary>
        public static MyFileLogger GetLogger(this Interaction interaction) => logger;
    }
}
