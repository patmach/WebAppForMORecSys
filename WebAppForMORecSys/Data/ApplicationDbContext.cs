﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
            SystemParameters.RecommenderSystem = RecommenderSystems.Where(rs => rs.Name == "SimpleMF").First();
            var metrics = Metrics.Where(m => m.RecommenderSystemID == SystemParameters.RecommenderSystem.Id).ToArray();
            SystemParameters.MetricsToColors = Enumerable.Range(0, metrics.Length).ToDictionary(i => metrics[i], i => SystemParameters.Colors[i]);

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
        public DbSet<UserMetric> UsersMetrics { get; set; }
        public DbSet<User> Users { get; set; }
    }
}