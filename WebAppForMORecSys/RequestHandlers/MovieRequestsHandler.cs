using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Web.Razor.Parser.SyntaxTree;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.RequestHandlers
{
    /// <summary>
    /// Class that handles request of movies pages
    /// </summary>
    public class MovieRequestsHandler
    {
        /// <summary>
        /// Database context
        /// </summary>
        private ApplicationDbContext _context;

        /// <summary>
        /// IDs of all movies in database sorted by number od rating
        /// </summary>
        private List<int> movieIDsSortedByRatings = new List<int>();

        /// <summary>
        /// Constructor. Sets connection to db. Prepares list movieIDsSortedByRatings
        /// </summary>
        /// <param name="context">Database context</param>
        public MovieRequestsHandler(ApplicationDbContext context)
        {
            _context = context;
            movieIDsSortedByRatings = context.Items.Include(i => i.Ratings).OrderByDescending(i => i.Ratings.Count)
                .Select(i => i.Id).ToList();
        }


        /// <summary>
        /// Handles the request of main movies page
        /// </summary>
        /// <param name="user">User that requests the page</param>
        /// <param name="search">Main user search on title</param>
        /// <param name="director">Movie filter user search on director</param>
        /// <param name="actor">Movie filter user search on actor></param>
        /// <param name="genres">Movie filter user search on genres</param>
        /// <param name="typeOfSearch">Specifies if user clicked the search for main search or in movie filter</param>
        /// <param name="releasedateto">Movie filter user search on the latest release date</param>
        /// <param name="releasedatefrom">Movie filter user search on the earliest release date</param>
        /// <param name="metricsimportance">Metrics importance in % according to user input</param>
        /// <returns>ViewModel for the main page</returns>
        public async Task<MainViewModel> ProcessMainQuery(User user, string search, string director,
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom,
          string[] metricsimportance)
        {
            var numberOfShownItems = 15;
            var viewModel = new MainViewModel();
            if (user != null)
            {
                viewModel.User = user;
                viewModel.UserRatings = await (from rating in _context.Ratings
                                                      where rating.UserID == viewModel.User.Id
                                                      select rating).ToListAsync();
               
            }
            RecommenderSystem rs = SystemParameters.RecommenderSystem;
            List<Metric> metrics = await _context.Metrics.Include(m => m.metricVariants)
                .Where(m => m.RecommenderSystemID == rs.Id).ToListAsync();
            viewModel.SetMetricImportance(user, metrics, metricsimportance, _context);

            viewModel.UsedVariants = user.GetMetricVariants(_context, metrics.Select(m => m.Id).ToList());
            viewModel.SearchValue = search ?? "";
            viewModel.FilterValues.Add("Director", director);
            viewModel.FilterValues.Add("Actor", actor);
            viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
            viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
            viewModel.FilterValues.Add("Genres", string.Join(',', genres));

            IQueryable<Item> whitelist = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, typeOfSearch, releasedateto, releasedatefrom);
            int[] whitelistIDs = whitelist == null ? new int[0] : await whitelist.Select(item => item.Id).ToArrayAsync();
            if((whitelistIDs.Length>0 && whitelistIDs.Length < 20))
            {
                viewModel.Items = whitelist;
                return viewModel;
            }
            if (viewModel.UserRatings.Where(r => r.RatingScore > 5).Count() < 10)
            {
                var possibleItems = whitelistIDs.Length > 0 ? whitelist
                                        : _context.Items.Where(i => movieIDsSortedByRatings.Take(250).Contains(i.Id));
                viewModel.Items = possibleItems.OrderBy(x => Guid.NewGuid()).Take(15);
                viewModel.Info = "Please rate positively atleast 10 movies you like so the recommender system can work.";
                return viewModel;
            }
            List<int> blacklist = BlockedItemsCache.GetBlockedItemIdsForUser(user.Id.ToString(), _context);
            blacklist = blacklist.Union(user.GetRatedAndSeenItems(_context)).ToList();
            var recommendations = await RecommenderCaller.GetRecommendations(whitelistIDs, blacklist.ToArray(),
                viewModel.Metrics.Values.ToArray(), user.Id, rs.HTTPUri,
                user.GetMetricVariantCodes(_context, metrics.Select(m => m.Id).ToList()));
            if (recommendations.Count > 0)
            {
                viewModel.Items = _context.Items.Where(item => recommendations.Keys.Contains(item.Id));
                viewModel.ItemsToMetricContributionScore = recommendations.Values.ToArray();
                if (whitelist.IsNullOrEmpty() && (!search.IsNullOrEmpty() || !actor.IsNullOrEmpty() || !director.IsNullOrEmpty() || !genres.IsNullOrEmpty()
                        || !releasedatefrom.IsNullOrEmpty() || !releasedateto.IsNullOrEmpty()))
                {
                    viewModel.Message = "<script>alert('There are no results for your search.\\n\\nTry to make simpler search.');</script>";
                }
            }
            else
            {
                if (!search.IsNullOrEmpty() || !actor.IsNullOrEmpty() || !director.IsNullOrEmpty() || !genres.IsNullOrEmpty()
                        || !releasedatefrom.IsNullOrEmpty() || !releasedateto.IsNullOrEmpty())
                {
                    viewModel.Message = "<script>alert('There are no results for your search.\\n\\nTry to make simpler search or check your block rules.');</script>";
                }
                viewModel.Items = _context.Items.Where(item => !blacklist.Contains(item.Id)).Take(numberOfShownItems);
            }
            return viewModel;
        }

        /// <summary>
        /// Handles the request of block management page on movies
        /// </summary>
        /// <param name="user">User that requests the page</param>
        /// <param name="search">Search in blocked items. (Possible for user only if he blocks a lot of movies)</param>
        /// <param name="block">New blocked value in format 'name_of_property: value'. Used with single block rule creation.</param>
        /// <param name="director">New blocked value for property director. Used with multiple block rule creation.</param>
        /// <param name="actor">New blocked value for property actor. Used with multiple block rule creation.</param>
        /// <param name="genres">New blocked value for property genres. Used with multiple block rule creation.</param>
        /// <param name="method">HTTP method that was used</param>
        /// <returns>View model for block management page</returns>
        public async Task<UserBlockRuleViewModel> ProcessBlockSettings(User user, string search, string block, 
            string director, string actor, string[] genres, string method)
        {
            string message = "";
            if (method == "POST")
            {
                AddBlockRule(user, block, director, actor, genres, out message);
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
                CurrentUserRatings = await(from rating in _context.Ratings
                                           where rating.UserID == user.Id
                                           select rating).ToListAsync(),
                Message=message

            };
            viewModel.StringPropertiesBlocks.Add("Genres", user.GetGenresInBlackList());
            viewModel.StringPropertiesBlocks.Add("Directors", user.GetDirectorsInBlackList());
            viewModel.StringPropertiesBlocks.Add("Actors", user.GetActorsInBlackList());
            return viewModel;
        }


        /// <summary>
        /// Adding new block rule. Choose method of addition that should be used.
        /// </summary>
        /// <param name="user">Currently logged user</param>
        /// <param name="block">New blocked value in format 'name_of_property: value'. Used with single block rule creation.</param>
        /// <param name="director">New blocked value for property director. Used with multiple block rule creation.</param>
        /// <param name="actor">New blocked value for property actor. Used with multiple block rule creation.</param>
        /// <param name="genres">New blocked value for property genres. Used with multiple block rule creation.</param>
        private void AddBlockRule(User user, string block, string director, string actor, string[] genres, out string message)
        {
            if (block.IsNullOrEmpty())
                message = user.AddMultipleBlockRules(director, actor, genres);
            else
                message = user.AddSingleBlockRule(block);
            _context.Update(user);
            _context.SaveChanges();
        }
    }
}
