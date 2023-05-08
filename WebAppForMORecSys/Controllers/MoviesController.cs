using Elfie.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using NuGet.Protocol;
using NuGet.Protocol.Plugins;
using System.Globalization;
using System.IO;
using System.Text;
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
        static HttpClient client = new HttpClient();


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
            var viewModel = new MainViewModel();
            User user = GetCurrentUser();
            if (user != null)
            {
                viewModel.CurrentUser = user;
                viewModel.CurrentUserRatings = await (from rating in _context.Ratings
                                                      where rating.UserID == viewModel.CurrentUser.Id
                                                      select rating).ToListAsync();
            }

            var rs = SystemParameters.RecommenderSystem;
            var metrics = await (_context.Metrics.Where(m => m.RecommenderSystemID == rs.Id).ToListAsync());
            int numberOfParts = 0;
            for (int i = 0; i < metrics.Count(); i++)
            {
                numberOfParts += i + 1;
            }
            metricsimportance = metricsimportance.IsNullOrEmpty() ? user.GetMetricsImportance() : metricsimportance; 
            if (metricsimportance.IsNullOrEmpty())
            {
                metricsimportance = new string[metrics.Count];
                for (int i = 0; i < metrics.Count(); i++)
                {
                    if (user.GetMetricsView() == MetricsView.DragAndDrop)
                        metricsimportance[i] = ((int)(100.0 / numberOfParts * (metrics.Count - i))).ToString();
                    else
                        metricsimportance[i] =  (100 / metrics.Count()).ToString();
                }
            }
            else
            {
                user.SetMetricsImportance(metricsimportance);
                _context.Update(user);
                _context.SaveChanges();
            }
            for (int i = 0; i < metrics.Count(); i++)
            {
                viewModel.Metrics.Add(metrics[i], (int)double.Parse(metricsimportance[i], CultureInfo.InvariantCulture));
            }

            viewModel.SearchValue = search ?? "";            
            var whitelist = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, type, releasedateto, releasedatefrom);
            var blacklist = BlockedItemsCache.GetBlockedItemIdsForUser(user.Id.ToString(),_context);
            viewModel.FilterValues.Add("Director", director);
            viewModel.FilterValues.Add("Actor", actor);
            viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
            viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
            viewModel.FilterValues.Add("Genres", string.Join(',', genres));            
            RecommenderQuery rq = new RecommenderQuery
            {
                WhiteListItemIDs = (whitelist==null) ? new int[0] :await whitelist.Select(item => item.Id).ToArrayAsync(),
                BlackListItemIDs = blacklist.ToArray(),
                Metrics = metricsimportance.Select(m => (int)double.Parse(m, CultureInfo.InvariantCulture)).ToArray(),
                Count = 50
            };
            JsonContent content = JsonContent.Create(rq);
            HttpResponseMessage response = await client.PostAsync($"{rs.HTTPUri}getRecommendations/{user.Id}", content);
            Dictionary<int, int[]> recommendations = new Dictionary<int, int[]>();
            if (response.IsSuccessStatusCode)
            {
                recommendations = await response.Content.ReadFromJsonAsync<Dictionary<int, int[]>>();
            }
            if (recommendations.Count > 0)
            {
                viewModel.Items = _context.Items.Where(item=> recommendations.Keys.Contains(item.Id));
                viewModel.ItemsToMetricImportance = recommendations.Values.ToArray();
            }
            else
                viewModel.Items = _context.Items.Take(50);//Nahradit voláním RS
            return View(viewModel);
        }

        public async Task<IActionResult> UserBlockSettings(string search, string block, string director,string actor, 
            string[] genres)
        {
            User user = GetCurrentUser();
            string method = HttpContext.Request.Method;
            if (method == "POST")
            {
                string message = "";
                if (block.IsNullOrEmpty())
                    message = AddMultipleBlockRules(user, director, actor, genres);
                else
                    message = AddSingleBlockRule(user, block);
                if (!message.IsNullOrEmpty())
                {
                    TempData["msg"] = "<script>alert('"+message+"');</script>";
                }
            }
            var itemIDs = user.GetItemsInBlackList();
            var items = _context.Items.Where(item => itemIDs.Contains(item.Id));
            if (!search.IsNullOrEmpty())
            {
                items = items.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }
            var viewModel = new UserBlockRuleViewModel
            {
                Items = _context.Items.Where(item => itemIDs.Contains(item.Id)).ToList(),
                SearchValue = search ?? "",
                CurrentUser = user,
                CurrentUserRatings = await (from rating in _context.Ratings
                                            where rating.UserID == user.Id
                                            select rating).ToListAsync()

            };       
            viewModel.StringPropertiesBlocks.Add("Genres", user.GetGenresInBlackList());
            viewModel.StringPropertiesBlocks.Add("Directors", user.GetDirectorsInBlackList());
            viewModel.StringPropertiesBlocks.Add("Actors", user.GetActorsInBlackList());

            return View(viewModel);
        }

        private string AddSingleBlockRule(User user, string block)
        {
            var blockedValue = block.Split(':');
            if(blockedValue.Length == 2) {
                switch(blockedValue[0].ToLower().Trim())
                {
                    case "actor":
                        return AddMultipleBlockRules(user, actor: blockedValue[1].Trim());
                    case "director":
                        return AddMultipleBlockRules(user, director: blockedValue[1].Trim());
                    case "genre":
                        return AddMultipleBlockRules(user, genres: new string[] { blockedValue[1].Trim() });
                    default:
                        return $"There is no property \"{blockedValue[0]}\" which values can be blocked!";
                }
            }
            else if (blockedValue.Length == 1) {
                string message = AddMultipleBlockRules(user, director: blockedValue[0].Trim()); 
                if(!message.IsNullOrEmpty())
                    message = AddMultipleBlockRules(user, actor: blockedValue[0].Trim());
                if (!message.IsNullOrEmpty())
                    message = AddMultipleBlockRules(user, genres: new string[] { blockedValue[0].Trim() });
                if (message.IsNullOrEmpty())
                    return $"No property contains a value \"{blockedValue[0]}\" that can be blocked!";

            }
            else
            {
                return $"Please specify blocked value in format \"property:value\" or \"value\".";
            }
            return "";

        }

        private string AddMultipleBlockRules(User user, string director=null, string actor=null, string[] genres= null)
        {
            var message = new StringBuilder();
            if (!actor.IsNullOrEmpty())
            {
                if (!Movie.AllActors.Contains(actor))
                    message.Append($"Block rule for actor \"{actor}\" was not added. Because there is no actor of that exact name in the database.\\n");
                else
                    HideActor(actor);
            }
            if (!director.IsNullOrEmpty())
            {
                if (!Movie.AllDirectors.Contains(director))
                    message.Append($"Block rule for director \"{director}\" was not added. Because there is no director of that exact name in the database.\\n");
                else
                    HideDirector(director);
            }
            if (!genres.IsNullOrEmpty())
            {
                foreach (var genre in genres)
                {
                    if (!Movie.AllGenres.Contains(genre))
                        message.Append($"Block rule for genre \"{genre}\" was not added. Because there is no genre of that exact name in the database.\\n");
                    else
                        HideGenre(genre);
                }
            }
            
            return message.ToString();
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
