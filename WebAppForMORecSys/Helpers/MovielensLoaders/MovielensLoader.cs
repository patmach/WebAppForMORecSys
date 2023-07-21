using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    /// <summary>
    /// Class that contains method that saves movielens data from files to app database
    /// </summary>
    public class MovielensLoader
    {

        public string movielensDataset;
        public bool filter;
        public MovielensLoader(string movielensDataset="25m", bool filter=false) 
        {
            this.movielensDataset = movielensDataset;
            this.filter = filter;
        }

        /// <summary>
        /// Print error to log file
        /// </summary>
        /// <param name="e">Exception that occured</param>
        static void LogError(Exception e)
        {
            File.WriteAllText("movielensloader_log.txt", DateTime.Now.ToString() + e.Message + "\nInner exception:"
                + (e.InnerException?.Message ?? "") + "\n\nST:"
                + e.StackTrace + "\n\n Inner ST:" + (e.InnerException?.StackTrace ?? ""));
        }

        /// <summary>
        /// Loads movielens data from files, gets additional info from TMBD API and saves movies to App database 
        /// </summary>
        /// <param name="context">Database context</param>
        public void LoadMovielensData(ApplicationDbContext context, bool moviesFromLoadedFile=false)
        {
            List<Item> movies;
            List<Link> links;
            if (moviesFromLoadedFile)
            {
                movies = CSVParsingMethods.ParseLoadedMovies();
            }
            else
            {
                movies = CSVParsingMethods.ParseMovies(movielensDataset);
                links = CSVParsingMethods.ParseLinks(movielensDataset);
                string apiKey = File.ReadAllText("apikeyTMBD.txt");

                foreach (var link in links)
                {
                    var movie = movies.Where(m => m.Id == int.Parse(link.Id)).FirstOrDefault();
                    if (movie != null)
                    {
                        JSONParse.AddDetailsToMovie(TMBDApiHelper.getMovieDetail(link.TMBDID).Result, movie);
                        JSONParse.AddCastToMovie(TMBDApiHelper.getMovieCredits(link.TMBDID).Result, movie);
                        JSONParse.AddYoutubeKeyToMovie(TMBDApiHelper.getMovieYoutubeKey(link.TMBDID).Result, movie);

                    }
                }

            }
            try
            {
                var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter= ":::",
                };
                using (var writer = new StreamWriter("moviesloaded.csv"))
                using (var csv = new CsvWriter(writer,configuration))
                {
                    csv.Context.RegisterClassMap<LoadedMovieMap>();
                    csv.WriteRecords(movies);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            var ratings = CSVParsingMethods.ParseRatings(movielensDataset);

            var users = ratings.DistinctBy(r => r.UserID).Select(r => new User { Id = r.UserID, UserName = "movielensUser" + r.UserID }).ToList();

            if (filter)
            {
                ratings = Filtering.RatingLowFilter(ratings);
                movies = Filtering.MovieFilterByYear(movies);
                ratings = Filtering.RatingFilterOld(ratings);
                movies = Filtering.RatingsPerYearFilter(movies, ratings);
                ratings = Filtering.RatingFilter(movies, users, ratings);
                users = Filtering.RatingUserFilter(users, ratings, 100);
                ratings = Filtering.RatingFilter(movies, users, ratings);
                movies = Filtering.RatedMovieFilter(movies, ratings);
                users = Filtering.UsersWithRaitingFilter(users, ratings);
            }
            context.Database.OpenConnection();
            try
            {
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Items ON;");
                context.AddRange(movies);
                context.SaveChanges();
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Items OFF;");


                
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users ON;");
                context.AddRange(users);
                context.SaveChanges();

                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users OFF;");
                int partLength = 100000;
                for (int i = 0; i <= ratings.Count / partLength; i++)
                {
                    var part = ratings.Skip(partLength * i).Take(partLength).ToList();
                    context.AddRange(part);
                    context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine(i + "- " + DateTime.Now.ToString());
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally { context.Database.CloseConnection(); }
        }
    }
}
