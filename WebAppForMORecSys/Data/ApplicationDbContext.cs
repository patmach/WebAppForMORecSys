using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection.Emit;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Data
{
    public class ApplicationDbContext : IdentityDbContext<Account>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Item)
                .WithMany(i => i.Ratings)
                .HasForeignKey(r=> r.ItemID);
            modelBuilder.Entity<Metric>()
                .HasOne(m => m.RecommenderSystem)
                .WithMany(rs => rs.Metrics);
            modelBuilder.Entity<Interaction>()
                .HasOne(ia => ia.User)
                .WithMany(u => u.Interactions);
            modelBuilder.Entity<Interaction>()
                .HasOne(ia => ia.Item)
                .WithMany(i => i.Interactions);
            modelBuilder.Entity<UserMetric>()
                .HasOne(um => um.User)
                .WithMany(u=>u.UserMetricList);
            modelBuilder.Entity<UserMetric>()
                .HasOne(um => um.Metric)
                .WithMany(m=>m.UserMetricList);
            */

            base.OnModelCreating(modelBuilder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);            
        }
        public DbSet<Item> Items { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<RecommenderSystem> RecommenderSystems { get; set; }
        public DbSet<UserMetric> UsersMetrics { get; set; }
        public DbSet<User> Users { get; set; }
    }
}