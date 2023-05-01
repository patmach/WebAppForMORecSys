namespace WebAppForMORecSys.Models.ViewModels
{
    public class PreviewDetailViewModel
    {
        public Item item;
        public User user;
        public List<Rating> userRatings;
        public Dictionary<Metric, int> metricsContribution;
        public PreviewDetailViewModel(Item item, User user, List<Rating> userRatings, Dictionary<Metric, int> metricsContribution = null)
        {
            this.item = item;
            this.user = user;
            this.userRatings = userRatings;
            this.metricsContribution = metricsContribution;
        }
    }
}
