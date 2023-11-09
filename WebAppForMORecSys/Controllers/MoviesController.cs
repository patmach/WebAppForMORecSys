using CsvHelper;
using Elfie.Serialization;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using NuGet.Protocol;
using NuGet.Protocol.Plugins;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Razor.Parser.SyntaxTree;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Helpers.MovielensLoaders;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.RequestHandlers;
using WebAppForMORecSys.Settings;
using static WebAppForMORecSys.Helpers.MovieJSONPropertiesHandler;
using static WebAppForMORecSys.Helpers.UserActHelper;
using Interaction = WebAppForMORecSys.Models.Interaction;

namespace WebAppForMORecSys.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        /// <summary>
        /// Database context
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// User manager for accesing acount the app communicates with
        /// </summary>
        private readonly UserManager<Account> _userManager;

        private readonly MovieRequestsHandler _requestsHandler;
        /// <summary>
        /// Gets connection to db and UserManager, saves possible values of movie properties
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        public MoviesController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
            _requestsHandler = new MovieRequestsHandler(context);
            //Uncomment next line if you want to load movielens data to the database
            //new MovielensLoader(filter: true).LoadMovielensData(context, moviesFromLoadedFile: true);
            /*var answers = CSVParsingMethods.ParseAnswers();
            _context.AddRange(answers);
            _context.SaveChanges(); */           
            Movie.SetAllGenres(context);
            Movie.SetAllDirectors(context);
            Movie.SetAllActors(context);          
        }

        /// <summary>
        /// Loads movies main page.Filters possible movies from user input and sends request to Recommender API.
        /// </summary>
        /// <param name="search">Main user search on title</param>
        /// <param name="director">Movie filter user search on director</param>
        /// <param name="actor">Movie filter user search on actor></param>
        /// <param name="genres">Movie filter user search on genres</param>
        /// <param name="typeOfSearch">Specifies if user clicked the search for main search or in movie filter</param>
        /// <param name="releasedateto">Movie filter user search on the latest release date</param>
        /// <param name="releasedatefrom">Movie filter user search on the earliest release date</param>
        /// <param name="metricsimportance">Metrics importance in % according to user input</param>
        /// <returns>View with the main page and recommended movies</returns>
        public async Task<IActionResult> Index(string search, string director,
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom, 
          string[] metricsimportance)
        {
            User user = GetCurrentUser();
            var viewModel = _requestsHandler.ProcessMainQuery(user,search, director, actor, genres, typeOfSearch,
                releasedateto, releasedatefrom, metricsimportance).Result;
            if (!viewModel.Message.IsNullOrEmpty())
                TempData["msg"] = viewModel.Message;
            AddUserActsFromMainViewModel(viewModel, typeOfSearch, _context);
            if (viewModel.Info.IsNullOrEmpty())
                viewModel.Info = FindUserActTip(user.Id, _context, Request, viewModel.UserRatings.Count);
            return View(viewModel);
        }

        /// <summary>
        /// Filters possible movies from user input and sends request to Recommender API.
        /// </summary>
        /// <param name="search">Main user search on title</param>
        /// <param name="director">Movie filter user search on director</param>
        /// <param name="actor">Movie filter user search on actor></param>
        /// <param name="genres">Movie filter user search on genres</param>
        /// <param name="typeOfSearch">Specifies if user clicked the search for main search or in movie filter</param>
        /// <param name="releasedateto">Movie filter user search on the latest release date</param>
        /// <param name="releasedatefrom">Movie filter user search on the earliest release date</param>
        /// <param name="metricsimportance">Metrics importance in % according to user input</param>
        /// <param name="l">IDs of recommended movies displayed on the page</param>
        /// <returns>Partial view with recommended movies</returns>
        public async Task<IActionResult> Recommendations(string search, string director,
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom,
          string[] metricsimportance, int[] l)
        {
            int[] currentList = l; 
            User user = GetCurrentUser();
            var viewModel = _requestsHandler.ProcessMainQuery(user, search, director, actor, genres, typeOfSearch,
                releasedateto, releasedatefrom, metricsimportance, currentList).Result;
            return PartialView(viewModel);
        }

        /// <summary>
        /// Process new blocking rule if the method was POST and returns page with blocks management
        /// </summary>
        /// <param name="search">Search in blocked items. (Possible for user only if he blocks a lot of movies)</param>
        /// <param name="block">New blocked value in format 'name_of_property: value'. Used with single block rule creation.</param>
        /// <param name="director">New blocked value for property director. Used with multiple block rule creation.</param>
        /// <param name="actor">New blocked value for property actor. Used with multiple block rule creation.</param>
        /// <param name="genres">New blocked value for property genres. Used with multiple block rule creation.</param>
        /// <returns>Page where user can manage her/his block settings.</returns>
        public async Task<IActionResult> UserBlockSettings(string search, string block, string director,string actor, 
            string[] genres)
        {
            User user = GetCurrentUser();
            string method = HttpContext.Request.Method;
            var viewModel = _requestsHandler.ProcessBlockSettings(user, search, block, director, actor, genres, method).Result;
            if (!viewModel.Message.IsNullOrEmpty())
                TempData["msg"] = "<script>alert('" + viewModel.Message + "');</script>";
            return View(viewModel);
        }


        /// <summary>
        /// </summary>
        /// <param name="id">Id of movie</param>
        /// <returns>Preview partial view for a movie</returns>
        public async Task<IActionResult> Preview(int id)
        {
            User user = GetCurrentUser();
            List<Rating> ratings = await (from rating in _context.Ratings
                                 where rating.UserID == user.Id
                                 select rating).ToListAsync();            
            return PartialView(new PreviewDetailViewModel(
                _context.Items.First(x => x.Id == id),
                user,
                ratings 
                ));
        }


        /// <summary>
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <returns>Partial view of details of the movie</returns>
        public async Task<IActionResult> Details(int id)
        {
            User user = GetCurrentUser();
            List<Rating> ratings = await _context.Ratings.Where(r=> r.UserID == user.Id).ToListAsync();            
            SaveMethods.SaveInteraction(id, user.Id, TypeOfInteraction.Click, _context);
            UserActCache.AddAct(user.Id.ToString(), "DetailsClicked", _context);
            return PartialView(new PreviewDetailViewModel(
                _context.Items.First(x => x.Id == id),
                user,
                ratings
                ));

        }

        /// <summary>
        /// Instance of random, used by FindUserActsTips
        /// </summary>
        public static Random rnd = new Random();

        /// <summary>
        /// </summary>
        /// <returns>Preview partial view for a random movie from the list of rated by user</returns>
        public async Task<IActionResult> PreviewOfRandomRated(int? questionID)
        {
            User user = GetCurrentUser();
            List<Rating> ratings = await _context.Ratings.Where(r => r.UserID == user.Id).ToListAsync();
            int randomRatedItemID = -1;
            if (questionID.HasValue)
            {
                var savedItemID = SanityCheckCache.Get(user.Id, questionID.Value);
                if (savedItemID.HasValue) {
                    randomRatedItemID = savedItemID.Value;
                }
                else {
                    randomRatedItemID = ratings[rnd.Next(ratings.Count)].ItemID;
                    SanityCheckCache.Add(user.Id, questionID.Value, randomRatedItemID);
                }
            }
            else
            {
                randomRatedItemID = ratings[rnd.Next(ratings.Count)].ItemID;                
            }
            return RedirectToAction("Preview","Movies", new { id = randomRatedItemID });
        }


        /// <summary>
        /// </summary>
        /// <param name="prefix">Searched value</param>
        /// <returns>All movie names that contains searched value</returns>
        public List<string> GetAllNames(string prefix)
        {
            User user = GetCurrentUser();
            return _context.Items.Where(m => EF.Functions.Like(m.Name, $"%{prefix}%"))
                /*.Except(user.GetAllBlockedItems(_context.Items))*/
                .Select(m => m.Name).Take(15).ToList();

        }

        /// <summary>
        /// </summary>
        /// <param name="prefix">Searched value</param>
        /// <returns>All movie directors that contains searched value</returns>
        public List<string> GetAllDirectors(string prefix)
        {
            User user = GetCurrentUser();
            return Movie.AllDirectors.Where(d => d.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                .Except(user.GetDirectorsInBlackList()).Take(10).ToList();

        }

        /// <summary>
        /// </summary>
        /// <param name="prefix">Searched value</param>
        /// <returns>All movie actors that contains searched value</returns>
        public List<string> GetAllActors(string prefix)
        {
            User user = GetCurrentUser();
            return Movie.AllActors.Where(a => a.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                .Except(user.GetActorsInBlackList()).Take(10).ToList();

        }


        /// <summary>
        /// </summary>
        /// <param name="prefix">Searched value</param>
        /// <returns>All movie genres that contains searched value</returns>
        public List<string> GetAllGenres(string prefix)
        {
            User user = GetCurrentUser();
            return Movie.AllGenres.Where(g => g.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                .Except(user.GetGenresInBlackList()).Take(10).ToList();

        }

        /// <summary>
        /// Returns distribution of genres in user profile.
        /// </summary>
        /// <returns>Text representation of user profiles</returns>
        public string GetUserProfileForCalibration()
        {
            User user = GetCurrentUser();
            var genres = Movie.AllGenres;
            var positively_rated = _context.Ratings.Where(r => (r.UserID == user.Id) && (r.RatingScore > 5))
                .Include(r=> r.Item).Select(r => r.Item).Distinct().ToList();
            var genresProb = new double[genres.Count];
            foreach (var movie in positively_rated)
            {
                var moviegenres = movie.GetGenres().ToList();
                moviegenres.Remove("(no genres listed)");
                foreach (var genre in moviegenres)
                {
                    genresProb[genres.IndexOf(genre)] += 1;
                }
            }
            double sum = genresProb.Sum();
            if (sum > 0)
            {
                for (int i = 0; i < genresProb.Length; i++)
                {
                    genresProb[i] /= sum;
                }
            }
            List<int> indicesOfTop = new List<int>();
            var listGenresProb = genresProb.ToList();
            for (int i = 0; i < 4; i++)
            {
                var index = listGenresProb.IndexOf(listGenresProb.Max());
                listGenresProb[index] = -1;
                indicesOfTop.Add(index);
            }
            StringBuilder sb = new StringBuilder("Your profile ratio - (");
            foreach(int i in indicesOfTop)
            {
                if (genresProb[i] > 0)
                {
                    sb.Append(genres[i]);
                    sb.Append(": ");
                    sb.Append(Math.Round(genresProb[i] * 100, 0));
                    sb.Append(" %, ");
                }
            }
            sb.Append("...)");
            return sb.ToString();

        }

        /// <summary>
        /// </summary>
        /// <param name="prefix">Searched value</param>
        /// <returns>All values of movie properties (in format 'name_of_property: value'  that contains searched value</returns>
        public List<string> GetAllPossibleValuesToBlock(string prefix)
        {
            List<string> possibleBlocks =  new List<string>();
            possibleBlocks.AddRange(GetAllGenres(prefix).Take(5).Select(g=> $"Genre: {g}"));
            possibleBlocks.AddRange(GetAllDirectors(prefix).Take(5).Select(d => $"Director: {d}"));
            possibleBlocks.AddRange(GetAllActors(prefix).Take(5).Select(a => $"Actor: {a}"));
            return possibleBlocks;
        }


        /// <summary>
        /// </summary>
        /// <returns>Currently logged user that sent this request.</returns>
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

        /// <summary>
        /// Add director to blocked.
        /// </summary>
        /// <param name="director">Director to be blocked</param>
        /// <returns>HTTP response without content</returns>
        public IResult HideDirector(string director)
        {
            if (!Movie.AllDirectors.Contains(director))
                return Results.NoContent();
            User user = GetCurrentUser();
            user.AddDirectorToBlackList(director);
            _context.Update(user);
            _context.SaveChanges();
            UserActCache.AddAct(user.Id.ToString(), "PropertyBlock", _context);
            user.LogBlock("director", director);
            return Results.NoContent();
        }

        /// <summary>
        /// Add actor to blocked.
        /// </summary>
        /// <param name="actor">Actor to be blocked</param>
        /// <returns>HTTP response without content</returns>
        public IResult HideActor(string actor)
        {
            if (!Movie.AllActors.Contains(actor))
                return Results.NoContent();
            User user = GetCurrentUser();
            user.AddActorToBlackList(actor);
            _context.Update(user);
            _context.SaveChanges();
            UserActCache.AddAct(user.Id.ToString(), "PropertyBlock", _context);
            user.LogBlock("actor", actor);
            return Results.NoContent();
        }

        /// <summary>
        /// Add genre to blocked.
        /// </summary>
        /// <param name="genre">Genre to be blocked</param>
        /// <returns>HTTP response without content</returns>
        public IResult HideGenre(string genre)
        {
            if (!Movie.AllGenres.Contains(genre))
                return Results.NoContent();
            User user = GetCurrentUser();
            user.AddGenreToBlackList(genre);
            _context.Update(user);
            _context.SaveChanges();
            UserActCache.AddAct(user.Id.ToString(), "PropertyBlock", _context);
            user.LogBlock("genre", genre);
            return Results.NoContent();
        }

        /// <summary>
        /// Remove director from blocked.
        /// </summary>
        /// <param name="director">Director to be unblocked</param>
        /// <returns>HTTP response without content</returns>
        public IResult ShowDirector(string director)
        {
            User user = GetCurrentUser();
            user.RemoveDirectorFromBlackList(director);
            _context.Update(user);
            _context.SaveChanges();
            user.LogUnblock("director", director);
            return Results.NoContent();
        }

        /// <summary>
        /// Remove actor from blocked.
        /// </summary>
        /// <param name="actor">Actor to be unblocked</param>
        /// <returns>HTTP response without content</returns>
        public IResult ShowActor(string actor)
        {
            User user = GetCurrentUser();
            user.RemoveActorFromBlackList(actor);
            _context.Update(user);
            _context.SaveChanges();
            user.LogUnblock("actor", actor);
            return Results.NoContent();
        }

        /// <summary>
        /// Remove genre from blocked.
        /// </summary>
        /// <param name="genre">Genre to be unblocked</param>
        /// <returns>HTTP response without content</returns>
        public IResult ShowGenre(string genre)
        {
            User user = GetCurrentUser();
            user.RemoveGenreFromBlackList(genre);
            _context.Update(user);
            _context.SaveChanges();
            user.LogUnblock("genre", genre);
            return Results.NoContent();
        }


    }
}
