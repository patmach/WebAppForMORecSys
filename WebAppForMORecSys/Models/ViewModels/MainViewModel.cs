using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using System.Globalization;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Helpers;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Models.ViewModels
{
    /// <summary>
    /// View model for the main page
    /// </summary>
    public class MainViewModel
    {
        /// <summary>
        /// Keys - used metrics, int - default importance for each metrics
        /// </summary>
        public Dictionary<Metric, int> Metrics { get; set; }

        /// <summary>
        /// Items to be shown
        /// </summary>
        public List<Item> Items { get; set; }

        /// <summary>
        /// Metric contribution scores for each item
        /// </summary>
        public double[][] ItemsToMetricContributionScore { get;set;}

        /// <summary>
        /// Searched value from user request. Will be set in the textbox
        /// </summary>
        public string SearchValue ="";

        /// <summary>
        /// User for whom the page is loaded
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Ratings of the user
        /// </summary>
        public List<Rating> UserRatings { get; set; }


        /// <summary>
        /// Variants of metrics computation user uses or default variants
        /// </summary>
        public List<MetricVariant> UsedVariants;

        /// <summary>
        /// Searched values from query. Will be set in filter
        /// </summary>
        public Dictionary<string, string> FilterValues = new Dictionary<string, string>();

        /// <summary>
        /// Message to be shown in alert window
        /// </summary>
        public string Message;

        /// <summary>
        /// Message to be shown in top of the page
        /// </summary>
        public string Info;

        public MainViewModel()
        {
            this.Metrics = new Dictionary<Metric, int>();
        }

        /// <summary>
        /// Sets metric importance for the view model
        /// Priority
        ///     1. Currently given importance from user - also saved for the user as last
        ///     2. Last saved given importance from user
        ///     3. Every metric same importance - 100/(number of metrics)
        /// </summary>
        /// <param name="user">User for whom the metrics impotances will be set</param>
        /// <param name="metrics">Used metrics</param>
        /// <param name="metricsimportance">Currently given importance from user</param>
        /// <param name="context">Database context - for loading the last saved given importance from user</param>
        public void SetMetricImportance(User user, List<Metric> metrics, string[] metricsimportance, ApplicationDbContext context)
        {
            int numberOfParts = 0;
            for (int i = 0; i < metrics.Count(); i++)
            {
                numberOfParts += i + 1;
            }
            metricsimportance = metricsimportance.IsNullOrEmpty() ? user.GetMetricsImportance() : metricsimportance;
            if (metricsimportance.IsNullOrEmpty() || (metricsimportance.Length != metrics.Count()))
            {
                metricsimportance = new string[metrics.Count];
                for (int i = 0; i < metrics.Count(); i++)
                {
                    if (user.GetMetricsView() == MetricsView.DragAndDrop)
                        metricsimportance[i] = ((int)(100.0 / numberOfParts * (metrics.Count - i))).ToString();
                    else
                        metricsimportance[i] = (100 / metrics.Count()).ToString();
                }
            }
            else
            {
                user.SetMetricsImportance(metricsimportance);
                context.Update(user);
                context.SaveChanges();
            }            
                
            for (int i = 0; i < metrics.Count(); i++)
            {
                Metrics.Add(metrics[i], (int)double.Parse(metricsimportance[i], CultureInfo.InvariantCulture));
            }
        }

        
    }
}
