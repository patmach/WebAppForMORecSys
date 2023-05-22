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
    public static class Movie
    {
        public static string GetDirector(this Item movie) => ItemHelper.getPropertyStringValueFromJSON(movie, "Director") ?? "";
        public static string[] GetActors(this Item movie) => ItemHelper.getPropertyListValueFromJSON(movie, "Actors");

        public static DateTime? GetReleaseDate(this Item movie)
        {
                DateTime dt = DateTime.MinValue;
                if (DateTime.TryParse(ItemHelper.getPropertyStringValueFromJSON(movie, "ReleaseDate"), out dt))
                    return dt;
                return null;
        }
        public static string[] GetGenres(this Item movie) => ItemHelper.getPropertyListValueFromJSON(movie, "Genres");

        

        public static List<string> AllGenres;

        public static List<string> AllDirectors;

        public static List<string> AllActors;

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

        public static IQueryable<Item> FilterByMovieFilter(User user, DbSet<Item> possibleitems, string director, string actor, string[] genres,
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
                filterSQL.Append($"(@genres))) ");
                sqlp.Add(new SqlParameter("@genres", $"%{String.Join(",", genres.Select(g => $"'{g}'"))}%"));
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
            return possibleitems.FromSqlRaw(filterSQL.ToString(), sqlp.ToArray()); 
        }


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

        public static void SetAllDirectors(ApplicationDbContext context)
        {
            if (AllDirectors == null)
            {
                var directors = new List<string>();
                context.Items.ToList().ForEach(m => directors.Add(MovieHelper.GetDirector(m)));
                AllDirectors = directors.Distinct().ToList();
            }
        }

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
