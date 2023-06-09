namespace WebAppForMORecSys.Settings
{
    /// <summary>
    /// Possibilities on type of information seen in explanations
    /// </summary>
    public enum ExplanationView
    {
        AllMetricsPopover,
        BestMetricPopover
    }

    public static class ExplanationViewExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="explanationView"></param>
        /// <returns>Name to display for the enum value</returns>
        public static string ToFriendlyString(this ExplanationView explanationView)
        {
            switch (explanationView)
            {
                case ExplanationView.AllMetricsPopover:
                    return "Contribution of all metrics";
                case ExplanationView.BestMetricPopover:
                    return "Contribution of best metric(s)";
            }
            return "";
        }
    }

}
