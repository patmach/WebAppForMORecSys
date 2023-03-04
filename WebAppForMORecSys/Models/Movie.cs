using System.Collections.Generic;
using System.Drawing.Text;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Controllers;

namespace WebAppForMORecSys.Models
{
    public class Movie : Item
    {
        public string Director => getPropertyStringValueFromJSON("Director");
        public string[] Actors => getPropertyListValueFromJSON("Actors");
        public DateTime ReleaseDate => DateTime.Parse(getPropertyStringValueFromJSON("ReleaseDate"));
        public string[] Genres => getPropertyListValueFromJSON("Genres");


        public string getPropertyStringValueFromJSON(string property)
        {
            try
            {
                if (JSONParams == null) return null;
                JsonObject? Params = (JsonObject?)JsonObject.Parse(JSONParams);
                JsonNode jsonNode;
                if (Params != null && Params.TryGetPropertyValue(property, out jsonNode))
                {
                    return jsonNode.ToString();
                }
            }
            catch(Exception e)
            {
                var x = e.Message;
            }
            return null;
        }

        public string[] getPropertyListValueFromJSON(string property)
        {
            string stringResult = getPropertyStringValueFromJSON(property);
            if (stringResult !=null)
            {
                stringResult = stringResult.Replace("[", "").Replace("]","").Replace("\"", "").Replace(" ", "")
                    .Replace(Environment.NewLine, "");
                return stringResult.Split(',');
            }
            return new string[0];
        }
        public Movie(Item item)
        {
            this.Id = item.Id;
            this.ImageURL = item.ImageURL;
            this.Name= item.Name;
            this.Interactions = item.Interactions;
            this.Ratings= item.Ratings;
            this.Description = item.Description;
            this.ShortDescription= item.ShortDescription;
            this.JSONParams = item.JSONParams;
        }
        public static List<Movie> GetAll()
        {
            return Item.GetAll().Select(i=> new Movie(i)).ToList();
        }

        public static List<string> GetAllGenres()
        {
            if (AllGenres == null)
            {
                var genres = new List<string>();
                var movies = GetAll();
                movies.ForEach(m => genres.AddRange(m.Genres));
                AllGenres = genres.Distinct().ToList();
                AllGenres.Remove("(nogenreslisted)");
            }
            return AllGenres;
        }

        private static List<string> AllGenres;
    }
}
