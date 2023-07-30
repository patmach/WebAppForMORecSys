using Elfie.Serialization;
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
using static WebAppForMORecSys.Helpers.MovieHelper;
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
            //new MovielensLoader(filter: true).LoadMovielensData(context/*, moviesFromLoadedFile: true*/);
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
        /// <returns>View with recommended movies</returns>
        public async Task<IActionResult> Index(string search, string director,
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom, 
          string[] metricsimportance)
        {
            User user = GetCurrentUser();
            var viewModel = _requestsHandler.ProcessMainQuery(user,search, director, actor, genres, typeOfSearch,
                releasedateto, releasedatefrom, metricsimportance).Result;
            if (!viewModel.Message.IsNullOrEmpty())
                TempData["msg"] = viewModel.Message;
            return View(viewModel);
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
            List<Rating> ratings = null;
            if (user != null)
            {
                user = user;
                ratings = await (from rating in _context.Ratings
                                 where rating.UserID == user.Id
                                 select rating).ToListAsync();
            }
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
            List<Rating> ratings = null;
            if (user != null)
            {
                user =  user;
                ratings = await(from rating in _context.Ratings
                                                     where rating.UserID == user.Id
                                                     select rating).ToListAsync();
            }
            Interaction.Save(id, user.Id, TypeOfInteraction.Click, _context);
            return PartialView(new PreviewDetailViewModel(
                _context.Items.First(x => x.Id == id),
                user,
                ratings
                ));

        }


        /// <summary>
        /// </summary>
        /// <param name="prefix">Searched value</param>
        /// <returns>All movie names that contains searched value</returns>
        public List<string> GetAllMovieNames(string prefix)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return null;
            }
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
            if (user == null)
            {
                return null;
            }
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
            if (user == null)
            {
                return null;
            }
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
            if (user == null)
            {
                return null;
            }
            return Movie.AllGenres.Where(g => g.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                .Except(user.GetGenresInBlackList()).Take(10).ToList();

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
        /// Saves new user rating for a movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <param name="score">Rating score</param>
        /// <returns>HTTP response without content</returns>
        public IResult Rate(int id, byte score)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            Rating.Save(id, user.Id, score, _context);
            return Results.NoContent();
        }

        /// <summary>
        /// Deletes user rating of a movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <returns>HTTP response without content</returns>
        public IResult Unrate(int id)
        {
            User user = GetCurrentUser();
            if (user == null)
            {
                return Results.Unauthorized();
            }
            Rating.Remove(id, user.Id, _context);
            return Results.NoContent();
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
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddDirectorToBlackList(director);
            _context.Update(user);
            _context.SaveChanges();
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
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddActorToBlackList(actor);
            _context.Update(user);
            _context.SaveChanges();
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
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.AddGenreToBlackList(genre);
            _context.Update(user);
            _context.SaveChanges();
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
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveDirectorFromBlackList(director);
            _context.Update(user);
            _context.SaveChanges();
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
            if (user == null)
            {
                return Results.Unauthorized();
            }
            user.RemoveActorFromBlackList(actor);
            _context.Update(user);
            _context.SaveChanges();
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
