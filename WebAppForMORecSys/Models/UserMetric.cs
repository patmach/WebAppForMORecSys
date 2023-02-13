using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;

namespace WebAppForMORecSys.Models
{
    public class UserMetric
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Metric")]
        public int MetricID { get; set; }

        public User User { get; set; }
        public Metric Metric { get; set; }

        public UserMetric()
        {

        }
    }
}
