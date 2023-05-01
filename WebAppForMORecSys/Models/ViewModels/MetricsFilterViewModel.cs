namespace WebAppForMORecSys.Models.ViewModels
{
    public class MetricsFilterViewModel
    {
        public Dictionary<Metric, int> Metrics { get; set; }
        public User User { get; set; }
        public MetricsFilterViewModel() {
            Metrics = new Dictionary<Metric, int>();
        }
    }
}
