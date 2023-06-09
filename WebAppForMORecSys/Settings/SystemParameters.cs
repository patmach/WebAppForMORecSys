﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public static string Controller { get; set; } = "Movies";

        /// <summary>
        /// Displayed name of web app
        /// </summary>
        public static string Name { get => Controller; }

        /// <summary>
        /// Default type of metrics filter
        /// </summary>
        public static MetricsView MetricsView { get; set; } = MetricsView.PlusMinusButtons;

        /// <summary>
        /// Used recommender system
        /// </summary>
        public static RecommenderSystem RecommenderSystem { get; set; }

        /// <summary>
        /// Default colours for used metrics (according to their ranking in the database)
        /// </summary>
        public static string[] Colors { get; set; } = new string[] { "#002D62", "#0000FF", "#007FFF", "#00FFFF", "#F0F8FF" };

        /// <summary>
        /// Dictionary mapping metrics to their default colours
        /// </summary>
        public static Dictionary<Metric, string> MetricsToColors { get; set; }

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

    }



}
