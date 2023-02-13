using Microsoft.Extensions.Hosting;
using System.Text.Json.Nodes;

namespace WebAppForMORecSys.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public String JSONParams { get; set;}
        public List<Rating> Ratings { get; set; }
        public List<Interaction> Interactions { get; set; }
        public Item()
        {

        }

    }
}


