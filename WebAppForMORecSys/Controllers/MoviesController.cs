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


        /// <summary>
        /// Gets connection to db and UserManager, saves possible values of movie properties
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        public MoviesController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
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
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom, string[] metricsimportance)
        {
            User user = GetCurrentUser();
            var viewModel = new MainViewModel();
            if (user != null)
            {
                viewModel.CurrentUser = user;
                viewModel.CurrentUserRatings = await (from rating in _context.Ratings
                                                      where rating.UserID == viewModel.CurrentUser.Id
                                                      select rating).ToListAsync();
            }
            RecommenderSystem rs = SystemParameters.RecommenderSystem;
            List<Metric> metrics = await (_context.Metrics.Where(m => m.RecommenderSystemID == rs.Id).ToListAsync());
            viewModel.SetMetricImportance(user, metrics, metricsimportance, _context);

            viewModel.SearchValue = search ?? "";

            viewModel.FilterValues.Add("Director", director);
            viewModel.FilterValues.Add("Actor", actor);
            viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
            viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
            viewModel.FilterValues.Add("Genres", string.Join(',', genres));

            IQueryable<Item> whitelist = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, typeOfSearch, releasedateto, releasedatefrom);
            int[] whitelistIDs = (whitelist == null) ? new int[0] : await whitelist.Select(item => item.Id).ToArrayAsync();
            List<int> blacklist = BlockedItemsCache.GetBlockedItemIdsForUser(user.Id.ToString(),_context);
            blacklist=blacklist.Union(user.GetRatedAndSeenItems(_context)).ToList();
            var recommendations = await RecommenderCaller.GetRecommendations(whitelistIDs, blacklist.ToArray(), 
                        viewModel.Metrics.Values.ToArray(), user.Id, rs.HTTPUri);
            if (recommendations.Count > 0)
            {
                viewModel.Items = _context.Items.Where(item => recommendations.Keys.Contains(item.Id));
                viewModel.ItemsToMetricContributionScore = recommendations.Values.ToArray();
                if (whitelist.IsNullOrEmpty() && (!search.IsNullOrEmpty() || !actor.IsNullOrEmpty() || !director.IsNullOrEmpty() || !genres.IsNullOrEmpty()
                        || !releasedatefrom.IsNullOrEmpty() || !releasedateto.IsNullOrEmpty()))
                {
                    TempData["msg"] = "<script>alert('There are no results for your search.\\n\\nTry to make simpler search.');</script>";
                }
            }
            else
            {
                if (!search.IsNullOrEmpty() || !actor.IsNullOrEmpty() || !director.IsNullOrEmpty() || !genres.IsNullOrEmpty()
                        || !releasedatefrom.IsNullOrEmpty() || !releasedateto.IsNullOrEmpty())
                {
                    TempData["msg"] = "<script>alert('There are no results for your search.\\n\\nTry to make simpler search or check your block rules.');</script>";
                }
                viewModel.Items = _context.Items.Where(item => !blacklist.Contains(item.Id)).Take(50);
            }
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
            if (method == "POST")
            {
                AddBlockRule(user, block, director, actor, genres);
            }
            var BlackListItemIDs = user.GetItemsInBlackList();
            var items = _context.Items.Where(item => BlackListItemIDs.Contains(item.Id));
            if (!search.IsNullOrEmpty())
            {
                items = items.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }
            var viewModel = new UserBlockRuleViewModel
            {
                Items = items.ToList(),
                SearchValue = search ?? "",
                CurrentUser = user,
                CurrentUserRatings = await (from rating in _context.Ratings
                                            where rating.UserID == user.Id
                                            select rating).ToListAsync()

            };       
            viewModel.StringPropertiesBlocks.Add("Genres", user.GetGenresInBlackList());
            viewModel.StringPropertiesBlocks.Add("Directors", user.GetDirectorsInBlackList());
            viewModel.StringPropertiesBlocks.Add("Actors", user.GetActorsInBlackList());
            try
            {
                return View(viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Adding new block rule. Choose method of addition that should be used.
        /// </summary>
        /// <param name="user">Currently logged user</param>
        /// <param name="block">New blocked value in format 'name_of_property: value'. Used with single block rule creation.</param>
        /// <param name="director">New blocked value for property director. Used with multiple block rule creation.</param>
        /// <param name="actor">New blocked value for property actor. Used with multiple block rule creation.</param>
        /// <param name="genres">New blocked value for property genres. Used with multiple block rule creation.</param>
        private void AddBlockRule(User user, string block, string director, string actor, string[] genres)
        {
            string message = "";
            if (block.IsNullOrEmpty())
                message = user.AddMultipleBlockRules(director, actor, genres);
            else
                message = user.AddSingleBlockRule(block);
            if (!message.IsNullOrEmpty())
            {
                TempData["msg"] = "<script>alert('" + message + "');</script>";
            }
            _context.Update(user);
            _context.SaveChanges();
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
