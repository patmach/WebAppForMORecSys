using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;

using WebAppForMORecSys.Data;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.ParseHelpers;
using System.Text.RegularExpressions;
using WebAppForMORecSys.Models.HomeViewsModels;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace WebAppForMORecSys.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

       
        public HomeController(ApplicationDbContext context)
        {
        
            _context = context;
            Item.context= context;
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
            var items = from item in _context.Items where item.Id < 100
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