using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Data;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Class that represents item. The item can be then viewed on as specified type of item (movie, etc.)
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Unique ID of item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of item
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// URL of item image
        /// </summary>
        public string? ImageURL { get; set; }

        /// <summary>
        /// Full description of item
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Short description of item
        /// </summary>
        [Display(Name = "Short\ndescription")]
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Other params. The structure of JSON corresponds to what item represent
        /// </summary>
        public string? JSONParams { get; set;}

        /// <summary>
        /// Ratings of item
        /// </summary>
        public List<Rating> Ratings { get; set; }

        /// <summary>
        /// Interactions with item
        /// </summary>
        public List<Interaction> Interactions { get; set; }

        public Item()
        {

        }
    }
}


