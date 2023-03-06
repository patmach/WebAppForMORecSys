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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using WebAppForMORecSys.Helpers;
using Microsoft.AspNetCore.Identity;
using WebAppForMORecSys.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore.Update;

namespace WebAppForMORecSys.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Account> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<Account> userManager)
        {
        
            _context = context;
            _userManager = userManager;
            //MovielensLoader.LoadMovielensData(context);

        }

       
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "Movies");
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}