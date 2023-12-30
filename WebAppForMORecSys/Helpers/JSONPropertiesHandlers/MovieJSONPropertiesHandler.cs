using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Models;
using static WebAppForMORecSys.Helpers.JSONPropertiesHandlers.UserJSONPropertiesHandler;
using NuGet.Packaging;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Models.ItemDomainExtension.Movie;

namespace WebAppForMORecSys.Helpers.JSONPropertiesHandlers
{
    /// <summary>
    /// Extension class for class Item. Enables to work with item as movie.
    /// </summary>
    public static class MovieJSONPropertiesHandler
    {

        /// <summary>
        /// </summary>
        /// <param name="movie">Selected movie</param>
        /// <returns>Director of the movie</returns>
        public static string GetDirector(Item movie) => ItemJSONPropertiesHandler.getPropertyStringValueFromJSON(movie, "Director") ?? "";

        /// <summary>
        /// </summary>
        /// <param name="movie">Selected movie</param>
        /// <returns>Actors that played in the movie</returns>
        public static string[] GetActors(Item movie) => ItemJSONPropertiesHandler.getPropertyListValueFromJSON(movie, "Actors");

        /// <summary>
        /// </summary>
        /// <param name="movie">Selected movie</param>
        /// <returns>Release date of the movie</returns>
        public static DateTime? GetReleaseDate(Item movie)
        {
            var stringDate = ItemJSONPropertiesHandler.getPropertyStringValueFromJSON(movie, "ReleaseDate");
            if (stringDate.IsNullOrEmpty()) return null;
            return DateTime.Parse(stringDate);
        }

        /// <summary>
        /// </summary>
        /// <param name="movie">Selected movie</param>
        /// <returns>Genres of the movie</returns>
        public static string[] GetGenres(Item movie) => ItemJSONPropertiesHandler.getPropertyListValueFromJSON(movie, "Genres");

        /// <summary>
        /// Adds actor to block rules
        /// </summary>
        /// <param name="user">User who adds the actor name to the blocked values</param>
        /// <param name="actor">Name of the actor</param>
        public static void AddDirectorToBlackList(this User user, string director)
        {
            user.AddStringValueToBlackList("Director", director);
        }

        /// <summary>
        /// Adds director to block rules
        /// </summary>
        /// <param name="user">User who adds the director name to the blocked values</param>
        /// <param name="director">Name of the director</param>
        public static void AddActorToBlackList(this User user, string actor)
        {
            user.AddStringValueToBlackList("Actor", actor);
        }

        /// <summary>
        /// Adds genre to block rules
        /// </summary>
        /// <param name="user">User who adds the genre name to the blocked values</param>
        /// <param name="genre">Name of the genre</param>
        public static void AddGenreToBlackList(this User user, string genre)
        {
            user.AddStringValueToBlackList("Genre", genre);
        }

        /// <summary>
        /// Remove director from the block rules
        /// </summary>
        /// <param name="user">User who removes the director name from the blocked values</param>
        /// <param name="director">Name of the director</param>
        public static void RemoveDirectorFromBlackList(this User user, string director)
        {
            user.RemoveStringValueFromBlackList("Director", director);
        }

        /// <summary>
        /// Remove actor from the block rules
        /// </summary>
        /// <param name="user">User who removes the actor name from the blocked values</param>
        /// <param name="actor">Name of the actor</param>
        public static void RemoveActorFromBlackList(this User user, string actor)
        {
            user.RemoveStringValueFromBlackList("Actor", actor);
        }

        /// <summary>
        /// Remove genre from the block rules
        /// </summary>
        /// <param name="user">User who removes the genre name from the blocked values</param>
        /// <param name="genre">Name of the genre</param>
        public static void RemoveGenreFromBlackList(this User user, string genre)
        {
            user.RemoveStringValueFromBlackList("Genre", genre);
        }

        /// <summary>
        /// Checks if director is in user's blocked values.
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <param name="director">Name of the director</param>
        public static bool IsDirectorInBlackList(this User user, string director)
        {
            return user.IsStringValueInBlackList("Director", director);
        }

        /// <summary>
        /// Checks if actor is in user's blocked values.
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <param name="actor">Name of the actor</param>
        public static bool IsActorInBlackList(this User user, string actor)
        {
            return user.IsStringValueInBlackList("Actor", actor);
        }

        /// <summary>
        /// Checks if genre is in user's blocked values.
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <param name="genre">Name of the genre</param>
        public static bool IsGenreInBlackList(this User user, string genre)
        {
            return user.IsStringValueInBlackList("Genre", genre);
        }


        /// <summary>
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <returns>Names of all blocked directors by user</returns>
        public static List<string> GetDirectorsInBlackList(this User user)
        {
            return user.GetStringValuesInBlackList("Director");
        }

