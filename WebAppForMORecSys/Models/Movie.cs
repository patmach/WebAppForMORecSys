using NuGet.Packaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Controllers;
using WebAppForMORecSys.Helpers;

namespace WebAppForMORecSys.Models
{
    public class Movie : Item
    {
        public string Director => MovieHelper.getPropertyStringValueFromJSON(this, "Director") ?? "";
        public string[] Actors => MovieHelper.getPropertyListValueFromJSON(this,"Actors");
        
        [Display(Name = "Release date")]
        public DateTime ReleaseDate => DateTime.Parse(MovieHelper.getPropertyStringValueFromJSON(this, "ReleaseDate"));
        public string[] Genres => MovieHelper.getPropertyListValueFromJSON(this, "Genres");


        
        public Movie(Item item)
        {
            this.Id = item.Id;
            this.ImageURL = item.ImageURL;
            this.Name= item.Name;
            this.Interactions = item.Interactions;
            this.Ratings = item.Ratings;
            this.Description = item.Description;
            this.ShortDescription= item.ShortDescription;
            this.JSONParams = item.JSONParams;
        }

        public static List<string> GetAllGenres()
        {
            return AllGenres;
        }

        public static List<string> AllGenres;
    }
}
