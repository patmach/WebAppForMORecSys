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
    public class MovieRequestsHandler
    {
        private ApplicationDbContext _context;
        public MovieRequestsHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MainViewModel> ProcessMainQuery(User user, string search, string director,
          string actor, string[] genres, string typeOfSearch, string releasedateto, string releasedatefrom,
          string[] metricsimportance)
        {
            var viewModel = new MainViewModel();
            if (user != null)
            {
                viewModel.CurrentUser = user;
                viewModel.CurrentUserRatings = await (from rating in _context.Ratings
                                                      where rating.UserID == viewModel.CurrentUser.Id
                                                      select rating).ToListAsync();
            }
            RecommenderSystem rs = SystemParameters.RecommenderSystem;
            List<Metric> metrics = await _context.Metrics.Include(m => m.metricVariants)
                .Where(m => m.RecommenderSystemID == rs.Id).ToListAsync();
            viewModel.SetMetricImportance(user, metrics, metricsimportance, _context);

            viewModel.SearchValue = search ?? "";
            viewModel.FilterValues.Add("Director", director);
            viewModel.FilterValues.Add("Actor", actor);
            viewModel.FilterValues.Add("ReleaseDateFrom", releasedatefrom);
            viewModel.FilterValues.Add("ReleaseDateTo", releasedateto);
            viewModel.FilterValues.Add("Genres", string.Join(',', genres));

            IQueryable<Item> whitelist = Movie.GetPossibleItems(_context.Items, user, search, director, actor, genres, typeOfSearch, releasedateto, releasedatefrom);
            int[] whitelistIDs = whitelist == null ? new int[0] : await whitelist.Select(item => item.Id).ToArrayAsync();
            if((whitelistIDs.Length>0 && whitelistIDs.Length < 20) || false) //TODO Doplň něco podle configu místo false
            {
                viewModel.Items = whitelist.Take(20);
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
                viewModel.Items = _context.Items.Where(item => !blacklist.Contains(item.Id)).Take(50);
            }
            return viewModel;
        }

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
