using System.Text;
using WebAppForMORecSys.Helpers;

namespace WebAppForMORecSys.Models.ViewModels
{
    public class PreviewDetailViewModel
    {
        public Item item;
        public User user;
        public List<Rating> userRatings;
        public Dictionary<Metric, double> metricsContribution;
        public PreviewDetailViewModel(Item item, User user, List<Rating> userRatings, Dictionary<Metric, double> metricsContribution = null)
        {
            this.item = item;
            this.user = user;
            this.userRatings = userRatings;
            this.metricsContribution = metricsContribution;
        }

        public string MetricsContributionToBorderImage(User user, double[] metricsContribution, string direction = "bottom right")
        {
            StringBuilder borderImage = new StringBuilder();
            borderImage.Append($"linear-gradient(to {direction}");
            int lastpoint = 0;
            int sum = 0;
            for (int i = 0; i < metricsContribution.Length; i++)
            {
                sum += (int)metricsContribution[i];
                borderImage.Append(',');
                borderImage.Append(user.GetColors()[i]);
                borderImage.Append(' ');
                borderImage.Append(lastpoint);
                borderImage.Append("% ");
                borderImage.Append(sum);
                borderImage.Append('%');
                lastpoint = sum;
            }
            borderImage.Append(") 1");
            return borderImage.ToString();
        }
    }
}
