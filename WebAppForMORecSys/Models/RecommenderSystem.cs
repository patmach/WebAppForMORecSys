using System.Text.Json.Nodes;

namespace WebAppForMORecSys.Models
{
    public class RecommenderSystem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HTTPUri { get; set; }
        public String SupportedCommunication { get; set; }
        public String Hyperparameters { get; set; }

        public List<Metric> Metrics { get; set; }
        public RecommenderSystem() { }
    }
}
