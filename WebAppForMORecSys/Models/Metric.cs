using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Class that represent one metric of recommender system
    /// </summary>
    public class Metric
    {
        /// <summary>
        /// Unique ID of metric
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the metric
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID of recommender system that's using the metric
        /// </summary>
        [ForeignKey("RecommenderSystem")]
        public int RecommenderSystemID { get; set; }

        /// <summary>
        /// Default explanation why item is contributing to the metric
        /// </summary>
        public string DefaultExplanation { get; set; }

        /// <summary>
        /// Recommender system that's using the metric
        /// </summary>
        public RecommenderSystem RecommenderSystem { get; set; }


        public List<UserMetric> UserMetricList { get; set; }
        public Metric()
        {

        }
    }
}
