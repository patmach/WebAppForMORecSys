using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAppForMORecSys.Models.ViewModels
{
    public class MainViewModel
    {
        public List<Metric> Metrics { get; set; }
        public List<Item> Items { get; set; }

        public string SearchValue ="";

        public User CurrentUser { get; set; }

        public List<Rating> CurrentUserRatings { get; set; }


        public Dictionary<string, string> FilterValues = new Dictionary<string, string>();
        public MainViewModel()
        {
            this.Metrics = new List<Metric>();
            this.Items = new List<Item>();
        }
    }
}
