using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Areas.Identity.Data;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Data.Cache;
using WebAppForMORecSys.Helpers.JSONPropertiesHandlers;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Controllers
{
    /// <summary>
    /// Controller for the requests on actions related to UserMetricVariants
    /// </summary>
    public class UserMetricVariantsController : Controller
    {
        /// <summary>
        /// Database context
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// User manager for accesing acount the app communicates with
        /// </summary>
        private readonly UserManager<Account> _userManager;

        /// <summary>
        /// Gets connection to db and UserManager
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="userManager">User manager for accesing acount the app communicates with</param>
        public UserMetricVariantsController(ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// </summary>
        /// <param name="metricID">ID of metric whose variant will be selected</param>
        /// <returns>Partial view where user can select which metric variant he wants to use.</returns>
        public async Task<IActionResult> UserMetricSetting(int metricID)
        {
            var user = GetCurrentUser();
            var variants = _context.MetricVariants.Include(mv => mv.Metric).Where(mv => mv.MetricID == metricID).ToList();
            var choosed = _context.UserMetricVariants.Include(um => um.MetricVariant).
                Where(um => um.UserID == user.Id && variants.Contains(um.MetricVariant)).FirstOrDefault();
            if (choosed != null)
            {
                variants.ForEach(v => {
                    if (v.Id == choosed.MetricVariant.Id)
                        v.DefaultVariant = true;
                    else v.DefaultVariant = false;
                });
            }
            return PartialView(variants);
        }

        /// <summary>
        /// Saves variant of the metric that user has selected to use
        /// </summary>
        /// <param name="variant">Code of selected metric variant</param>
        /// <returns>No Content - if everything goes well</returns>
        public IResult Save(string variant)
        {
            User user = GetCurrentUser();
            var metricVariant = _context.MetricVariants.Include(mv => mv.Metric).Where(mv => mv.Code == variant)
                .FirstOrDefault();
            if (metricVariant == null)
            {
                return Results.BadRequest();
            }
            if (user == null)
            {
                return Results.Unauthorized();
            }
            UserMetricVariants.Save(user.Id, metricVariant, _context);
            //AddAct(metricVariant.Code);
            return Results.NoContent();
        }

        /// <summary>
        /// </summary>
        /// <returns>Currently logged user that sent this request.</returns>
        private User GetCurrentUser()
        {
            var account = _userManager.GetUserAsync(User).Result;
            User user = null;
            if (account != null)
            {
                user = _context.Users.Where(u => u.UserName == account.UserName).FirstOrDefault();
            }
            return user;
        }
    }
}

