using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAppForMORecSys.Models.ViewModels
{
    public class MainViewModel
    {
        public Dictionary<Metric, int> Metrics { get; set; }
        public IQueryable<Item> Items { get; set; }

        public string SearchValue ="";

        public User CurrentUser { get; set; }

        public List<Rating> CurrentUserRatings { get; set; }


        public Dictionary<string, string> FilterValues = new Dictionary<string, string>();
        public MainViewModel()
        {
            this.Metrics = new Dictionary<Metric, int>();
        }
    }
}