        /// <summary>
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <returns>Names of all blocked actors by user</returns>
        public static List<string> GetActorsInBlackList(this User user)
        {
            return user.GetStringValuesInBlackList("Actor");
        }

        /// <summary>
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <returns>All blocked genres by user</returns>
        public static List<string> GetGenresInBlackList(this User user)
        {
            return user.GetStringValuesInBlackList("Genre");
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose blocked list should be computed</param>
        /// <param name="allItems">Database context for items</param>
        /// <returns>Queryable of all user's blocked movies</returns>
        public static IQueryable<Item> ComputeAllBlockedMovies(this User user, DbSet<Item> allItems)
        {
            StringBuilder filterSQL = new StringBuilder($"SELECT * FROM dbo.{nameof(Item)}s WHERE ");
            filterSQL.Append(getAllBlockedItemsSQLWhere(user));
            return allItems.FromSqlRaw(filterSQL.ToString());

        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose blocked list should be computed</param>
        /// <returns>Part of sql query for the where clause to compute all blocked movies</returns>
        public static string getAllBlockedItemsSQLWhere(User user)
        {
            StringBuilder filterSQL = new StringBuilder();
            var idsBL = user.GetItemsInBlackList();
            var directorsBL = user.GetDirectorsInBlackList();
            var actorsBL = user.GetActorsInBlackList();
            var genresBL = user.GetGenresInBlackList();


            if (!directorsBL.IsNullOrEmpty() || !genresBL.IsNullOrEmpty() || !actorsBL.IsNullOrEmpty())
            {
                filterSQL.Append($" (ISJSON({nameof(Item.JSONParams)}) > 0) and ");
            }
            filterSQL.Append("(1!=1");

            if (!idsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or ");
                filterSQL.Append($"(Id IN ({string.Join(",", idsBL)}) ) ");
            }
            if (!genresBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or EXISTS(");
                filterSQL.Append("SELECT value FROM OPENJSON(JSON_QUERY(JSONParams, '$.Genres'))  WHERE value IN ");
                filterSQL.Append($"({string.Join(",", genresBL.Select(g => $"'{g.Replace("'", "''")}'"))})) ");

            }
            if (!directorsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or ");
                filterSQL.Append($"(JSON_VALUE(JSONParams, '$.Director') IN ");
                filterSQL.Append($"({string.Join(",", directorsBL.Select(d => $"'{d.Replace("'", "''")}'"))}))");
            }
            if (!actorsBL.IsNullOrEmpty())
            {
                filterSQL.Append(" or EXISTS(");
                filterSQL.Append("SELECT value FROM OPENJSON(JSON_QUERY(JSONParams, '$.Actors'))  WHERE value IN ");
                filterSQL.Append($"({string.Join(",", actorsBL.Select(a => $"'{a.Replace("'", "''")}'"))})) ");

            }

            filterSQL.Append(")");
            return filterSQL.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User that adds new block rule</param>
        /// <param name="block">Block rule in format "property: value"</param>
        /// <returns>Message if the addition of the new block rule wasnt successful</returns>
        public static string AddSingleBlockRule(this User user, string block)
        {
            var blockedValue = block.Split(':');
            if (blockedValue.Length == 2)
            {
                switch (blockedValue[0].ToLower().Trim())
                {
                    case "actor":
                        return user.AddMultipleBlockRules(actor: blockedValue[1].Trim());
                    case "director":
                        return user.AddMultipleBlockRules(director: blockedValue[1].Trim());
                    case "genre":
                        return user.AddMultipleBlockRules(genres: new string[] { blockedValue[1].Trim() });
                    default:
                        return $"There is no property \"{blockedValue[0]}\" which values can be blocked!";
                }
            }
            else if (blockedValue.Length == 1)
            {
                string message = user.AddMultipleBlockRules(director: blockedValue[0].Trim());
                if (!message.IsNullOrEmpty())
                    message = user.AddMultipleBlockRules(actor: blockedValue[0].Trim());
                if (!message.IsNullOrEmpty())
                    message = user.AddMultipleBlockRules(genres: new string[] { blockedValue[0].Trim() });
                if (!message.IsNullOrEmpty())
                    return $"No property contains a value \"{blockedValue[0]}\" that can be blocked!";

            }
            else
            {
                return $"Please specify blocked value in format \"property:value\" or \"value\".";
            }
            return "";

        }

        /// <summary>
        /// </summary>
        /// <param name="user">User that adds new block rule</param>
        /// <param name="director">Director to be blocked</param>
        /// <param name="actor">Actor to be blocked</param>
        /// <param name="genres">Genres to be blocked</param>
        /// <returns>Message if the addition of the new block rule wasnt successful</returns>
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
