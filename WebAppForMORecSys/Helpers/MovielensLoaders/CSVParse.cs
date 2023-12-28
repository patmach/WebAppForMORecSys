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
        public static List<Item> ParseMovies(string movielensDataset)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = movielensDataset == "25m" ? true : movielensDataset == "1m" ? false : false,
                Delimiter = movielensDataset == "25m" ? "," : movielensDataset == "1m" ? "::" : ""
            };
            List<Item> movies = new List<Item>();
            string moviesfile = movielensDataset == "25m" ? "Resources/Movielens25m/movies.csv" :
                movielensDataset == "1m" ? "Resources/ml-1m/movies.dat" : "";
            using (var reader = new StreamReader(moviesfile))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<MovieMap>();
                movies = csv.GetRecords<Item>().ToList();
            }
            movies.ForEach(x => { x.JSONParams = "\"Genres\":[\"" + x.JSONParams.Replace("|", "\",\"") + "\"]"; });

            return movies;
        }

        /// <summary>
        /// Read the csv file with parsed movies. It should be called
        /// when movies where loaded but not everzthing was successfully added to db.
        /// </summary>
        /// <returns>List of all movies with all the info</returns>
        public static List<Item> ParseLoadedMovies()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ":::",
            };
            List<Item> movies = new List<Item>();
            string moviesfile = "moviesloaded.csv";
            using (var reader = new StreamReader(moviesfile))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<LoadedMovieMap>();
                movies = csv.GetRecords<Item>().ToList();
            }

            return movies;
        }

        /// <summary>
        /// Read the csv/dat file and creates list of movie links (IMBD ID and TMBD ID) corresponding to the file.
        /// </summary>
        /// <returns>List of movie links</returns>
        public static List<Link> ParseLinks(string movielensDataset)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<Link> links = new List<Link>();
            var linksfile = movielensDataset == "25m" ? "Resources/Movielens25m/links.csv" :
                movielensDataset == "1m" ? "Resources/ml-1m/links.csv" : "";
            using (var reader = new StreamReader(linksfile))
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
        public static List<Rating> ParseRatings(string movielensDataset)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = movielensDataset == "25m" ? true : movielensDataset == "1m" ? false : false,
                Delimiter = movielensDataset == "25m" ? "," : movielensDataset == "1m" ? "::" : ""
            };
            List<Rating> ratings = new List<Rating>();
            var ratingsfile = movielensDataset == "25m" ? "Resources/Movielens25m/ratings.csv" :
                movielensDataset == "1m" ? "Resources/ml-1m/ratings.dat" : "";
            using (var reader = new StreamReader(ratingsfile))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<RatingMap>();
                ratings = csv.GetRecords<Rating>().ToList();
            }
            return ratings;
        }


        /*
        public static List<Act> ParseActs()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";"
            };
            List<Act> acts = new List<Act>();
            var actsfile = "Acts.csv";
            using (var reader = new StreamReader(actsfile))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<ActMap>();
                acts = csv.GetRecords<Act>().ToList();
            }
            return acts;
        }

        public static List<Question> ParseQuestions()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };
            List<Question> questions = new List<Question>();
            var questionsfile = "Questions.csv";
            using (var reader = new StreamReader(questionsfile))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<QuestionMap>();
                questions = csv.GetRecords<Question>().ToList();
            }
            return questions;
        }

        public static List<Answer> ParseAnswers()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };
            List<Answer> answers = new List<Answer>();
            var answersfile = "Answers.csv";
            using (var reader = new StreamReader(answersfile))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<AnswerMap>();
                answers = csv.GetRecords<Answer>().ToList();
            }
            return answers;
        }*/
    }
    /*
    public class AnswerMap : ClassMap<Answer>
    {
        public AnswerMap()
        {
            Map(p => p.Text).Index(0);
            Map(p => p.QuestionID).Convert(args => int.Parse(args.Row.GetField(1)));
        }
    }public class QuestionMap : ClassMap<Question>
    {
        public QuestionMap()
        {
            Map(p => p.Text).Index(0);
            Map(p => p.AnswerType).Convert(args => (TypeOfAnswer)int.Parse(args.Row.GetField(1)));
        }
    }
    public class ActMap : ClassMap<Act>
    {
        public ActMap()
        {
            Map(p => p.Priority).Convert(args => int.Parse(args.Row.GetField(0)));
            Map(p => p.SuggestionText).Index(1);
            Map(p => p.Code).Index(2);
        }
    }

     */
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

    public class LoadedMovieMap : ClassMap<Item>
    {
        public LoadedMovieMap()
        {
            Map(p => p.Id).Index(0);
            Map(p => p.Name).Index(1);
            Map(p => p.ImageURL).Index(2);
            Map(p => p.ShortDescription).Index(3);
            Map(p => p.JSONParams).Index(4);
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
