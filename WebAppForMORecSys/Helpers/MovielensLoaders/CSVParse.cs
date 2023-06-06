using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Globalization;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    /// <summary>
    /// Contains methods that parse Movielens Datasets
    /// </summary>
    public static class CSVParsingMethods
    {
        /// <summary>
        /// Read the csv/dat file and creates list of movies corresponding to the file.
        /// </summary>
        /// <returns>List of all movies</returns>
        public static List<Item> ParseMovies()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "::"
            };
            List<Item> movies = new List<Item>();
            using (var reader = new StreamReader("Resources/ml-1m/movies.dat"))//("Resources/Movielens25m/movies.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<MovieMap>();
                movies = csv.GetRecords<Item>().ToList();
            }
            movies.ForEach(x => { x.JSONParams = "\"Genres\":[\"" + x.JSONParams.Replace("|", "\",\"") + "\"]"; });

            return movies;
        }

        /// <summary>
        /// Read the csv/dat file and creates list of movie links (IMBD ID and TMBD ID) corresponding to the file.
        /// </summary>
        /// <returns>List of movie links</returns>
        public static List<Link> ParseLinks()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<Link> links = new List<Link>();
            using (var reader = new StreamReader("Resources/ml-1m/links.csv"))//("Resources/Movielens25m/links.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<LinkMap>();
                links = csv.GetRecords<Link>().ToList();
            }
            return links;
        }

        /// <summary>
        /// Read the csv/dat file and creates list of ratings corresponding to the file.
        /// </summary>
        /// <returns>List of ratings</returns>
        public static List<Rating> ParseRatings()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "::"
            };
            List<Rating> ratings = new List<Rating>();
            using (var reader = new StreamReader("Resources/ml-1m/ratings.dat"))//("Resources/Movielens25m/ratings.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<RatingMap>();
                ratings = csv.GetRecords<Rating>().ToList();
            }
            return ratings;
        }
    }

    /// <summary>
    /// Class that describes mapping between movies.csv/movies.dat file and Movie Class
    /// </summary>
    public class MovieMap : ClassMap<Item>
    {
        public MovieMap()
        {
            Map(p => p.Id).Convert(args => int.Parse(args.Row.GetField(0)));
            Map(p => p.Name).Index(1);
            Map(p => p.JSONParams).Index(2);
        }
    }

    /// <summary>
    /// Class corresponding to the links.csv file
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Movielens ID of movie
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// IMBD ID of movie
        /// </summary>
        public string IMBDID { get; set; }
        /// <summary>
        /// TMBD ID of movie
        /// </summary>
        public string TMBDID { get; set; }
        public Link()
        {

        }

    }

    /// <summary>
    /// Class that describes mapping between links.csv file and Link Class
    /// </summary>
    public class LinkMap : ClassMap<Link>
    {
        public LinkMap()
        {
            Map(p => p.Id).Index(0);
            Map(p => p.IMBDID).Index(1);
            Map(p => p.TMBDID).Index(2);
        }
    }

    /// <summary>
    /// Class that describes mapping between ratings.csv/ratings.dat file and Rating Class
    /// </summary>
    public class RatingMap : ClassMap<Rating>
    {
        public RatingMap()
        {
            Map(p => p.UserID).Convert(args => int.Parse(args.Row.GetField(0)));
            Map(p => p.ItemID).Convert(args => int.Parse(args.Row.GetField(1)));
            Map(p => p.RatingScore).Convert(args =>
                (byte)(2 * double.Parse(args.Row.GetField(2), CultureInfo.InvariantCulture)));
            Map(p => p.Date).Convert(args => UnixTimeStampToDateTime(long.Parse(args.Row.GetField(3))));

        }

        /// <summary>
        /// </summary>
        /// <param name="unixTimeStamp">Timestamp in UNIX specification</param>
        /// <returns>DateTime corresponding to that timestamp</returns>
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

}
