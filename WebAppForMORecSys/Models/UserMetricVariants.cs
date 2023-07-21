using System.ComponentModel.DataAnnotations.Schema;
using WebAppForMORecSys.Areas.Identity.Data;

namespace WebAppForMORecSys.Models
{
    public class UserMetricVariants
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("MetricVariant")]
        public int MetricVariantID { get; set; }

        public User User { get; set; }
        public MetricVariant MetricVariant { get; set; }

        public UserMetricVariants()
        {

        }
    }
}
