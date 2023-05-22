using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Settings
{
    public static class SystemParameters
    {
        public static string Controller { get; set; } = "Movies";

        public static string Name { get => Controller; }

        public static MetricsView MetricsView { get; set; } = MetricsView.PlusMinusButtons;

        public static RecommenderSystem RecommenderSystem { get; set; }

        public static string[] Colors { get; set; } = new string[] { "#002D62", "#0000FF", "#007FFF", "#00FFFF", "#F0F8FF" };

        public static Dictionary<Metric, string> MetricsToColors { get; set; }

        public static AddBlockRuleView AddBlockRuleView { get; set; } = AddBlockRuleView.Single;

        public static ExplanationView ExplanationView { get; set; } = ExplanationView.BestMetricPopover;
    }

    
    
}
