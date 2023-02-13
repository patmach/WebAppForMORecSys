using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
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