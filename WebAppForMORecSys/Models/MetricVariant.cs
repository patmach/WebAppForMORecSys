

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Class that represent possible variant how metric is computed inside RS
    /// </summary>
    public class MetricVariant
    {
        /// <summary>
        /// Unique ID of MetricVariant
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// ID of metric that variant belongs to
        /// </summary>
        [ForeignKey("Metric")]
        public int MetricID { get; set; }

        /// <summary>
        /// Code of variant that corresponds to code in Recommender System 
        /// </summary>
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// Says if variant is default for given metric
        /// </summary>
        [Required]
        public bool DefaultVariant { get; set; }

        /// <summary>
        /// Description of variant
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Explanation on why item is contributing to the metric variant
        /// </summary>
        public string? Explanation { get; set; }

        /// <summary>
        /// Default explanation on why item is not contributing well to the metric
        /// </summary>
        public string NegativeExplanation { get; set; }

        /// <summary>
        /// Metric that variant belongs to
        /// </summary>
        public Metric Metric { get; set; }

        public List<UserMetricVariants> UserMetricVariantsList { get; set; }

        public MetricVariant()
        {

        }
    }
}
