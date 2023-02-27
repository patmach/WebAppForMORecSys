using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WebAppForMORecSys.Models.HomeViewsModels
{
    public class MovieFilterModel
    {
        public static List<string> Directors;
        public static List<string> Actors;
        public static List<string> Genres;
        public MovieFilterModel()
        {
        }
        public static void Load()
        {
            if (Actors == null)
            {
                var movies = Movie.GetAll();
                Directors = movies.Select(m => m.Director).Distinct().ToList();
                Genres = new List<string>();
                movies.ForEach(m => Genres.AddRange(m.Genres));
                Genres = Genres.Distinct().ToList();
                Actors = new List<string>();
                movies.ForEach(m => Actors.AddRange(m.Actors));
                Actors = Actors.Distinct().ToList();
            }

        }
    }
}
