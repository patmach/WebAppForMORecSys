using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Helpers
{
    public static class LatinSquaresForNewUser
    {

        /// <summary>
        /// Instance of random, used by GetLatinSquaresForFirstSetting
        /// </summary>
        static Random rnd = new Random();

        public static List<List<object>> GetLatinSquaresForFirstSetting(ApplicationDbContext context)
        {
            if (_latinSquares == null)
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
                _latinSquares = latinSquares;
            }
            return _latinSquares;

        }

        private static List<List<object>> _latinSquares;

        private static List<object> AddToAndReturn(this List<object> ls, object o)
        {
            List<object> newList = new List<object>();
            newList.AddRange(ls);
            newList.Add(o);
            return newList;
        }
    }
}
