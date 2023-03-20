using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.HomeViewsModels;
using static WebAppForMORecSys.Helpers.MovieHelper;

namespace WebAppForMORecSys.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Account> _userManager;

       IQueryable<Item> allItems;

        public MoviesController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
            SetAllMovies();
            SetAllGenres();
            SetAllDirectors();
            SetAllActors();
            allItems = from item in _context.Items
                           select item;
        }

        public async Task<IActionResult> Index(string search, string[] metricimportance, string director,
          string actor, string[] genres, string type, string releasedateto, string releasedatefrom)
        {
            var viewModel = new MainViewModel();
            User user = GetCurrentUser();
            if (user != null)
            {
                viewModel.CurrentUser = user;
                viewModel.CurrentUserRatings = await (from rating in _context.Ratings
                                               where rating.UserID == viewModel.CurrentUser.Id
                                               select rating).ToListAsync();
            }
            var metrics = from metric in _context.Metrics
                          select metric;
            viewModel.SearchValue = search ?? "";
            viewModel.Metrics = await metrics.ToListAsync();
            viewModel.Metrics = new List<Metric> { new Metric { Name = "Relevance" },
                new Metric { Name = "Novelty" }, new Metric { Name = "Diversity" } };//DELETE Later
            List<Item> possibleItems = new List<Item>();
            if (type.IsNullOrEmpty())
                possibleItems = await allItems.ToListAsync();
            else if (type == "Search" && search != null)
            {
                possibleItems = await allItems.Where(x => x.Name.ToLower().Contains(search.ToLower())).ToListAsync();
            }
            else if (type == "MovieFilter")
            {
                possibleItems = FilterByMovieFilter(director, actor, genres, releasedatefrom, releasedateto);
                viewModel.FilterValues.Add("Director", director);
                viewModel.FilterValues.Add("Actor", actor);
                viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
                viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
                viewModel.FilterValues.Add("Genres", string.Join(',', genres));
            }
            viewModel.Items = possibleItems.Take(50).ToList();//Nahradit voláním RS
            return View(viewModel);
        }

        public List<Item> FilterByMovieFilter(string director, string actor, string[] genres,
            string releasedatefrom, string releasedateto)
        {

            List<Item> itemslist = allItems.ToList();
            if (!director.IsNullOrEmpty())
                itemslist = itemslist.Where(i => (MovieHelper.GetDirector(i) ?? "")
                    .Contains(director, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!actor.IsNullOrEmpty())
                itemslist = itemslist.Where(i => MovieHelper.GetActors(i)
                    .Any(a => a.Contains(actor, StringComparison.OrdinalIgnoreCase))).ToList();
            if (genres != null && genres.Length != 0)
                itemslist = itemslist.Where(i => MovieHelper.GetGenres(i).Intersect(genres).Count() > 0).ToList();
            if (!releasedatefrom.IsNullOrEmpty())
                itemslist = itemslist.Where(i => MovieHelper.GetReleaseDate(i) > DateTime.Parse(releasedatefrom)).ToList();
            if (!releasedateto.IsNullOrEmpty())
                itemslist = itemslist.Where(i => MovieHelper.GetReleaseDate(i) < DateTime.Parse(releasedateto)).ToList();
            return itemslist;
        }

        public async Task<IActionResult> MovieDetails(int id)
        {
            User user = GetCurrentUser();
            List<Rating> ratings = null;
            if (user != null)
            {
                user =  user;
                ratings = await(from rating in _context.Ratings
                                                     where rating.UserID == user.Id
                                                     select rating).ToListAsync();
            }
            return PartialView(new MovieUserUserratings(
                new Movie(_context.Items.First(x => x.Id == id)),
                user,
                ratings
                ));

        }

        public void SetAllGenres()
        {            
            if (Movie.AllGenres == null)
            {
                var genres = new List<string>();
                Movie.AllMovies.ForEach(m => genres.AddRange(MovieHelper.GetGenres(m)));
                Movie.AllGenres = genres.Distinct().ToList();
                Movie.AllGenres.Remove("(nogenreslisted)");
            }
        }

        public void SetAllDirectors()
        {
            if (Movie.AllDirectors == null)
            {
                var directors = new List<string>();
                Movie.AllMovies.ForEach(m => directors.Add(MovieHelper.GetDirector(m)));
                Movie.AllDirectors = directors.Distinct().ToList();
            }
        }

        public void SetAllActors()
        {
            if (Movie.AllActors == null)
            {
                var actors = new List<string>();
                Movie.AllMovies.ForEach(m => actors.AddRange(MovieHelper.GetActors(m)));
                Movie.AllActors = actors.Distinct().ToList();
            }
        }

        public void SetAllMovies()
        {
            if (Movie.AllActors == null)
            {
                var actors = new List<string>();
                Movie.AllMovies = _context.Items.Select(i => new Movie(i)).ToList();
            }
        }

        public List<string> GetAllMovieNames(string prefix)
        {
            return Movie.AllMovies.Where(m => m.Name.Contains(prefix, StringComparison.OrdinalIgnoreCase)).Select(m => m.Name).ToList();

        }
        public List<string> GetAllDirectors(string prefix)
        {
            return Movie.AllDirectors.Where(d => d.Contains(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        }
        public List<string> GetAllActors(string prefix)
        {
            return Movie.AllActors.Where(a => a.Contains(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        }



        public IResult Rate(int id, int score)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            var rating = _context.Ratings.Where(r => r.ItemID == id && r.UserID == user.Id).FirstOrDefault();
            if (rating == null)
            {
                var newRating = new Rating
                {
                    UserID = user.Id,
                    ItemID = id,
                    RatingScore = (byte)score,
                    Date = DateTime.Now,
                };
                _context.Add(newRating);
            }
            else
            {
                rating.RatingScore = (byte)score;
                rating.Date = DateTime.Now;
                _context.Update(rating);
            }
            _context.SaveChanges();
            return Results.NoContent();
        }

        private User GetCurrentUser()
        {
            var account = _userManager.GetUserAsync(User).Result;
            User user = null;
            if (account != null)
            {
                user = _context.Users.Where(u => u.UserName == account.UserName).FirstOrDefault();
            }
            return user;
        }
        public IResult Hide(int id)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddItemToBlackList(id);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult HideDirector(string director)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddDirectorToBlackList(director);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult HideActor(string actor)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddActorToBlackList(actor);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult HideGenre(string genre)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddGenreToBlackList(genre);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult Show(int id)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveItemFromBlackList(id);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult ShowDirector(string director)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveDirectorFromBlackList(director);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult ShowActor(string actor)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveActorFromBlackList(actor);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }

        public IResult ShowGenre(string genre)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveGenreFromBlackList(genre);
            _context.Update(user);
            _context.SaveChanges();
            return Results.NoContent();
        }
    }
}
