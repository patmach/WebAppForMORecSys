namespace WebAppForMORecSys.Models.ViewModels
{
    /// <summary>
    /// View model for the metrics filter partial view
    /// </summary>
    public class MetricsFilterViewModel
    {
        /// <summary>
        /// Keys - used metrics, int - default importance for each metrics
        /// </summary>
        public Dictionary<Metric, int> Metrics { get; set; }

        /// <summary>
        /// User for whom the page is loaded
        /// </summary>
        public User User { get; set; }

        public MetricsFilterViewModel() {
            Metrics = new Dictionary<Metric, int>();
        }
    }
}
