using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.HomeViewsModels;

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
            SetAllGenres();
            allItems = from item in _context.Items/*.Include(i => i.Ratings)*/
                           select item;
        }

        public async Task<IActionResult> Index(string search, string[] metricimportance, string director,
          string actor, string[] genres, string type, string releasedateto, string releasedatefrom)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'Context.Items'  is null.");
            }
            var viewModel = new MainViewModel();
            var account = await _userManager.GetUserAsync(User);
            if (account != null)
            {
                viewModel.CurrentUser = _context.Users.Where(u => u.UserName == account.UserName).FirstOrDefault();
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


           
            if (type == "Search" && search != null)
            {
                allItems = allItems.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }
            if (type == "MovieFilter")
            {

                var itemslist = allItems.ToList();
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
                allItems = itemslist.AsQueryable();

                viewModel.FilterValues.Add("Director", director);
                viewModel.FilterValues.Add("Actor", actor);
                viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
                viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
                viewModel.FilterValues.Add("Genres", string.Join(',', genres));


            }
            viewModel.Items = allItems.Take(50).ToList();//Nahradit voláním RS
            return View(viewModel);
        }

        public ActionResult Details(int id)
        {
            //get model from database
            return PartialView(new Movie(_context.Items.First(x => x.Id == id)));
        }

        public void SetAllGenres()
        {            
            if (Movie.AllGenres == null)
            {
                var genres = new List<string>();
                var movies = _context.Items.ToList();
                movies.ForEach(m => genres.AddRange(MovieHelper.GetGenres(m)));
                Movie.AllGenres = genres.Distinct().ToList();
                Movie.AllGenres.Remove("(nogenreslisted)");
            }
        }


        public IResult Rate(int id, int score)
        {
            var account = _userManager.GetUserAsync(User).Result;
            User user = null;
            if (account != null)
            {
                user = _context.Users.Where(u => u.UserName == account.UserName).FirstOrDefault();
            }
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
    }
}
