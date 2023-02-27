using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Data;

namespace WebAppForMORecSys.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? ImageURL { get; set; }

        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? JSONParams { get; set;}
        public List<Rating> Ratings { get; set; }
        public List<Interaction> Interactions { get; set; }

        public Item()
        {

        }
        public static List<Item>GetAll() { 
            List<Item> items = new List<Item>();
            items = context.Items.ToList();
            return items; 
        }

        public static ApplicationDbContext context;
    }
}


