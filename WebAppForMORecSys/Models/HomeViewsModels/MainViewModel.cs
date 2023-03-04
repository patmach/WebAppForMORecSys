using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAppForMORecSys.Models.HomeViewsModels
{
    public class MainViewModel
    {
        public List<Metric> Metrics { get; set; }
        public List<Item> Items { get; set; }

        public string SearchValue ="";
        
        public MainViewModel()
        {
            this.Metrics = new List<Metric>();
            this.Items = new List<Item>();
        }
    }
}
