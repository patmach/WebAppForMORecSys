using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;
using System.Text;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Controllers;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Class that works like Item extension so it can be used as movie
    /// </summary>
    public static class Movie
    {
        /// <summary>
        /// </summary>
        /// <param name="movie">Movie the director should be returned for</param>
        /// <returns>Movie director</returns>
        public static string GetDirector(this Item movie) => ItemHelper.getPropertyStringValueFromJSON(movie, "Director") ?? "";

        /// <summary>
        /// </summary>
        /// <param name="movie">Movie the actors should be returned searched for</param>
        /// <returns>Movie actors</returns>
        public static string[] GetActors(this Item movie) => ItemHelper.getPropertyListValueFromJSON(movie, "Actors");

        /// <summary>
        /// </summary>
        /// <param name="movie">Movie the release date should be returned for</param>
        /// <returns>Release date of the movie</returns>
        public static DateTime? GetReleaseDate(this Item movie)
        {
                DateTime dt = DateTime.MinValue;
                if (DateTime.TryParse(ItemHelper.getPropertyStringValueFromJSON(movie, "ReleaseDate"), out dt))
                    return dt;
                return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="movie">Movie the genres should be returned searched for</param>
        /// <returns>Movie genres</returns>
        public static string[] GetGenres(this Item movie) => ItemHelper.getPropertyListValueFromJSON(movie, "Genres");

        public static string GetYoutubeKey(this Item movie) => ItemHelper.getPropertyStringValueFromJSON(movie, "YoutubeKey") ?? null;


        /// <summary>
        /// Contains all genres that are used for movies
        /// </summary>
        public static List<string> AllGenres;

        /// <summary>
        /// Contains directors of all of the movies in database
        /// </summary>
        public static List<string> AllDirectors;

        /// <summary>
        /// Contains actors of all of the movies in database
        /// </summary>
        public static List<string> AllActors;

        /// <summary>
        /// Filter items according to parameters and returns the result.
        /// </summary>
        /// <param name="allItems">Link to all items stored in the database</param>
        /// <param name="user">User that searched for the specified movies</param>
        /// <param name="search">Text search on name of the movie</param>
        /// <param name="director">Searched director</param>
        /// <param name="actor">Searched actor</param>
        /// <param name="genres">Searched genres</param>
        /// <param name="type">Type of search - text search on name / detailed filter</param>
        /// <param name="releasedateto">Latest searched release date</param>
        /// <param name="releasedatefrom">Earliest searched release date</param>
        /// <returns>Items filtered by the parameters</returns>
        public static IQueryable<Item> GetPossibleItems(DbSet<Item> allItems, User user, string search, string director,
          string actor, string[] genres, string type, string releasedateto, string releasedatefrom)
        {
            
            IQueryable<Item> possibleItems = allItems;
            if (type == "MovieFilter")
            {
                return FilterByMovieFilter(user, allItems, director, actor, genres, releasedatefrom, releasedateto);
            }
            else if ((type == "Search") && (!search.IsNullOrEmpty()))
            {
                /*possibleItems = user.GetAllNotBlockedItems(allItems);*/
                return possibleItems.Where(movie => movie.Name.Contains(search));
            }
            else
            {
                return null;
            }        

        }

        /// <summary>
        /// Filter items according to parameters in detailed filter and returns the result.
        /// </summary>
        /// <param name="allItems">Link to all items stored in the database</param>
        /// <param name="user">User that searched for the specified movies</param>
        /// <param name="director">Searched director</param>
        /// <param name="actor">Searched actor</param>
        /// <param name="genres">Searched genres</param>
        /// <param name="releasedateto">Latest searched release date</param>
        /// <param name="releasedatefrom">Earliest searched release date</param>
        /// <returns>Items filtered by the parameters of detailed filter</returns>
        public static IQueryable<Item> FilterByMovieFilter(User user, DbSet<Item> allItems, string director, string actor, string[] genres,
            string releasedatefrom, string releasedateto)
        {

            StringBuilder filterSQL = new StringBuilder($"SELECT * FROM dbo.{nameof(Item)}s WHERE ISJSON({nameof(Item.JSONParams)}) > 0");
            var sqlp = new List<SqlParameter>();
            if (!director.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"JSON_VALUE({nameof(Item.JSONParams)}, '$.Director') like @director ");
                sqlp.Add(new SqlParameter("@director", $"%{director}%"));
            }
            if (!actor.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"JSON_QUERY({nameof(Item.JSONParams)}, '$.Actors') like @actor");
                sqlp.Add(new SqlParameter("@actor", $"%{actor}%"));
            }
            if (genres != null && genres.Length != 0)
            {
                filterSQL.Append(" and (EXISTS(");
                filterSQL.Append("SELECT value FROM OPENJSON(JSON_QUERY(JSONParams, '$.Genres'))  WHERE value IN ");
                filterSQL.Append($"(@genre0");
                sqlp.Add(new SqlParameter("@genre0", $"{genres[0]}"));
                for (int i = 1; i < genres.Length; i++)
                {
                    filterSQL.Append($",@genre{i}");
                    sqlp.Add(new SqlParameter($"@genre{i}", $"{genres[i]}"));
                }
                filterSQL.Append(")))");
            }
            DateTime dt;
            if (!releasedatefrom.IsNullOrEmpty() && DateTime.TryParse(releasedatefrom, out dt))
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"CONVERT(DATETIME,JSON_VALUE({nameof(Item.JSONParams)}, '$.ReleaseDate')) >= CONVERT(DATETIME,@releasedatefrom) ");
                sqlp.Add(new SqlParameter($"@releasedatefrom", releasedatefrom));
            }
            if (!releasedateto.IsNullOrEmpty() && DateTime.TryParse(releasedateto, out dt))
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"CONVERT(DATETIME,JSON_VALUE({nameof(Item.JSONParams)}, '$.ReleaseDate')) <= CONVERT(DATETIME,@releasedateto) ");
                sqlp.Add(new SqlParameter($"@releasedateto", releasedateto));
            }
            /*filterSQL.Append(" and ");
            filterSQL.Append(MovieHelper.getAllNotBlockedItemsSQLWhere(user));*/
            return allItems.FromSqlRaw(filterSQL.ToString(), sqlp.ToArray()); 
        }

        /// <summary>
        /// Loads all possible genres
        /// </summary>
        /// <param name="context">Database context</param>
        public static void SetAllGenres(ApplicationDbContext context)
        {
            if (Movie.AllGenres == null)
            {
                var genres = new List<string>();
                context.Items.ToList().ForEach(m => genres.AddRange(MovieHelper.GetGenres(m)));
                Movie.AllGenres = genres.Distinct().ToList();
                Movie.AllGenres.Remove("(no genres listed)");
            }
        }

        /// <summary>
        /// Loads all possible directors
        /// </summary>
        /// <param name="context">Database context</param>
        public static void SetAllDirectors(ApplicationDbContext context)
        {
            if (AllDirectors == null)
            {
                var directors = new List<string>();
                context.Items.ToList().ForEach(m => directors.Add(MovieHelper.GetDirector(m)));
                AllDirectors = directors.Distinct().ToList();
            }
        }

        /// <summary>
        /// Loads all possible actors
        /// </summary>
        /// <param name="context">Database context</param>
        public static void SetAllActors(ApplicationDbContext context)
        {
            if (AllActors == null)
            {
                var actors = new List<string>();
                context.Items.ToList().ForEach(m => actors.AddRange(MovieHelper.GetActors(m)));
                AllActors = actors.Distinct().ToList();
            }
        }

    }
}
