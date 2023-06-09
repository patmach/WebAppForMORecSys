namespace WebAppForMORecSys.Settings
{
    /// <summary>
    /// Possibilities on type of metrics filter
    /// </summary>
    public enum MetricsView
    {
        Sliders,
        TextBoxNumbers,
        DragAndDrop,
        PlusMinusButtons
    }

    public static class MetricsViewExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="explanationView"></param>
        /// <returns>Name to display for the enum value</returns>
        public static string ToFriendlyString(this MetricsView metricsView)
        {
            switch (metricsView)
            {
                case MetricsView.Sliders:
                    return "Sliders";
                case MetricsView.TextBoxNumbers:
                    return "Textboxes";
                case MetricsView.DragAndDrop:
                    return "Drag and drop";
                case MetricsView.PlusMinusButtons:
                    return "Buttons";
            }
            return "";
        }
    }
}
