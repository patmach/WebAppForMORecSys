using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Globalization;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.ParseHelpers
{
    public static class CSVParsingMethods
    {
        public static List<Item> ParseMovies()
        {            
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<Item> movies = new List<Item>();
            using (var reader = new StreamReader("Resources/Movielens25m/movies.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<MovieMap>();
                movies = csv.GetRecords<Item>().ToList();
            }
            movies.ForEach(x => { x.JSONParams = "Genres:[\"" + x.JSONParams.Replace("|","\",\"") + "\"]"; });

            return movies;
        }

        public static List<Link> ParseLinks()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<Link> links = new List<Link>();
            using (var reader = new StreamReader("Resources/Movielens25m/links.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<LinkMap>();
                links = csv.GetRecords<Link>().ToList();
            }
            return links;
        }
        public static List<Rating> ParseRatings()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<Rating> ratings = new List<Rating>();
            using (var reader = new StreamReader("Resources/Movielens25m/ratings.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<RatingMap>();
                ratings = csv.GetRecords<Rating>().ToList();
            }
            return ratings;
        }
    }
    public class MovieMap : ClassMap<Item>
    {
        public MovieMap()
        {
            Map(p => p.Id).Convert(args => int.Parse(args.Row.GetField("movieId")));
            Map(p => p.Name).Index(1);
            Map(p => p.JSONParams).Index(2);
        }
    }

    public class Link
    {

        public string Id { get; set; }
        public string IMBDID { get; set; }
        public string TMBDID { get; set; }
        public Link()
        {

        }

    }
    public class LinkMap : ClassMap<Link>
    {
        public LinkMap()
        {
            Map(p => p.Id).Index(0);
            Map(p => p.IMBDID).Index(1);
            Map(p => p.TMBDID).Index(2);
        }
    }

    public class RatingMap : ClassMap<Rating>
    {
        public RatingMap()
        {
            Map(p => p.UserID).Convert(args => int.Parse(args.Row.GetField("userId")));
            Map(p => p.ItemID).Convert(args => int.Parse(args.Row.GetField("movieId")));
            Map(p => p.RatingScore).Convert(args => 
                (byte)(2*double.Parse(args.Row.GetField("rating"), CultureInfo.InvariantCulture)));
            Map(p => p.Date).Convert(args => UnixTimeStampToDateTime(long.Parse(args.Row.GetField("timestamp"))));

        }
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
    
}
