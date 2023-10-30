namespace WebAppForMORecSys.Settings
{
    
    /// <summary>
    /// Possibilities on display of score for metrics in explanations
    /// </summary>
    public enum MetricContributionScoreView
    {
        Percentage,
        Raw,
        Rounded,
        Bar
    }

    public static class MetricContributionScoreViewExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="metricContributionScoreView"></param>
        /// <returns>Name to display for the enum value</returns>
        public static string ToFriendlyString(this MetricContributionScoreView metricContributionScoreView )
        {
            switch ( metricContributionScoreView )
            {
                case MetricContributionScoreView.Percentage:
                    return "Percentage share of full score";
                case MetricContributionScoreView.Raw:
                    return "Raw score";
                case MetricContributionScoreView.Rounded:
                    return "Rounded score";
                case MetricContributionScoreView.Bar:
                    return "Bar chart";
            }
            return "";
        }
    }
}
