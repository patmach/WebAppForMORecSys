using Microsoft.Build.Logging;
using System.Text;
using WebAppForMORecSys.Helpers;

namespace WebAppForMORecSys.Models.ViewModels
{
    /// <summary>
    /// View model for item preview and detail
    /// </summary>
    public class PreviewDetailViewModel
    {
        /// <summary>
        /// Item that the preview or detail is about
        /// </summary>
        public Item item;

        /// <summary>
        /// User to whom is the view displayed
        /// </summary>
        public User user;

        /// <summary>
        /// Ratings of that user
        /// </summary>
        public List<Rating> userRatings;

        /// <summary>
        /// Metrics score of this item. Only used when its displayed as recommendation 
        /// </summary>
        public Dictionary<Metric, double> metricsContribution;
        public PreviewDetailViewModel(Item item, User user, List<Rating> userRatings, Dictionary<Metric, double> metricsContribution = null)
        {
            this.item = item;
            this.user = user;
            this.userRatings = userRatings;
            this.metricsContribution = metricsContribution;
        }

        /// <summary>
        /// Border-image value for this item according to metrics score
        /// </summary>
        /// <param name="user">User to whom is the view displayed</param>
        /// <param name="metricsContribution">Metrics score of this item</param>
        /// <param name="direction">Sets in which direction the colors in image should be changed</param>
        /// <returns></returns>
        public string MetricsContributionToBorderImage(User user, double[] metricsContribution, string direction = "bottom right")
        {
            var percentageMetricsContribution = new double[metricsContribution.Length];
            for (int i = 0; i < metricsContribution.Length; i++)
            {
                percentageMetricsContribution[i] = 100 * metricsContribution[i]/ metricsContribution.Sum();
            }
            StringBuilder borderImage = new StringBuilder();
            borderImage.Append($"linear-gradient(to {direction}");
            var colors = user.GetColors().ToList().GetRange(0,metricsContribution.Length).ToArray();
            Array.Sort(percentageMetricsContribution, colors);
            Array.Reverse(colors);
            Array.Sort(percentageMetricsContribution);
            Array.Reverse(percentageMetricsContribution);
            int lastpoint = 0;
            int sum = 0;
            for (int i = 0; i < percentageMetricsContribution.Length; i++)
            {
                sum += (int)percentageMetricsContribution[i];
                borderImage.Append(',');
                borderImage.Append(colors[i]);
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
