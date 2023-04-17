using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Models;
using static WebAppForMORecSys.Helpers.UserHelper;
using NuGet.Packaging;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Helpers
{
    public static class MovieHelper
    {

        public class PreviewDetailViewModel {
            public Movie movie;
            public User user;
            public List<Rating> userRatings;
            public int[] metricsContribution;
            public PreviewDetailViewModel(Movie movie, User user, List<Rating> userRatings, int[] metricsContribution = null)
            {
                this.movie = movie;
                this.user = user;
                this.userRatings = userRatings;
                this.metricsContribution = metricsContribution;
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
        
        public static IQueryable<Item> ComputeAllNotBlockedMovies(this User user, DbSet<Item> allItems)
        {
            StringBuilder filterSQL = new StringBuilder($"SELECT * FROM dbo.{nameof(Item)}s WHERE 1= 1 and ");
            filterSQL.Append(getAllNotBlockedItemsSQLWhere(user));
            return allItems.FromSqlRaw(filterSQL.ToString());
            
        }

        public static string getAllNotBlockedItemsSQLWhere(User user)
        {
            StringBuilder filterSQL = new StringBuilder();
            var idsBL = UserHelper.GetItemsInBlackList(user);
            var directorsBL = GetDirectorsInBlackList(user);
            var actorsBL = GetActorsInBlackList(user);
            var genresBL = GetGenresInBlackList(user);

            if (!directorsBL.IsNullOrEmpty() || !genresBL.IsNullOrEmpty() || !actorsBL.IsNullOrEmpty())
                filterSQL.Append($" ((ISJSON({nameof(Item.JSONParams)}) > 0) ");

            if (!idsBL.IsNullOrEmpty())
            {
                ;
                filterSQL.Append(" and ");
                filterSQL.Append($"(Id != {idsBL.First()} )"); ;
                for (int i = 1; i < idsBL.Count; i++)
                {
                    filterSQL.Append(" and ");
                    filterSQL.Append($"(Id  != {idsBL[i]}) ");
                }


            }
            if (!genresBL.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"(JSON_QUERY({nameof(Item.JSONParams)}, '$.Genres') not like '%{genresBL.First()}%' )");
                for (int i = 1; i < genresBL.Count; i++)
                {
                    filterSQL.Append(" and ");
                    filterSQL.Append($"(JSON_QUERY({nameof(Item.JSONParams)}, '$.Genres') not like '%{genresBL[i]}%' )");
                }

            }
            if (!directorsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"(JSON_VALUE({nameof(Item.JSONParams)}, '$.Director') != '{directorsBL.First()}' )"); ;
                for (int i = 1; i < directorsBL.Count; i++)
                {
                    filterSQL.Append(" and ");
                    filterSQL.Append($"(JSON_VALUE({nameof(Item.JSONParams)}, '$.Director')  != '{directorsBL[i]}' )");
                }


            }
            if (!actorsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"(JSON_QUERY({nameof(Item.JSONParams)}, '$.Actors') not like '%{actorsBL.First()}%' )");
                for (int i = 1; i < actorsBL.Count; i++)
                {
                    filterSQL.Append(" and ");
                    filterSQL.Append($"(JSON_QUERY({nameof(Item.JSONParams)}, '$.Actors') not like '%{actorsBL[i]}%' )");
                }

            }

            filterSQL.Append(")");
            return filterSQL.ToString();
        }

        //change destination of method
        public static string MetricsContributionToBorderImage(int[] metricsContribution)
        {
            StringBuilder borderImage = new StringBuilder();
            borderImage.Append("linear-gradient(to bottom right");
            int lastpoint = 0;
            int sum = 0;
            for (int i = 0; i < metricsContribution.Length; i++)
            {
                sum += metricsContribution[i];
                borderImage.Append(',');
                borderImage.Append(SystemParameters.MetricsColors[i]);
                borderImage.Append(' ');
                borderImage.Append(lastpoint);
                borderImage.Append("% ");
                borderImage.Append(sum);
                borderImage.Append('%');
                lastpoint = sum;
            }
            borderImage.Append(") 1");
            return borderImage.ToString();
        }
    }
}
