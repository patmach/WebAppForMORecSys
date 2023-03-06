using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers
{
    public static class MovieHelper
    {

        public class MovieUserUserratings {
            public Movie movie;
            public User user;
            public List<Rating> userRatings;
            public MovieUserUserratings(Movie movie, User user, List<Rating> userRatings)
            {
                this.movie = movie;
                this.user = user;
                this.userRatings = userRatings;
            }
        }
        public static string getPropertyStringValueFromJSON(Item movie, string property)
        {
            try
            {
                if (movie.JSONParams == null) return "";
                JsonObject? Params = (JsonObject?)JsonObject.Parse(movie.JSONParams);
                JsonNode jsonNode;
                if (Params != null && Params.TryGetPropertyValue(property, out jsonNode))
                {
                    return jsonNode.ToString();
                }
            }
            catch (Exception e)
            {
                var x = e.Message;
            }
            return "";
        }

        public static string[] getPropertyListValueFromJSON(Item movie, string property)
        {
            string stringResult = getPropertyStringValueFromJSON(movie, property);
            if (!stringResult.IsNullOrEmpty())
            {
                stringResult = stringResult.Replace("[", "").Replace("]", "").Replace("\"", "").Replace(", ",",")
                    .Replace(" ,",",").Replace(Environment.NewLine, "");
                var list =  stringResult.Split(',').ToList();
                return list.Select(g => g.Trim()).ToArray();
            }
            return new string[0];
        }

        public static string GetDirector(Item movie) => MovieHelper.getPropertyStringValueFromJSON(movie, "Director") ?? "";
        public static string[] GetActors(Item movie) => MovieHelper.getPropertyListValueFromJSON(movie, "Actors");
        public static DateTime? GetReleaseDate(Item movie)
        {
            var stringDate = MovieHelper.getPropertyStringValueFromJSON(movie, "ReleaseDate");
            if (stringDate.IsNullOrEmpty()) return null;
            return DateTime.Parse(stringDate);
        }
        public static string[] GetGenres(Item movie) => MovieHelper.getPropertyListValueFromJSON(movie, "Genres");
    }
}
