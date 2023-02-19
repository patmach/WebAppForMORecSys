using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        private class MovieMap : ClassMap<Movie>
        {
            public MovieMap()
            {
                Map(p => p.Id).Convert(args => int.Parse(args.Row.GetField("movieId")));
                Map(p => p.Name).Index(1);
                Map(p => p.JSONParams).Index(2);
            }
        }

        private class Link
        {
           
            public string Id { get; set; }
            public string IMBDID { get; set; }
            public string TMBDID { get; set; }
            public Link()
            {

            }

        }
        private class LinkMap : ClassMap<Link>
        {
            public LinkMap()
            {
                Map(p => p.Id).Index(0);
                Map(p => p.IMBDID).Index(1);
                Map(p => p.TMBDID).Index(2);
            }
        }

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            List<Movie> movies = new List<Movie>();
            using (var reader = new StreamReader("Resources/Movielens/movies.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<MovieMap>();
                movies = csv.GetRecords<Movie>().ToList();
            }
            movies.ForEach(x => { x.JSONParams = "Genres: [" + x.JSONParams + ']'; });

            List<Link> links = new List<Link>();
            using (var reader = new StreamReader("Resources/Movielens/links.csv"))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Context.RegisterClassMap<LinkMap>();
                links = csv.GetRecords<Link>().ToList();
            }
            

        }

        public class MainViewModel
        {
            public List<Metric> Metrics { get; set; }
            public List<Item> Items { get; set; }

            public MainViewModel()
            {
                this.Metrics = new List<Metric>();
                this.Items = new List<Item>();
            }
        }
        public async Task<IActionResult> Index(string search)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'Context.Items'  is null.");
            }
            var viewModel = new MainViewModel();
            var metrics = from metric in _context.Metrics
                          select metric;
            viewModel.Metrics = await metrics.ToListAsync();
            viewModel.Metrics = new List<Metric> { new Metric { Name = "Relevance" },
                new Metric { Name = "Novelty" }, new Metric { Name = "Diversity" } };//DELETE Later
            var items = from item in _context.Items
                         select item;
            List<Item> items1;
            List<Item> items2;
            List<Item> items3;

            if (!String.IsNullOrEmpty(search))
            {
                items1 = items.Where(s => s.Name!.Contains(search)).ToList();
                items2 = items.Where(s => s.JSONParams!.Contains(search)).ToList();
                items3 = items.Where(s => s.Description!.Contains(search)).ToList();
                items1.AddRange(items2);
                items1.AddRange(items3);
                viewModel.Items = items1.Distinct().ToList();
                return View(viewModel);
            }
            viewModel.Items =  await items.ToListAsync();
            return View(viewModel);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}