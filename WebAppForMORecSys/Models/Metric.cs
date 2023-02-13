using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    public class Metric
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("RecommenderSystem")]
        public int RecommenderSystemID { get; set; }
        public string DefaultExplanation { get; set; }

        public RecommenderSystem RecommenderSystem { get; set; }
        public List<UserMetric> UserMetricList { get; set; }
        public Metric()
        {

        }
    }
}
