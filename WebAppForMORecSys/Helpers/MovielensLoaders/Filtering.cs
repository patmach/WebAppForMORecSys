using System.Linq;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    /// <summary>
    /// Filter records from MovieLens dataset
    /// </summary>
    public static class Filtering
    {
        /// <summary>
        /// </summary>
        /// <param name="ratings">List of ratings</param>
        /// <param name="minRatingScore">Minimal rating score for the rating to be kept</param>
        /// <returns>List of ratings with rating score greater or equal to minRatingScore</returns>
        public static List<Rating> RatingLowFilter(List<Rating> ratings, byte minRatingScore = 8)
        {
            return ratings.Where(rating=> rating.RatingScore >= minRatingScore).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="movies">List of movies</param>
        /// <param name="minYear">Minimal year of the release date for movie to be kept</param>
        /// <returns>List of movies without the old ones</returns>
        public static List<Item> MovieFilterByYear(List<Item> movies, int minYear = 1990)
        {
            return movies.Where(movie => (movie.GetReleaseDate()!=null) 
                && (movie.GetReleaseDate().Value.Year >= minYear)).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="ratings">List of rating</param>
        /// <param name="latestRatingYear">Latest year of rating for it to be kept</param>
        /// <returns>List of ratings without the old ones</returns>
        public static List<Rating> RatingFilterOld(List<Rating> ratings, int latestRatingYear = 2010)
        {
            return ratings.Where(rating=> rating.Date.Year >= latestRatingYear).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="movies">List of movies</param>
        /// <param name="ratings">List of ratings</param>
        /// <param name="minimalratingsperyear">Minimal ratings of movies per year since its release date</param>
        /// <returns>List of movies with enough ratings</returns>
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

        /// <summary>
        /// </summary>
        /// <param name="users">List of users</param>
        /// <param name="ratings">List of ratings</param>
        /// <param name="minimalratings">Minimal number of ratings for user for him to be kept</param>
        /// <returns>List of users with enough ratings</returns>
        public static List<User> RatingUserFilter(List<User> users, List<Rating> ratings, int minimalratings=100)
        {
            var dictionary = ratings.GroupBy(r => r.UserID)
                .ToDictionary(group => group.Key, group => group.Count()); 
            return users.Where(user => dictionary.ContainsKey(user.Id) &&
                    dictionary[user.Id] >= minimalratings).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="movies">List of movies</param>
        /// <param name="users">List of users</param>
        /// <param name="ratings">List of ratings</param>
        /// <returns>Ratings of movies by users from the lists given as arguments</returns>
        public static List<Rating> RatingFilter(List<Item> movies, List<User> users, List<Rating> ratings)
        {
            var movieIDs = movies.Select(m => m.Id).ToList();
            var userIDs = users.Select(u => u.Id).ToList();

            return ratings.Where(rating => movieIDs.Contains(rating.ItemID) && userIDs.Contains(rating.UserID)).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="movies">List of movies</param>
        /// <param name="ratings">List of ratings</param>
        /// <returns>List of movies that are part of the ratings</returns>
        public static List<Item> RatedMovieFilter(List<Item> movies, List<Rating> ratings)
        {
            var ratedmovies = ratings.Select(r => r.ItemID).Distinct().ToList();
            return movies.Where(movie => ratedmovies.Contains(movie.Id)).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="users">List of users</param>
        /// <param name="ratings">List of ratings</param>
        /// <returns>List of users that are part of the ratings</returns>
        public static List<User> UsersWithRaitingFilter(List<User> users, List<Rating> ratings)
        {
            var userswithrating = ratings.Select(r => r.UserID).Distinct().ToList();
            return users.Where(user => userswithrating.Contains(user.Id)).ToList();
        }



    }
}
