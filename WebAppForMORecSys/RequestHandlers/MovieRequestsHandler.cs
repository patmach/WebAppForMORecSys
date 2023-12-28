using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Web.Razor.Parser.SyntaxTree;
using System.Xml.Linq;
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
        /// IDs of all movies in database sorted by number of ratings
        /// </summary>
        private static List<int> movieIDsSortedByRatings = new List<int>();

        /// <summary>
        /// Constructor. Sets connection to db. Prepares list movieIDsSortedByRatings
        /// </summary>
        /// <param name="context">Database context</param>
        public MovieRequestsHandler(ApplicationDbContext context)
        {
            _context = context;
            if (movieIDsSortedByRatings.Count == 0)
            {
                movieIDsSortedByRatings = context.Ratings.GroupBy(r => r.ItemID).OrderByDescending(g => g.Count())
                    .Select(g => g.Key).ToList();
            }
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
        /// <param name="currentList">IDs of recommended movies displayed on the page</param>
        /// <returns>ViewModel for the main page</returns>
        public async Task<MainViewModel> ProcessMainQuery(User user, string search, string director,
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom,
          string[] metricsimportance, int[]? currentList = null)
        {
            if (currentList == null)
                currentList = new int[0];
            var viewModel = new MainViewModel();
            if (user != null)
            {
                viewModel.User = user;
                viewModel.UserRatings = await (from rating in _context.Ratings
                                                      where rating.UserID == viewModel.User.Id
                                                      select rating).ToListAsync();
               
            }
            RecommenderSystem rs = SystemParameters.GetRecommenderSystem(_context);
            List<Metric> metrics = await _context.Metrics.Include(m => m.MetricVariants)
                .Where(m => m.RecommenderSystemID == rs.Id).ToListAsync();
            SetMainViewModel(viewModel, user, metrics, metricsimportance, search, typeOfSearch, director, actor,
                releasedatefrom, releasedateto, genres);
            IQueryable<Item> whitelist = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, typeOfSearch, releasedateto, releasedatefrom);
            List<int> whitelistIDs = whitelist == null ? new List<int>() : await whitelist.Select(item => item.Id).ToListAsync();
            var positivelyRatedCount = viewModel.UserRatings.Where(r => r.RatingScore > 5).Count();
            if (positivelyRatedCount < SystemParameters.MinimalPositiveRatings)  
                viewModel.Info = $"Please rate positively atleast another {SystemParameters.MinimalPositiveRatings - positivelyRatedCount} movies you like so the recommender system can work.\n" +
                    "More ratings make recommmendations better.\n" +
                    $"You can use search or filter for finding your favourite movies.\n\n" +
                    $"The database contains selected movies from 1990-2019.";
            if (ReturnSearched(viewModel, whitelistIDs, currentList, whitelist, out viewModel))
                return viewModel;    
            if (positivelyRatedCount < SystemParameters.MinimalPositiveRatings)
            {
                return ReturnNonPersonalized(viewModel, whitelistIDs, whitelist, currentList.ToList(), positivelyRatedCount);
            }
            if (user.FirstRecommendationTime == null) {
                user.FirstRecommendationTime = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            var mvCodes = viewModel.User.GetMetricVariantCodes(_context, metrics.Select(m => m.Id).ToList()).ToList();
            UserActCache.AddActs(viewModel.User.Id.ToString(), mvCodes, _context);
            viewModel.Log(mvCodes);
            return await ReturnPersonalized(viewModel, whitelistIDs, whitelist, rs, metrics, currentList, search,
                actor, director, releasedatefrom, releasedateto, genres);
        }

        /// <summary>
        /// Returns personalized recommendations from recommender system
        /// </summary>
        /// <param name="viewModel">View model of main page</param>
        /// <param name="whitelistIDs">IDs of searched items (Empty if nothing is searched)</param>
        /// <param name="whitelist">searched items (Empty if nothing is searched)</param>
        /// <param name="rs">Recommender system instance</param>
        /// <param name="metrics">List of used metrics</param>
        /// <param name="currentList">Items already displayed on the page</param>
        /// <param name="search">searched value (full-text search on name of the item)</param>
        /// <param name="actor">Movie filter user search on actor></param>
        /// <param name="director">Movie filter user search on director</param>
        /// <param name="releasedatefrom">Movie filter user search on the earliest release date</param>
        /// <param name="releasedateto">Movie filter user search on the latest release date</param>
        /// <param name="genres">Movie filter user search on genres</param>
        /// <returns>View model of main page with loaded items that will be displayed to user</returns>
        private async Task<MainViewModel> ReturnPersonalized(MainViewModel viewModel, List<int> whitelistIDs, 
            IQueryable<Item> whitelist, RecommenderSystem rs, List<Metric> metrics, int[] currentList,
            string search, string actor, string director, string releasedatefrom, string releasedateto, string[] genres)
        {
            List<int> blacklist = BlockedItemsCache.GetBlockedItemIdsForUser(viewModel.User.Id.ToString(), _context);
            blacklist = blacklist.Union(viewModel.User.GetRatedAndSeenItems(_context)).Union(currentList).ToList();
            var recommendations = await RecommenderCaller.GetRecommendations(whitelistIDs.ToArray(), blacklist.ToArray(),
                viewModel.Metrics.Values.ToArray(), viewModel.User.Id, rs.HTTPUri,
                viewModel.User.GetMetricVariantCodes(_context, metrics.Select(m => m.Id).ToList()),
                currentList);
            if (recommendations.Count > 0)
            {
                viewModel.Items = _context.Items.Where(item => recommendations.Keys.Contains(item.Id)).ToList()
                    .OrderBy(item => recommendations.Keys.ToList().IndexOf(item.Id)).ToList();
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
                viewModel.Items = await _context.Items.Where(item => !blacklist.Contains(item.Id))
                    .Take(SystemParameters.LengthOfRecommendationsList).ToListAsync();
            }

            return viewModel;
        }

        /// <summary>
        /// Returns random items from the most known ones
        /// </summary>
        /// <param name="viewModel">View model of main page</param>
        /// <param name="whitelistIDs">IDs of searched items (Empty if nothing is searched)</param>
        /// <param name="whitelist">searched items (Empty if nothing is searched)</param>
        /// <param name="blacklist">Items that can't be recommended</param>
        /// <param name="positivelyRatedCount">Number of positively rated items by user</param>
        /// <returns>View model of main page with loaded items that will be displayed to user</returns>
        private MainViewModel ReturnNonPersonalized(MainViewModel viewModel, List<int> whitelistIDs, 
            IQueryable<Item> whitelist, List<int> blacklist, int positivelyRatedCount)
        {
            var possibleItems = whitelistIDs.Count > 0 ? whitelist
                                        : _context.Items.Where(i => movieIDsSortedByRatings.Take(500).Contains(i.Id));
            possibleItems = possibleItems.Where(i => !blacklist.Contains(i.Id));
            viewModel.Items = possibleItems.OrderBy(x => Guid.NewGuid()).Take(15).ToList();            
            return viewModel;
        }

        /// <summary>
        /// Returns items corresponding to the user search
        /// </summary>
        /// <param name="viewModel">View model of main page</param>
        /// <param name="whitelistIDs">IDs of searched items (Empty if nothing is searched)</param>
        /// <param name="currentList">Items already displayed on the page</param>
        /// <param name="whitelist">searched items (Empty if nothing is searched)</param>
        /// <param name="returnedViewModel">View model of main page with loaded items that will be displayed to user</param>
        /// <returns>True - If there are less items that corresponds to the search than length of the list of recommendations</returns>
        private bool ReturnSearched(MainViewModel viewModel, List<int> whitelistIDs, int[] currentList,
            IQueryable<Item> whitelist, out MainViewModel returnedViewModel)
        {
            returnedViewModel = viewModel;
            if (whitelistIDs.Count > 0){
                whitelistIDs.RemoveAll(id => currentList.Contains(id));
                if (whitelistIDs.Count <= SystemParameters.LengthOfRecommendationsList)
                {
                    returnedViewModel.Items = whitelist.Where(item => whitelistIDs.Contains(item.Id)).ToList();
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Sets parameters to the properties of main page view model
        /// </summary>
        /// <param name="viewModel">main page view model</param>
        /// <param name="user">User that requests the page</param>
        /// <param name="metrics">Used metrics</param>
        /// <param name="metricsimportance">Currently given importance from user</param>
        /// <param name="search">Searched value from user request. Will be set in the textbox</param>
        /// <param name="typeOfSearch">Specifies if user clicked the search for main search or in movie filter</param>
        /// <param name="director">Movie filter user search on director</param>
        /// <param name="actor">Movie filter user search on actor></param>
        /// <param name="releasedatefrom">Movie filter user search on the earliest release date</param>
        /// <param name="releasedateto">Movie filter user search on the latest release date</param>
        /// <param name="genres">Movie filter user search on genres</param>
        private void SetMainViewModel(MainViewModel viewModel, User user, List<Metric> metrics, string[] metricsimportance, 
            string search, string typeOfSearch, string director, string actor, string releasedatefrom, string releasedateto,
            string[] genres)
        {
            viewModel.SetMetricImportance(user, metrics, metricsimportance, _context);
            viewModel.UsedVariants = user.GetMetricVariants(_context, metrics.Select(m => m.Id).ToList());
            viewModel.SearchValue = search ?? "";
            viewModel.FilterValues.Add("TypeOfSearch", typeOfSearch);
            viewModel.FilterValues.Add("Director", director);
            viewModel.FilterValues.Add("Actor", actor);
            viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
            viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
            viewModel.FilterValues.Add("Genres", string.Join(',', genres));
            viewModel.FilterValues.Add("MetricsImportance", string.Join(',', metricsimportance ?? new string[0]));
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
            string director, string actor, string[] genres, string method, ApplicationDbContext context)
        {
            string message = "";
            if (method == "POST")
            {
                UserActCache.AddAct(user.Id.ToString(), "PropertyBlock", context);
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
