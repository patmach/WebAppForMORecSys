using Elfie.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using NuGet.Protocol.Plugins;
using System.Globalization;
using System.IO;
using System.Text;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.Settings;
using static WebAppForMORecSys.Helpers.MovieHelper;

namespace WebAppForMORecSys.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Account> _userManager;
        static HttpClient client = new HttpClient();


        public MoviesController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
            SetAllGenres();
            SetAllDirectors();
            SetAllActors();
            SystemParameters.RecommenderSystem = context.RecommenderSystems.Where(rs => rs.Name == "SimpleMF").First();
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
            var metrics = await (_context.Metrics.Where(m=>m.RecommenderSystemID == rs.Id).ToListAsync());
            /*metrics = new List<Metric> { new Metric { Name = "Relevance" },
                new Metric { Name = "Novelty" }, new Metric { Name = "Diversity" } }*/;//DELETE Later
            int numberOfParts = 0;
            for (int i = 0; i < metrics.Count(); i++)
            {
                numberOfParts += i + 1;
            }
            if (!metricsimportance.IsNullOrEmpty())
            {
                for (int i = 0; i < metrics.Count(); i++)
                {
                    viewModel.Metrics.Add(metrics[i], (int)double.Parse(metricsimportance[i], CultureInfo.InvariantCulture));
                }
            }
            else
            {
                for (int i = 0; i < metrics.Count; i++)
                {
                    if (Settings.SystemParameters.MetricsView == Settings.MetricsView.DragAndDrop)
                        viewModel.Metrics.Add(metrics[i], ((int)(100.0 / numberOfParts * (metrics.Count - i))));
                    else
                        viewModel.Metrics.Add(metrics[i], 100 / metrics.Count());
                }
            }

            viewModel.SearchValue = search ?? "";            
            var possibleItems = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, type, releasedateto, releasedatefrom);
            viewModel.FilterValues.Add("Director", director);
            viewModel.FilterValues.Add("Actor", actor);
            viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
            viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
            viewModel.FilterValues.Add("Genres", string.Join(',', genres));            
            RecommenderQuery rq = new RecommenderQuery
            {
                PossibleItems = await possibleItems.Select(item => item.Id).ToArrayAsync(),
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
                viewModel.Items = possibleItems.Take(50);//Nahradit voláním RS
            return View(viewModel);
        }

        public async Task<IActionResult> UserBlockSettings(string search, string director,string actor, 
            string[] genres)
        {
            User user = GetCurrentUser();
            string method = HttpContext.Request.Method;
            if (method == "POST")
            {
                string message = AddBlockRules(user, director, actor, genres);
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
            var viewModel = new UserBlockRule
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

        private string AddBlockRules(User user, string director, string actor, string[] genres)
        {
            var message = new StringBuilder();
            if (!actor.IsNullOrEmpty())
            {
                if (!Movie.AllActors.Contains(actor))
                    message.Append("Block rule for actor " + actor + " was not added. Because there is no actor of that exact name in the database.\\n");
                else
                    HideActor(actor);
            }
            if (!director.IsNullOrEmpty())
            {
                if (!Movie.AllDirectors.Contains(director))
                    message.Append("Block rule for director " + director + " was not added. Because there is no director of that exact name in the database.\\n");
                else
                    HideDirector(director);
            }
            foreach (var genre in genres) 
            {
                if (!Movie.AllGenres.Contains(genre))
                    message.Append("Block rule for genre " + genre + " was not added. Because there is no genre of that exact name in the database.\\n");
                else
                    HideGenre(genre);
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
                new Movie(_context.Items.First(x => x.Id == id)),
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
            return PartialView(new PreviewDetailViewModel(
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
                _context.Items.ToList().ForEach(m => genres.AddRange(MovieHelper.GetGenres(m)));
                Movie.AllGenres = genres.Distinct().ToList();
                Movie.AllGenres.Remove("(nogenreslisted)");
            }
        }

        public void SetAllDirectors()
        {
            if (Movie.AllDirectors == null)
            {
                var directors = new List<string>();
                _context.Items.ToList().ForEach(m => directors.Add(MovieHelper.GetDirector(m)));
                Movie.AllDirectors = directors.Distinct().ToList();
            }
        }

        public void SetAllActors()
        {
            if (Movie.AllActors == null)
            {
                var actors = new List<string>();
                _context.Items.ToList().ForEach(m => actors.AddRange(MovieHelper.GetActors(m)));
                Movie.AllActors = actors.Distinct().ToList();
            }
        }


        public List<string> GetAllMovieNames(string prefix)
        {
            return _context.Items.Where(m => EF.Functions.Like(m.Name, $"%{prefix}%"))
                .Select(m => m.Name).Take(15).ToList();

        }
        public List<string> GetAllDirectors(string prefix)
        {
            return Movie.AllDirectors.Where(d => d.Contains(prefix, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();

        }
        public List<string> GetAllActors(string prefix)
        {
            return Movie.AllActors.Where(a => a.Contains(prefix, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();

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
            if (_context.Items.Where(m=>m.Id==id).Count()==0)
                return Results.NoContent();
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
