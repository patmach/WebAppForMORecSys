namespace WebAppForMORecSys.Settings
{
    /// <summary>
    /// Possibilities on type of metrics filter
    /// </summary>
    public enum MetricsView
    {
        Sliders,
        Textboxes,
        DragAndDrop,
        PlusMinusButtons
    }

    /// <summary>
    /// Extension class with ToFriendlyString for enum MetricsView
    /// </summary>
    public static class MetricsViewExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="metricsView"></param>
        /// <returns>Name to display for the enum value</returns>
        public static string ToFriendlyString(this MetricsView metricsView)
        {
            switch (metricsView)
            {
                case MetricsView.Sliders:
                    return "Sliders";
                case MetricsView.Textboxes:
                    return "Textboxes";
                case MetricsView.DragAndDrop:
                    return "Drag and drop";
                case MetricsView.PlusMinusButtons:
                    return "Buttons";
            }
            return "";
        }

        /// <summary>
        /// </summary>
        /// <param name="metricsView"></param>
        /// <returns>Explanation on how to use the metrics filter in HELP</returns>
        public static string Help(this MetricsView metricsView)
        {
            switch (metricsView)
            {
                case MetricsView.Sliders:
                    return "You can change importance of metrics by moving the sliders. If you overcome the sum 100, values of other sliders will decrease.";
                case MetricsView.Textboxes:
                    return "You can change importance of metrics by writing numeric values inside the text boxes. If you overcome the sum 100, values in other text boxes will decrease.";
                case MetricsView.DragAndDrop:
                    return "You can change importance of metrics by drag and drop action. You can drag (by pressing the mouse button) the box with metric and drop (by releasing the mouse button) it over the box of other metrics that should change place with the dragged one. Metrics on the left has greater importance than the ones to the right.";
                case MetricsView.PlusMinusButtons:
                    return "You can change importance of metrics by clicking the + and - buttons. If you overcome the maximum sum, values of other metrics importances will decrease.";
            }
            return "";
        }
    }
}
