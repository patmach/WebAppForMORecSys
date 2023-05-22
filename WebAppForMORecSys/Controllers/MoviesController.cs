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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Account> _userManager;
        


        public MoviesController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
            Movie.SetAllGenres(context);
            Movie.SetAllDirectors(context);
            Movie.SetAllActors(context);
            
        }

        public async Task<IActionResult> Index(string search, string director,
          string actor, string[] genres, string type, string releasedateto, string releasedatefrom, string[] metricsimportance)
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

            IQueryable<Item> whitelist = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, type, releasedateto, releasedatefrom);
            int[] whitelistIDs = (whitelist == null) ? new int[0] : await whitelist.Select(item => item.Id).ToArrayAsync();
            List<int> blacklist = BlockedItemsCache.GetBlockedItemIdsForUser(user.Id.ToString(),_context);
            var recommendations = await RecommenderCaller.GetRecommendations(whitelistIDs, blacklist.ToArray(), 
                        viewModel.Metrics.Values.ToArray(), user.Id, rs.HTTPUri);
            if (recommendations.Count > 0)
            {
                viewModel.Items = _context.Items.Where(item => recommendations.Keys.Contains(item.Id));
                viewModel.ItemsToMetricImportance = recommendations.Values.ToArray();
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

        public List<string> GetAllPossibleValuesToBlock(string prefix)
        {
            List<string> possibleBlocks =  new List<string>();
            possibleBlocks.AddRange(GetAllGenres(prefix).Take(5).Select(g=> $"Genre: {g}"));
            possibleBlocks.AddRange(GetAllDirectors(prefix).Take(5).Select(d => $"Director: {d}"));
            possibleBlocks.AddRange(GetAllActors(prefix).Take(5).Select(a => $"Actor: {a}"));
            return possibleBlocks;
        }


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
