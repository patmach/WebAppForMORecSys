using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Data;

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

        public static void Save(int userID, MetricVariant mv, ApplicationDbContext context)
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
            context.SaveChanges();
        }

        
        /// <summary>
        /// Sets random metric variants for user 
        /// </summary>
        /// <param name="user">Newly created user</param>
        /// <param name="context">Database context</param>
        public static void SetRandomMetricVariants(User user, List<List<object>> latinSquares, 
            ApplicationDbContext context)
        {
            var selectedRow = latinSquares[user.Id % latinSquares.Count];
            var metricsWithVariants = context.Metrics.Include(m => m.MetricVariants)
                .Where(m => m.MetricVariants.Count > 0).OrderBy(m => m.Id);
            List<string> selectedVariantsCodes = new List<string>();
            int count = -1;
            foreach (var metric in metricsWithVariants)
            {
                count++;
                var code = (string)selectedRow[count];
                MetricVariant mv = metric.MetricVariants.Where(mv => mv.Code == code).First();
                Save(user.Id, mv, context);
                selectedVariantsCodes.Add(mv.Code);
            }
            //UserActCache.AddActs(user.Id.ToString(), selectedVariantsCodes, context);
        }
    }
}
