using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using System.Reflection.Emit;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Settings;

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
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Item> Items { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<RecommenderSystem> RecommenderSystems { get; set; }
        public DbSet<UserMetricVariants> UserMetricVariants { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<MetricVariant> MetricVariants { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Act> Acts { get; set; }
        public DbSet<QuestionAct> QuestionsActs { get; set; }
        public DbSet<UserAct> UserActs { get; set; }
    }
}