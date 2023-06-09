using System.Text.Json.Nodes;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Class that represent recommender system that is used for recommendations
    /// </summary>
    public class RecommenderSystem
    {
        /// <summary>
        /// Unique ID of the recommender system
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the recommender system
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URI of the recommender system API
        /// </summary>
        public string HTTPUri { get; set; }

        /// <summary>
        /// Settings on what communication is supported
        /// </summary>
        public String SupportedCommunication { get; set; }

        /// <summary>
        /// Settings of hyperparameters of the system
        /// </summary>
        public String Hyperparameters { get; set; }

        /// <summary>
        /// Metrics that recommender system is using
        /// </summary>
        public List<Metric> Metrics { get; set; }
        public RecommenderSystem() { }
    }
}
