using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// Class that creates and then returns randomly ordered list of combinations of set parameters 
    /// to choose users first set variants
    /// </summary>
    public static class NewUserSetting
    {

        /// <summary>
        /// Instance of random, used by GetLatinSquaresForFirstSetting
        /// </summary>
        static Random rnd = new Random();

        /// <summary>
        /// Creates randomly ordered list of combinations of set parameters if not already created
        /// </summary>
        /// <param name="context">Database conrext</param>
        /// <returns>combinations of set parameters</returns>
        public static List<List<object>> GetCombinationsForFirstSetting(ApplicationDbContext context)
        {
            if (_combinations == null)
            {

                var lists = new List<List<object>>();
                /*{
                    Enum.GetValues(typeof(AddBlockRuleView)).Cast<object>().ToList(),
                    Enum.GetValues(typeof(ExplanationView)).Cast<object>().ToList(),
                    Enum.GetValues(typeof(MetricContributionScoreView)).Cast<object>().ToList(),
                    Enum.GetValues(typeof(PreviewExplanationView)).Cast<object>().ToList(),
                    Enum.GetValues(typeof(MetricsView)).Cast<object>().ToList()
                };*/
                var metricsWithVariants = context.Metrics.Include(m => m.MetricVariants)
                    .Where(m => m.MetricVariants.Count() > 0).OrderBy(m => m.Id);
                foreach (var metric in metricsWithVariants)
                {
                    lists.Add(metric.MetricVariants.OrderBy(mv => mv.Id).Select(mv => mv.Code).Cast<object>().ToList());
                }
                List<List<object>> latinSquares = new List<List<object>> { new List<object>() };
                foreach (var list in lists)
                {
                    // cross join the current result with each member of the next list
                    latinSquares = latinSquares.SelectMany(ls => list, (ls, o) => AddToAndReturn(ls, o)).ToList();
                }
                latinSquares = latinSquares.OrderBy(ls => Guid.NewGuid()).ToList();
                _combinations = latinSquares;
            }
            return _combinations;

        }

        private static List<List<object>> _combinations;

        /// <summary>
        /// Creates new instance of list that is exteded by object
        /// </summary>
        /// <param name="ls">List to be extended</param>
        /// <param name="o">Object that will be add to the end of the list</param>
        /// <returns>new instance of list that is exteded by object</returns>
        private static List<object> AddToAndReturn(this List<object> ls, object o)
        {
            List<object> newList = new List<object>();
            newList.AddRange(ls);
            newList.Add(o);
            return newList;
        }
    }
}
