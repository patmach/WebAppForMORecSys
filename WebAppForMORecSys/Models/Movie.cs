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
                possibleItems = possibleItems.Where(movie => movie.Name.Contains(search));
            }
            else
            {
                return null;
            }
        

            return possibleItems;
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
                filterSQL.Append(" and (");
                filterSQL.Append($"JSON_QUERY({nameof(Item.JSONParams)}, '$.Genres') like @genres0 ");
                sqlp.Add(new SqlParameter("@genres0", $"%{genres.First()}%"));
                for (int i = 1; i < genres.Length; i++)
                {
                    filterSQL.Append(" or ");
                    filterSQL.Append($"JSON_QUERY({nameof(Item.JSONParams)}, '$.Genres') like  @genres{i} ");
                    sqlp.Add(new SqlParameter($"@genres{i}", $"%{genres[i]}%"));
                }

                filterSQL.Append(')');
            }
            if (!releasedatefrom.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"CONVERT(DATETIME,JSON_VALUE({nameof(Item.JSONParams)}, '$.ReleaseDate')) >= CONVERT(DATETIME,@releasedatefrom) ");
                sqlp.Add(new SqlParameter($"@releasedatefrom", releasedatefrom));
            }
            if (!releasedateto.IsNullOrEmpty())
            {
                filterSQL.Append(" and ");
                filterSQL.Append($"CONVERT(DATETIME,JSON_VALUE({nameof(Item.JSONParams)}, '$.ReleaseDate')) <= CONVERT(DATETIME,@releasedateto) ");
                sqlp.Add(new SqlParameter($"@releasedateto", releasedateto));
            }
            /*filterSQL.Append(" and ");
            filterSQL.Append(MovieHelper.getAllNotBlockedItemsSQLWhere(user));*/
            return possibleitems.FromSqlRaw(filterSQL.ToString(), sqlp.ToArray()); 
        }

    }
}
