using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Models;
using static WebAppForMORecSys.Helpers.UserHelper;
using NuGet.Packaging;

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


        public static void AddDirectorToBlackList(this User user, string director)
        {
            UserHelper.AddStringValueToBlackList(user, "Director", director);
        }

        public static void AddActorToBlackList(this User user, string actor)
        {
            UserHelper.AddStringValueToBlackList(user, "Actor", actor);
        }

        public static void AddGenreToBlackList(this User user, string genre)
        {
            UserHelper.AddStringValueToBlackList(user, "Genre", genre);
        }

        public static void RemoveDirectorFromBlackList(this User user, string director)
        {
            UserHelper.RemoveStringValueFromBlackList(user, "Director", director);
        }

        public static void RemoveActorFromBlackList(this User user, string actor)
        {
            UserHelper.RemoveStringValueFromBlackList(user, "Actor", actor);
        }

        public static void RemoveGenreFromBlackList(this User user, string genre)
        {
            UserHelper.RemoveStringValueFromBlackList(user, "Genre", genre);
        }

        public static bool IsDirectorInBlackList(this User user, string actor)
        {
            return UserHelper.IsStringValueInBlackList(user, "Director", actor);
        }

        public static bool IsActorInBlackList(this User user, string actor)
        {
            return UserHelper.IsStringValueInBlackList(user, "Actor", actor);
        }

        public static bool IsGenreInBlackList(this User user, string genre)
        {
            return UserHelper.IsStringValueInBlackList(user, "Genre", genre);
        }


        public static List<string> GetDirectorsInBlackList(this User user)
        {
            return UserHelper.GetStringValuesInBlackList(user, "Director");
        }

        public static List<string> GetActorsInBlackList(this User user)
        {
            return UserHelper.GetStringValuesInBlackList(user, "Actor");
        }

        public static List<string> GetGenresInBlackList(this User user)
        {
            return UserHelper.GetStringValuesInBlackList(user, "Genre");
        }

        public static List<int> ComputeAllBlockedMovies(this User user)
        {
            var blackList = new List<int>();
            blackList.AddRange(UserHelper.GetItemsInBlackList(user));
            var directorsBL = GetDirectorsInBlackList(user);
            var actorsBL = GetActorsInBlackList(user);
            var genresBL = GetGenresInBlackList(user);
            blackList.AddRange(Movie.AllMovies.Where(m => (m.Actors.Intersect(actorsBL).Count() > 0) ||
                                                    (m.Genres.Intersect(genresBL).Count() > 0) ||
                                                    (directorsBL.Contains(m.Director)))
                                                    .Select(m=> m.Id).ToList());
            blackList = blackList.Distinct().ToList();
            return blackList;
        }
    }
}
