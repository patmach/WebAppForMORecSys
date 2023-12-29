using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Data;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Represents current user choice of variant of the metric
    /// </summary>
    public class UserMetricVariants
    {
        /// <summary>
        /// Unique ID of the user choice of metric variant
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  ID of user with selected variant
        /// </summary>
        [ForeignKey("User")]
        public int UserID { get; set; }

        /// <summary>
        /// ID of selected variant
        /// </summary>
        [ForeignKey("MetricVariant")]
        public int MetricVariantID { get; set; }

        /// <summary>
        ///  User with selected variant
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Selected variant
        /// </summary>
        public MetricVariant MetricVariant { get; set; }

        public UserMetricVariants()
        {

        }

        public static void Save(int userID, MetricVariant mv, ApplicationDbContext context, bool saveChanges = true)
        {
            List<UserMetricVariants> umvs = context.UserMetricVariants.Include(umv => umv.MetricVariant)
                .Where(umv => (umv.UserID == userID) && (umv.MetricVariant.MetricID == mv.MetricID)).ToList();
            UserMetricVariants umv = umvs.FirstOrDefault();
            if (umvs.Count > 1)
            {
                try
                {
                    context.RemoveRange(umvs);
                    context.SaveChanges();
                    umv = null;
                }
                catch (DbUpdateConcurrencyException)
                {
                    //Nothing to do. Was already deleted
                }
            }
            if (umv == null)
            {
                var newUmv = new UserMetricVariants
                {
                    UserID = userID,
                    MetricVariantID = mv.Id
                };
                context.Add(newUmv);
            }
            else
            {
                umv.MetricVariantID = mv.Id;
                context.Update(umv);
            }
            if (saveChanges)
                context.SaveChanges();
        }

        
        /// <summary>
        /// Sets random metric variants for user 
        /// </summary>
        /// <param name="user">Newly created user</param>
        /// <param name="context">Database context</param>
        public static void SetRandomMetricVariants(User user, List<List<object>> combinations, 
            ApplicationDbContext context)
        {
            var selectedRow = combinations[user.Id % combinations.Count];
            var metricsWithVariants = context.Metrics.Include(m => m.MetricVariants)
                .Where(m => m.MetricVariants.Count > 0).OrderBy(m => m.Id);
            List<string> selectedVariantsCodes = new List<string>();
            int count = -1;
            foreach (var metric in metricsWithVariants)
            {
                count++;
                var code = (string)selectedRow[count];
                MetricVariant mv = metric.MetricVariants.Where(mv => mv.Code == code).First();
                Save(user.Id, mv, context, false);
                selectedVariantsCodes.Add(mv.Code);
            }
            context.SaveChanges();
            //UserActCache.AddActs(user.Id.ToString(), selectedVariantsCodes, context);
        }
    }
}
