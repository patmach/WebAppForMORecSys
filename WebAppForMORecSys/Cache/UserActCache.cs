using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Runtime.Caching;
using System.Security.Policy;
using System.Threading;
using System.Timers;
using System.Web.Mvc;
using WebAppForMORecSys.Data;
using WebAppForMORecSys.Loggers;
using WebAppForMORecSys.Models;
using static System.Net.WebRequestMethods;

namespace WebAppForMORecSys.Cache
{
    public static class UserActCache
    {
        /// <summary>
        /// Timer for saving cache contents to database
        /// </summary>
        private static System.Timers.Timer _savetodbtimer;

        /// <summary>
        /// All acts that user can do
        /// </summary>
        public static List<Act> AllActs = new List<Act>();

        /// <summary>
        /// Cache object
        /// </summary>
        private static System.Runtime.Caching.MemoryCache _cache = new System.Runtime.Caching.MemoryCache("UserActs");

        /// <summary>
        /// Expiration time of data in cache
        /// </summary>
        private static readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// </summary>
        /// <param name="userId">Id of user whose acts should be returned</param>
        /// <param name="context">Database context</param>
        /// <returns>Acts done by user</returns>
        public static List<int> GetActs(string userId, ApplicationDbContext context)
        {
            if (_cache.Contains(userId))
            {
                return (List<int>)_cache.Get(userId);
            }
            var userActs = GetActsFromDatabase(userId, context);
            _cache.Set(userId, userActs, new CacheItemPolicy { SlidingExpiration = _expiration });
            return userActs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">Id of user whose acts should be returned</param>
        /// <param name="context">Database context</param>
        /// <returns>Acts done by user</returns>
        private static List<int> GetActsFromDatabase(string userId, ApplicationDbContext context)
        {
            int id = -1;
            if (!int.TryParse(userId, out id))
                return new List<int>();
            List<UserAct>? UserActs = context.Users.Include(u => u.UserActs).Where(u => u.Id == id).Select(u => u.UserActs)
                .FirstOrDefault();
            if (UserActs == null)
                return new List<int>();
            return UserActs.Select(ua => ua.ActID).ToList();
        }

        /// <summary>
        /// For logging of every act to file
        /// </summary>
        private static MyFileLogger logger = new MyFileLogger("Logs/UserActs.txt");


        /// <summary>
        /// Add acts done by the user
        /// </summary>
        /// <param name="userId">Id of user that has done new acts.</param>
        /// <param name="actCodes">Codes of acts user has done</param>
        /// <param name="context">Database context</param>
        public static void AddActs(string userId, List<string> actCodes, ApplicationDbContext context)
        {
            List<int> actIDs = AllActs.Where(a => actCodes.Contains(a.Code)).Select(a => a.Id).ToList();
            List<int> list = GetActs(userId, context);
            list.AddRange(actIDs);
            list = list.Distinct().ToList();
            _cache.Set(userId, list, new CacheItemPolicy { SlidingExpiration = _expiration });
            actIDs.ForEach(actID => logger.Log($"{userId};{actID};{DateTime.Now.ToString(logger.format)}"));
        }

        /// <summary>
        /// Add one act done by the user
        /// </summary>
        /// <param name="userId">Id of user that has done new act.</param>
        /// <param name="actCode">Code of act user has done</param>
        /// <param name="context">Database context</param>
        public static void AddAct(string userId, string actCode, ApplicationDbContext context)
        {
            int actID = AllActs.Where(a => a.Code==actCode).Select(a => a.Id).FirstOrDefault();
            if (actID == 0)
                return;
            List<int> list = GetActs(userId, context);
            if (!list.Contains(actID))
            {
                list.Add(actID);
            }
            _cache.Set(userId, list, new CacheItemPolicy { SlidingExpiration = _expiration });
            logger.Log($"{userId};{actID};{DateTime.Now.ToString(logger.format)}");
        }

        /// <summary>
        /// Sets timer for repeatedly calling function that saves cache contents to database
        /// </summary>
        /// <param name="context">Database context</param>
        public static void SetSaveToDbTimer(ApplicationDbContext context, HttpRequest Request)
        {
            if (_savetodbtimer == null)
            {
                string baseAddress = $"{Request.Scheme}://{Request.Host}/";
                SaveUserActsToDb(context);
                _savetodbtimer = new System.Timers.Timer(5 * 60 * 1000/*_expiration.TotalMilliseconds / 1.5*/);
                _savetodbtimer.Elapsed += new ElapsedEventHandler((sender, e) =>
                {
                    
                    var response = _client.GetAsync($"{baseAddress}UserAct/SaveContentsOfTheCache").Result;
                }
                );
                _savetodbtimer.Start();
            }
            
        }

        /// <summary>
        /// Http client used for sending requests
        /// </summary>
        private static HttpClient _client = new HttpClient();

        /// <summary>
        /// Saves cache contents to database
        /// </summary>
        /// <param name="context">Database context</param>
        public static void SaveUserActsToDb(ApplicationDbContext context)
        {
            var userIDs = context.Users.Select(u => u.Id);
            var allActsIDs = AllActs.Select(a => a.Id).ToList();
            foreach (var userID in userIDs)
            {
                string id = userID.ToString();
                if (_cache.Contains(id)) {
                    List<int> actIDs = (List<int>)_cache.Get(id);
                    var useracts = actIDs.Select(actId => new UserAct { ActID = actId, UserID = userID }).ToList();
                    foreach (var useract in useracts)
                    {
                        context.UserActs.AddIfNotExists(useract, ua=> (ua.UserID == useract.UserID) && (ua.ActID == useract.ActID));                    
                    }
                }
            }
            context.SaveChanges();
            AllActs = context.Acts.ToList();
        }
    }
}
