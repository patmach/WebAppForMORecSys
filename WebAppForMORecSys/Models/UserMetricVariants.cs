﻿using CsvHelper.Configuration.Attributes;
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
        /// Instance of random, used by SetRandomMetricVariants
        /// </summary>
        static Random rnd = new Random();

        /// <summary>
        /// Sets random metric variants for user 
        /// </summary>
        /// <param name="user">Newly created user</param>
        /// <param name="context">Database context</param>
        public static void SetRandomMetricVariants(User user, ApplicationDbContext context)
        {

            var metricsWithVariants = context.Metrics.Include(m => m.metricVariants).Where(m => m.metricVariants.Count > 0).ToList();
            List<string> selectedVariantsCodes = new List<string>();
            foreach (var metric in metricsWithVariants)
            {
                MetricVariant mv = metric.metricVariants[rnd.Next(metricsWithVariants.Count)];
                var newUmv = new UserMetricVariants
                {
                    UserID = user.Id,
                    MetricVariantID = mv.Id
                };
                context.Add(newUmv);
                selectedVariantsCodes.Add(mv.Code);
            }
            context.SaveChanges();
            UserActCache.AddActs(user.Id.ToString(), selectedVariantsCodes, context);
        }
    }
}
