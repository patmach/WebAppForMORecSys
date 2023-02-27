using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.ParseHelpers
{
    public class MovielensLoader
    {
        public MovielensLoader() { }

        static readonly HttpClient httpClient = new HttpClient();
        void PrintError(Exception e)
        {
            System.IO.File.WriteAllText("log.txt", e.Message + "\nInner exception:" + (e.InnerException?.Message ?? "") + "\n\nST:"
                + e.StackTrace + "\n\n Inner ST:" + (e.InnerException?.StackTrace ?? ""));
        }

        public void LoadMovielensData(ApplicationDbContext context)
        {
            List<Item> movies = CSVParsingMethods.ParseMovies();            
            var links = CSVParsingMethods.ParseLinks();
            string apiKey = System.IO.File.ReadAllText("apikeyTMBD.txt");
            
            foreach (var link in links)
            {
                var movie = movies.Where(m=> m.Id == int.Parse(link.Id)).FirstOrDefault();
                JSONParse.AddDetailsToMovie(TMBDApiHelper.getMovieDetail(link).Result, movie);
                JSONParse.AddCastToMovie(TMBDApiHelper.getMovieCredits(link).Result, movie);
            }
            if (movies.Count > 1000)
            {
                try
                {
                    movies.ForEach(m => { m.JSONParams = m.JSONParams.Replace("\n", ""); });
                    using (var writer = new StreamWriter("moviesloaded.csv"))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(movies);
                    }
                }
                catch (Exception ex)
                {
                    PrintError(ex);
                }
            }
            context.Database.OpenConnection();
            try
            {
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Items ON;");
                context.AddRange(movies);
                context.SaveChanges();
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Items OFF;");


                var ratings = CSVParsingMethods.ParseRatings();
                var users = ratings.DistinctBy(r=>r.UserID).Select(r => new User { Id = r.UserID, UserName = "movielensUser" + r.UserID });
              
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users ON;");
                context.AddRange(users);
                context.SaveChanges();
                
                context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users OFF;");
                int partLength = 100000;
                for (int i = 153; i < ratings.Count / partLength; i++)
                {
                    var part = ratings.Skip(partLength * i).Take(partLength).ToList();
                    context.AddRange(part);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
            finally { context.Database.CloseConnection(); }
        }
    }
}
