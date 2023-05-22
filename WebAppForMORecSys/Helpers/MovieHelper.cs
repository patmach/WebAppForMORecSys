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

        public static string GetDirector(Item movie) => ItemHelper.getPropertyStringValueFromJSON(movie, "Director") ?? "";
        public static string[] GetActors(Item movie) => ItemHelper.getPropertyListValueFromJSON(movie, "Actors");
        public static DateTime? GetReleaseDate(Item movie)
        {
            var stringDate = ItemHelper.getPropertyStringValueFromJSON(movie, "ReleaseDate");
            if (stringDate.IsNullOrEmpty()) return null;
            return DateTime.Parse(stringDate);
        }
        public static string[] GetGenres(Item movie) => ItemHelper.getPropertyListValueFromJSON(movie, "Genres");


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
        
        public static IQueryable<Item> ComputeAllBlockedMovies(this User user, DbSet<Item> allItems)
        {
            StringBuilder filterSQL = new StringBuilder($"SELECT * FROM dbo.{nameof(Item)}s WHERE ");
            filterSQL.Append(getAllBlockedItemsSQLWhere(user));
            return allItems.FromSqlRaw(filterSQL.ToString());
            
        }

        public static string getAllBlockedItemsSQLWhere(User user)
        {
            StringBuilder filterSQL = new StringBuilder();
            var idsBL = UserHelper.GetItemsInBlackList(user);
            var directorsBL = GetDirectorsInBlackList(user);
            var actorsBL = GetActorsInBlackList(user);
            var genresBL = GetGenresInBlackList(user);

            
            if (!directorsBL.IsNullOrEmpty() || !genresBL.IsNullOrEmpty() || !actorsBL.IsNullOrEmpty())
            {
                filterSQL.Append($" (ISJSON({nameof(Item.JSONParams)}) > 0) and ");
            }
            filterSQL.Append("(1!=1");

            if (!idsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or ");
                filterSQL.Append($"(Id IN ({String.Join(",", idsBL)}) ) ");         
            }
            if (!genresBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or EXISTS(");
                filterSQL.Append("SELECT value FROM OPENJSON(JSON_QUERY(JSONParams, '$.Genres'))  WHERE value IN ");
                filterSQL.Append($"({String.Join(",", genresBL.Select(g=> $"'{g.Replace("'", "''")}'"))})) ");

            }
            if (!directorsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or ");
                filterSQL.Append($"(JSON_VALUE(JSONParams, '$.Director') IN ");
                filterSQL.Append($"({String.Join(",",directorsBL.Select(d => $"'{d.Replace("'", "''")}'"))}))");
            }
            if (!actorsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or EXISTS(");
                filterSQL.Append("SELECT value FROM OPENJSON(JSON_QUERY(JSONParams, '$.Actors'))  WHERE value IN ");
                filterSQL.Append($"({String.Join(",", actorsBL.Select(a => $"'{a.Replace("'", "''")}'"))})) ");

            }

            filterSQL.Append(")");
            return filterSQL.ToString();
        }


        public static string AddSingleBlockRule(this User user, string block)
        {
            var blockedValue = block.Split(':');
            if (blockedValue.Length == 2)
            {
                switch (blockedValue[0].ToLower().Trim())
                {
                    case "actor":
                        return AddMultipleBlockRules(user, actor: blockedValue[1].Trim());
                    case "director":
                        return AddMultipleBlockRules(user, director: blockedValue[1].Trim());
                    case "genre":
                        return AddMultipleBlockRules(user, genres: new string[] { blockedValue[1].Trim() });
                    default:
                        return $"There is no property \"{blockedValue[0]}\" which values can be blocked!";
                }
            }
            else if (blockedValue.Length == 1)
            {
                string message = AddMultipleBlockRules(user, director: blockedValue[0].Trim());
                if (!message.IsNullOrEmpty())
                    message = AddMultipleBlockRules(user, actor: blockedValue[0].Trim());
                if (!message.IsNullOrEmpty())
                    message = AddMultipleBlockRules(user, genres: new string[] { blockedValue[0].Trim() });
                if (!message.IsNullOrEmpty())
                    return $"No property contains a value \"{blockedValue[0]}\" that can be blocked!";

            }
            else
            {
                return $"Please specify blocked value in format \"property:value\" or \"value\".";
            }
            return "";

        }

        public static string AddMultipleBlockRules(this User user, string director = null, string actor = null, string[] genres = null)
        {
            var message = new StringBuilder();
            if (!actor.IsNullOrEmpty())
            {
                if (!Movie.AllActors.Contains(actor))
                    message.Append($"Block rule for actor \"{actor}\" was not added. Because there is no actor of that exact name in the database.\\n");
                else
                    user.AddActorToBlackList(actor);

            }
            if (!director.IsNullOrEmpty())
            {
                if (!Movie.AllDirectors.Contains(director))
                    message.Append($"Block rule for director \"{director}\" was not added. Because there is no director of that exact name in the database.\\n");
                else
                    user.AddDirectorToBlackList(director);

            }
            if (!genres.IsNullOrEmpty())
            {
                foreach (var genre in genres)
                {
                    if (!Movie.AllGenres.Contains(genre))
                        message.Append($"Block rule for genre \"{genre}\" was not added. Because there is no genre of that exact name in the database.\\n");
                    else
                        user.AddGenreToBlackList(genre);
                }
            }

            return message.ToString();
        }
    }
}
