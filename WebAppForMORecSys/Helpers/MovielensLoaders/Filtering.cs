using System.Linq;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    public static class Filtering
    {
        public static List<Rating> RatingLowFilter(List<Rating> ratings, byte minRatingScore = 8)
        {
            return ratings.Where(rating=> rating.RatingScore >= minRatingScore).ToList();
        }

        public static List<Item> MovieFilterByYear(List<Item> movies, int minYear = 1990)
        {
            return movies.Where(movie => (movie.GetReleaseDate()!=null) 
                && (movie.GetReleaseDate().Value.Year >= minYear)).ToList();
        }

        public static List<Rating> RatingFilterOld(List<Rating> ratings, int latestRatingYear = 2010)
        {
            return ratings.Where(rating=> rating.Date.Year >= latestRatingYear).ToList();
        }

        public static List<Item> RatingsPerYearFilter(List<Item> movies, List<Rating> ratings, int minimalratingsperyear=50)
        {
            int max = movies.Select(movie=>movie.GetReleaseDate().Value.Year).Max() + 1;
            var dictionary = ratings.GroupBy(r => r.ItemID)
                .ToDictionary(group => group.Key, group => group.Count()); 
            int minYear= ratings.Select(rating=> rating.Date.Year).Min();
            return movies.Where(movie => dictionary.ContainsKey(movie.Id) &&
                    dictionary[movie.Id]/ ((max) - Math.Max(minYear, movie.GetReleaseDate().Value.Year))
                    >= minimalratingsperyear).ToList();
        }

        public static List<User> RatingUserFilter(List<User> users, List<Rating> ratings, int minimalratings=100)
        {
            var dictionary = ratings.GroupBy(r => r.UserID)
                .ToDictionary(group => group.Key, group => group.Count()); 
            return users.Where(user => dictionary.ContainsKey(user.Id) &&
                    dictionary[user.Id] >= minimalratings).ToList();
        }

        public static List<Rating> RatingFilter(List<Item> movies, List<User> users, List<Rating> ratings)
        {
            var movieIDs = movies.Select(m => m.Id).ToList();
            var userIDs = users.Select(u => u.Id).ToList();

            return ratings.Where(rating => movieIDs.Contains(rating.ItemID) && userIDs.Contains(rating.UserID)).ToList();
        }

        public static List<Item> RatedMovieFilter(List<Item> movies, List<Rating> ratings)
        {
            var ratedmovies = ratings.Select(r => r.ItemID).Distinct().ToList();
            return movies.Where(movie => ratedmovies.Contains(movie.Id)).ToList();
        }

        public static List<User> UsersWithRaitingFilter(List<User> users, List<Rating> ratings)
        {
            var userswithrating = ratings.Select(r => r.UserID).Distinct().ToList();
            return users.Where(user => userswithrating.Contains(user.Id)).ToList();
        }



    }
}
