using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Identity.Client;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Loggers;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Settings
{
    /// <summary>
    /// Class that contains system default settings
    /// </summary>
    public static class SystemParameters
    {
        /// <summary>
        /// Name of used controller
        /// </summary>
        public static string DomainController { get; set; } = "Movies";

        /// <summary>
        /// Length of list of recommendatins returned by RS
        /// </summary>
        public static int LengthOfRecommendationsList { get; set; } = 15;

        /// <summary>
        /// Minimal ratings before recommender starts working
        /// </summary>
        public static int MinimalPositiveRatings { get; set; } = 3;

        /// <summary>
        /// Displayed name of web app
        /// </summary>
        public static string Name { get => DomainController; }

        /// <summary>
        /// Default type of metrics filter
        /// </summary>
        public static MetricsView MetricsView { get; set; } = MetricsView.Sliders;
#if DEBUG
        /// <summary>
        /// Base address where the app is run
        /// </summary>
        public static string BaseAddress { get; } = "https://localhost:44397/";
#else
        /// <summary>
        /// Base address where the app is run
        /// </summary>
        public static string BaseAddress { get; } = "http://webapp:80/";
#endif
        /// <summary>
        /// Get used recommender system
        /// </summary>
        public static RecommenderSystem GetRecommenderSystem(ApplicationDbContext context) 
        {            
            if (_recommenderSystem == null) {
                _recommenderSystem = context.RecommenderSystems.Where(rs => rs.Name == _recommenderSystemName).First();
                var metrics = context.Metrics.Where(m => m.RecommenderSystemID == _recommenderSystem.Id).ToArray();
                MetricsToColors = Enumerable.Range(0, metrics.Length).ToDictionary(i => metrics[i], i => Colors[i]);
            }
            return _recommenderSystem;           
        }

        /// <summary>
        /// Main file logger for debugging purpose
        /// </summary>
        public static MyFileLogger MainDebugLogger { get; } = new MyFileLogger("Logs/MainLog.txt");

        /// <summary>
        /// Name of used recommender system
        /// </summary>
        private static string _recommenderSystemName = "MOO as voting fast";

        /// <summary>
        /// Used recommender system
        /// </summary>
        private static RecommenderSystem? _recommenderSystem;

        /// <summary>
        /// Default colours for used metrics (according to their ranking in the database)
        /// </summary>
        public static string[] Colors { get; set; } = new string[] { "#002d62", "#ff0000", "#f98686", "#00FFFF", "#088F8F", "#00A36C" };

        /// <summary>
        /// Dictionary mapping metrics to their default colours
        /// </summary>
        public static Dictionary<Metric, string>? MetricsToColors { get; set; }

        /// <summary>
        /// Default type of new block rules addition
        /// </summary>
        public static AddBlockRuleView AddBlockRuleView { get; set; } = AddBlockRuleView.Single;

        /// <summary>
        /// Default type of information seen in explanations
        /// </summary>
        public static ExplanationView ExplanationView { get; set; } = ExplanationView.BestMetricPopover;

        /// <summary>
        /// Default displaying of score for metrics in explanations
        /// </summary>
        public static MetricContributionScoreView MetricContributionScoreView { get; set; } = MetricContributionScoreView.Raw;

        /// <summary>
        /// Default displaying of metrics share in preview
        /// </summary>
        public static PreviewExplanationView PreviewExplanationView { get; set; } = PreviewExplanationView.FullBorderImage;


        
    }



}
