using WebAppForMORecSys.Areas.Identity.Data;

namespace WebAppForMORecSys.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public String JSONBlockRules { get; set; }

        public String JSONFilter { get; set; }

        public String SearchHistory { get; set; }

        public List<UserMetric> UserMetricList { get; set; }

        public List<Rating> Ratings { get; set; }

        public List<Interaction> Interactions { get; set; }

        public Account account;
    }
}
