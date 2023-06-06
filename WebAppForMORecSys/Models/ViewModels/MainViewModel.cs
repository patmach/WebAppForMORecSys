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
    public class MainViewModel
    {
        public Dictionary<Metric, int> Metrics { get; set; }
        public IQueryable<Item> Items { get; set; }

        public double[][] ItemsToMetricImportance { get;set;}

        public string SearchValue ="";

        public User CurrentUser { get; set; }

        public List<Rating> CurrentUserRatings { get; set; }


        public Dictionary<string, string> FilterValues = new Dictionary<string, string>();
        public MainViewModel()
        {
            this.Metrics = new Dictionary<Metric, int>();
        }


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
