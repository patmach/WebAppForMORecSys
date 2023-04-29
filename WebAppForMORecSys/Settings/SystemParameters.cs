using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Settings
{
    public static class SystemParameters
    {
        public static string Controller { get; set; }

        public static MetricsView MetricsView { get; set; }

        public static RecommenderSystem RecommenderSystem { get; set; }

        public static string[] Colors { get; set; }

        public static Dictionary<Metric, string> MetricsToColors { get; set; }
        
    }

    public enum MetricsView
    {
        Sliders,
        TextBoxNumbers,
        DragAndDrop
    }
    
}
