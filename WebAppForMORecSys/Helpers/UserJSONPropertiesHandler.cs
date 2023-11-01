using System.Web.Helpers;
using WebAppForMORecSys.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Settings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebAppForMORecSys.Data;
using Humanizer;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// Adds new method to be called on user. Mostly setting the json properties
    /// </summary>
    public static class UserJSONPropertiesHandler
    {
        /// <summary>
        /// </summary>
        /// <param name="user">User whose JSONBlockRules value should be returned</param>
        /// <returns>Value of property JSONBlockRules</returns>
        private static dynamic GetBlockRuleDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.JSONBlockRules??"");
            return jsonObj;

        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose UserChoice value should be returned</param>
        /// <returns>>Value of property UserChoice</returns>
        private static dynamic GetUserChoicesDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.UserChoices ?? "");
            return jsonObj;

        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose JSONFilter value should be returned</param>
        /// <returns>>Value of property JSONFilter</returns>
        private static dynamic GetJSONFilterDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.JSONFilter ?? "");
            return jsonObj;

        }

        /// <summary>
        /// Adds item to block rules if it isnt present
        /// </summary>
        /// <param name="user">User who adds the item to the blocked items</param>
        /// <param name="itemId">ID of item that should be added to the blocked items</param>
        public static void AddItemToBlackList(this User user, int itemId)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if (jsonObj == null)
            {
                user.JSONBlockRules="{\"Id\":["+itemId+"]}";
                return;
            }
            var jarray = (JArray)jsonObj["Id"];
            if ((!jarray?.ToObject<List<int>>()?.Contains(itemId)) ?? false)
                ((JArray)jsonObj["Id"]).Add(itemId);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);
            BlockedItemsCache.RemoveBlockedItemIdsForUser(user.Id.ToString());
        }

        /// <summary>
        /// Adds new block rule for user
        /// </summary>
        /// <param name="user">User that adds the new block rule</param>
        /// <param name="name">Name of key in the block rule</param>
        /// <param name="value">The blocked value</param>
        public static void AddStringValueToBlackList(this User user, string name, string value)
        {
            var jsonObj = AddStringValue(GetBlockRuleDynamic(user), user, name, value);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);

            BlockedItemsCache.RemoveBlockedItemIdsForUser(user.Id.ToString());
        }

        /// <summary>
        /// Adds to the json object new value to the given key
        /// </summary>
        /// <param name="jsonObj">Current value of JSON property</param>
        /// <param name="user">User that addsnew value to one of its JSON properties</param>
        /// <param name="name">Name of key</param>
        /// <param name="value">The added value</param>
        /// <param name="uniqueValues">True - Checks if value is already present and then dont add the value. False - Add anyway</param>
        /// <returns></returns>
        public static dynamic AddStringValue(dynamic jsonObj,User user ,string name, string value, bool uniqueValues = true)
        {
            if (jsonObj == null)
            {
                jsonObj = new JObject();
            }
            if (!jsonObj.ContainsKey(name))
            {
                ((JObject)jsonObj).Add(name, new JArray());
            }
            var jarray = (JArray)jsonObj[name];
            if (!uniqueValues || ((!jarray?.ToObject<List<string>>()?.Contains(value)) ?? false))
                ((JArray)jsonObj[name]).Add(value);
            return jsonObj;
        }

        /// <summary>
        /// Sets new value to the given key in the given JSON object
        /// </summary>
        /// <param name="jsonObj">Current value of JSON property</param>
        /// <param name="user">User that sets new value to one of its JSON properties</param>
        /// <param name="name">Name of key</param>
        /// <param name="value">Value to be set</param>
        /// <returns></returns>
        private static dynamic SetStringValue(dynamic jsonObj, User user, string name, string value)
        {
            if (jsonObj == null)
            {
                jsonObj = new JObject();
            }
            if (!jsonObj.ContainsKey(name))
            {
                ((JObject)jsonObj).Add(name, value);
            }
            else 
            {
                jsonObj[name] = value;
            }
            return jsonObj;
        }

        /// <summary>
        /// Removes item to block rules if it is present
        /// </summary>
        /// <param name="user">User who removes the item to the blocked items</param>
        /// <param name="itemId">ID of item that should be removed from the blocked items</param>
        public static void RemoveItemFromBlackList(this User user, int itemId)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if ((jsonObj == null) || (jsonObj["Id"] == null))
            {
                return;
            }
            var listOfIDs = ((JArray)jsonObj["Id"])?.ToObject<List<int>>();
            if ((listOfIDs?.Contains(itemId)) ?? false)
                listOfIDs.Remove(itemId);
            else return;
            jsonObj["Id"] = JArray.FromObject(listOfIDs);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);
            BlockedItemsCache.RemoveBlockedItemIdsForUser(user.Id.ToString());
        }

        /// <summary>
        /// Removes block rule for user
        /// </summary>
        /// <param name="user">User that removes the block rule</param>
        /// <param name="name">Name of key in the block rule</param>
        /// <param name="value">The unblocked value</param>
        public static void RemoveStringValueFromBlackList(this User user, string name, string value)
        {
            var jsonObj = RemoveStringValue(GetBlockRuleDynamic(user), user, name, value);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);
            BlockedItemsCache.RemoveBlockedItemIdsForUser(user.Id.ToString());
        }

        /// <summary>
        /// Removes set user choice
        /// </summary>
        /// <param name="user">User from whom his set choice should be removed</param>
        /// <param name="name">Name of key in UserChoice</param>
        /// <param name="value">The value to be removed</param>
        public static void RemoveStringValueFromUserChoices(this User user, string name, string value)
        {
            var jsonObj = RemoveStringValue(GetUserChoicesDynamic(user), user, name, value);
            user.UserChoices = JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary>
        /// Removes saved filter
        /// </summary>
        /// <param name="user">User from whom his saved filter should be removed</param>
        /// <param name="name">Name of key in JSONFIlter</param>
        /// <param name="value">should be removedalue to be removed</param>
        public static void RemoveStringValueFromJSONFilter(this User user, string name, string value)
        {
            var jsonObj = RemoveStringValue(GetJSONFilterDynamic(user), user, name, value);
            user.JSONFilter = JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary>
        /// Removes value of the given key from the json object
        /// </summary>
        /// <param name="jsonObj">Current value of JSON property</param>
        /// <param name="user">User from whom his saved value in one of its JSON properties should be removed</param>
        /// <param name="name">Name of key</param>
        /// <param name="value">Value to be removed</param>
        /// <returns></returns>
        private static dynamic RemoveStringValue(dynamic jsonObj, User user, string name, string value)
        {
            if ((jsonObj == null) || (!jsonObj.ContainsKey(name)))
            {
                return jsonObj;
            }
            var listOfValues = ((JArray)jsonObj[name])?.ToObject<List<string>>();
            if ((listOfValues?.Contains(value)) ?? false)
                listOfValues.Remove(value);
            jsonObj[name] = JArray.FromObject(listOfValues);
            return jsonObj;
        }

        /// <summary>
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <param name="itemId">ID of checked item</param>
        /// <returns>Is item blocked by user?</returns>
        public static bool IsItemInBlackList(this User user, int itemId)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if ((jsonObj == null)||(jsonObj["Id"]==null))
            {
                return false;
            }
            var listOfIDs = ((JArray)jsonObj["Id"])?.ToObject<List<int>>();
            if ((listOfIDs?.Contains(itemId)) ?? false)
                return true;
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="user">Checked user</param>
        /// <param name="name">Checked key</param>
        /// <param name="value":Checked value</param>
        /// <returns>Is value of the given key blocked by user?</returns>
        public static bool IsStringValueInBlackList(this User user, string name, string value)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if ((jsonObj == null) || (jsonObj[name] == null))
            {
                return false;
            }
            var listOfValues = ((JArray)jsonObj[name])?.ToObject<List<string>>();
            if ((listOfValues?.Contains(value)) ?? false)
                return true;
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose blocked items should be returned</param>
        /// <returns>IDs of items blocked by user</returns>
        public static List<int> GetItemsInBlackList(this User user)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if ((jsonObj == null) || (jsonObj["Id"] == null))
            {
                return new List<int>();
            }
            var listOfIDs = ((JArray)jsonObj["Id"])?.ToObject<List<int>>();
            return listOfIDs;
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose blocked values should be returned</param>
        /// <param name="name">Name of checked key</param>
        /// <returns>Values of given key blocked by user</returns>
        public static List<string> GetStringValuesInBlackList(this User user, string name)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            return GetStringValues(user, name, jsonObj);
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose choices should be returned</param>
        /// <param name="name">Name of checked key</param>
        /// <returns>Chosen values by user</returns>
        private static List<string> GetStringValuesInUserChoices(this User user, string name)
        {
            var jsonObj = GetUserChoicesDynamic(user);
            return GetStringValues(user, name, jsonObj);
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose filter settings should be returned</param>
        /// <param name="name">Name of checked key</param>
        /// <returns>Users filter settings for given key</returns>
        private static List<string> GetStringValuesInJSONFilter(this User user, string name)
        {
            var jsonObj = GetJSONFilterDynamic(user);
            return GetStringValues(user, name, jsonObj);
        }

        /// <summary>
        /// Gets all values of the given key in the given JSON object
        /// </summary>
        /// <param name="jsonObj">Current value of JSON property</param>
        /// <param name="user">User whose set values should be returned</param>
        /// <param name="name">Name of key</param>
        /// <returns>Values of given key from the given JSON property</returns>
        private static List<string> GetStringValues(User user, string name, dynamic jsonObj) {
            if ((jsonObj == null) || (jsonObj[name] == null))
            {
                return new List<string>();
            }
            var listOfValues = ((JArray)jsonObj[name])?.ToObject<List<string>>();
            return listOfValues;
        
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose choice should be returned</param>
        /// <param name="name">Name of checked key</param>
        /// <returns>Chosen value by user</returns>
        private static string GetStringValueInUserChoices(this User user, string name)
        {
            var jsonObj = GetUserChoicesDynamic(user);
            if ((jsonObj == null) || (jsonObj[name] == null))
            {
                return null;
            }
            return ((JToken)jsonObj[name]).ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User who sets new choice value</param>
        /// <param name="name">Name of checked key to which the new value belongs</param>
        /// <param name="value">Value to be saves</param>

        private static void SetStringValueToUserChoices(this User user, string name, string value)
        {
            var jsonObj = SetStringValue(GetUserChoicesDynamic(user), user, name, value);
            user.UserChoices = JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User who adds new choice value</param>
        /// <param name="name">Name of checked key to which the new values belongs</param>
        /// <param name="value">Value to be saved</param>
        /// <param name="uniqueValues">True - Checks if value is already present and then dont add the value. False - Add anyway</param>
        private static void AddStringValueToUserChoices(this User user, string name, string value, bool uniqueValues = true)
        {
            var jsonObj = AddStringValue(GetUserChoicesDynamic(user), user, name, value, uniqueValues);
            user.UserChoices = JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User who adds new filter settings</param>
        /// <param name="name">Name of checked key to which the new filter settings value belongs</param>
        /// <param name="value">Value to be saved</param>
        /// <param name="uniqueValues">True - Checks if value is already present and then dont add the value. False - Add anyway</param>
        private static void AddStringValueToJSONFilter(this User user, string name, string value, bool uniqueValues = true)
        {
            var jsonObj = AddStringValue(GetJSONFilterDynamic(user), user, name, value, uniqueValues);
            user.JSONFilter = JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary>
        /// Sets chosen metrics view value
        /// </summary>
        /// <param name="user">User that sets the value</param>
        /// <param name="value">Value to be saved</param>
        public static void SetMetricsView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "MetricsView", value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved value should be returned</param>
        /// <returns>Saved type of metrics view</returns>
        public static MetricsView GetMetricsView(this User user)
        {
            var metricsview = GetStringValueInUserChoices(user, "MetricsView");
            int value;
            if ((metricsview == null) || !int.TryParse(metricsview, out value))
            {
                return SystemParameters.MetricsView;
            }
            else
            {
                return (MetricsView)value;
            }
        }

        /// <summary>
        /// Sets chosen explanation view value
        /// </summary>
        /// <param name="user">User that sets the value</param>
        /// <param name="value">Value to be saved</param>
        public static void SetExplanationView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "ExplanationView", value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved value should be returned</param>
        /// <returns>Saved type of explanation view </returns>
        public static ExplanationView GetExplanationView(this User user)
        {
            var Explanationview = GetStringValueInUserChoices(user, "ExplanationView");
            int value;
            if ((Explanationview == null) || !int.TryParse(Explanationview, out value))
            {
                return SystemParameters.ExplanationView;
            }
            else
            {
                return (ExplanationView)value;
            }
        }

        /// <summary>
        /// Sets chosen metric contribution score view view value
        /// </summary>
        /// <param name="user">User that sets the value</param>
        /// <param name="value">Value to be saved</param>
        public static void SetMetricContributionScoreView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "MetricContributionScoreView", value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved value should be returned</param>
        /// <returns>Saved type of metric contribution score view</returns>
        public static MetricContributionScoreView GetMetricContributionScoreView(this User user)
        {
            var MetricContributionScoreView = GetStringValueInUserChoices(user, "MetricContributionScoreView");
            int value;
            if ((MetricContributionScoreView == null) || !int.TryParse(MetricContributionScoreView, out value))
            {
                return SystemParameters.MetricContributionScoreView;
            }
            else
            {
                return (MetricContributionScoreView)value;
            }
        }


        /// <summary>
        /// Sets chosen preview explanation view
        /// </summary>
        /// <param name="user">User that sets the value</param>
        /// <param name="value">Value to be saved</param>
        public static void SetPreviewExplanationView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "PreviewExplanationView", value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved value should be returned</param>
        /// <returns>Saved type of preview explanation view</returns>
        public static PreviewExplanationView GetPreviewExplanationView(this User user)
        {
            var PreviewExplanationView = GetStringValueInUserChoices(user, "PreviewExplanationView");
            int value;
            if ((PreviewExplanationView == null) || !int.TryParse(PreviewExplanationView, out value))
            {
                return SystemParameters.PreviewExplanationView;
            }
            else
            {
                return (PreviewExplanationView)value;
            }
        }

        /// <summary>
        /// Sets chosen type of block rule addition
        /// </summary>
        /// <param name="user">User that sets the value</param>
        /// <param name="value">Value to be saved</param>
        public static void SetAddBlockRuleView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "AddBlockRuleView", value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved value should be returned</param>
        /// <returns>Saved type of block rule addition</returns>
        public static AddBlockRuleView GetAddBlockRuleView(this User user)
        {
            var addBlockRuleView = GetStringValueInUserChoices(user, "AddBlockRuleView");
            int value;
            if ((addBlockRuleView == null) || !int.TryParse(addBlockRuleView, out value))
            {
                return SystemParameters.AddBlockRuleView;
            }
            else
            {
                return (AddBlockRuleView)value;
            }
        }

        /// <summary>
        /// Sets chosen colors for metrics
        /// </summary>
        /// <param name="user">User that sets the values</param>
        /// <param name="value">Values to be saved</param>
        public static void SetColors(this User user, string[] value)
        {
            foreach (var color in GetStringValuesInUserChoices(user, "Colors")) {
                RemoveStringValueFromUserChoices(user, "Colors", color);
            }
            foreach (var color in value)
            {
                AddStringValueToUserChoices(user, "Colors", color, false);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved values should be returned</param>
        /// <returns>Chosen colors for metrics</returns>
        public static string[] GetColors(this User user)
        {
            var colors = GetStringValuesInUserChoices(user, "Colors");
            if ((colors == null) || (colors.Count == 0))
                return SystemParameters.Colors;
            for (int i = colors.Count; i < SystemParameters.Colors.Length; i++)
            {
                colors.Add(SystemParameters.Colors[i]);
            }
            return colors.ToArray(); 
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved color values should be mapped to metrics</param>
        /// <returns>Metrics mapped on chosen colors</returns>
        public static Dictionary<int,string> GetMetricIDsToColors(this User user)
        {
            var colors = GetColors(user);
            var metrics = SystemParameters.MetricsToColors.Keys.Select(metric => metric.Id).ToList();
            return Enumerable.Range(0, metrics.Count).ToDictionary(i => metrics[i], i => colors[i]);
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose saved value to JSONFilter propert should be returned</param>
        /// <returns>Saved value of metrics importance in JSONFilter property</returns>
        public static string[] GetMetricsImportance(this User user)
        {
            var metricsImportance = GetStringValuesInJSONFilter(user, "metricsImportance");
            if ((metricsImportance == null) || (metricsImportance.Count == 0))
                return null;
            return metricsImportance.ToArray();
        }

        /// <summary>
        /// Saves metrics importance value to JSONFilter property
        /// </summary>
        /// <param name="user">User for which the values should be saved</param>
        /// <param name="value">Values to be saved</param>
        public static void SetMetricsImportance(this User user, string[] value)
        {
            foreach (var metricImportance in GetStringValuesInJSONFilter(user, "metricsImportance"))
            {
                RemoveStringValueFromJSONFilter(user, "metricsImportance", metricImportance);
            }
            foreach (var metricImportance in value)
            {
                AddStringValueToJSONFilter(user, "metricsImportance", metricImportance, false);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose rated and seen items should be returned</param>
        /// <param name="context">Databse context</param>
        /// <returns>Rated and seen items by user</returns>
        public static List<int> GetRatedAndSeenItems(this User user, ApplicationDbContext context)
        {
            var rated = context.Ratings.Where(r => r.UserID == user.Id).Select(r => r.ItemID).ToList();
            var seen = context.Interactions.Where(i => (i.type == TypeOfInteraction.Click && i.Last > DateTime.Now.AddMinutes(-15))
            || (i.type == TypeOfInteraction.Seen && (i.Last > DateTime.Now.AddMinutes(-5) || i.NumberOfInteractions >= 3)))
                .Select(r => r.ItemID).ToList();
            return rated.Union(seen).ToList();
        }

        /// <summary>
        /// Sets ID of last question user saw in the formular
        /// </summary>
        /// <param name="user">User that sees the question</param>
        /// <param name="value">Value to be saved</param>
        public static void SetLastSectionID(this User user, int value)
        {
            SetStringValueToUserChoices(user, "LastSectionID", value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User that saw this question as last one</param>
        /// <returns>Last question user saw in the formular</returns>
        public static int GetLastSectionID(this User user)
        {
            var lastQuestionID = GetStringValueInUserChoices(user, "LastSectionID");
            int value;
            if ((lastQuestionID == null) || !int.TryParse(lastQuestionID, out value))
            {
                return -1;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose codes of selected metric variants should be returned</param>
        /// <param name="context">Database context</param>
        /// <param name="metricIDs">IDs of metrics whose variants codes should be returned</param>
        /// <returns>Codes of variants of metrics selected by user</returns>
        public static string[] GetMetricVariantCodes(this User user, ApplicationDbContext context, List<int> metricIDs)
        {
            return GetMetricVariants(user, context, metricIDs).Select(mv=> mv?.Code ?? "").ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="user">User whose selected metric variants should be returned</param>
        /// <param name="context">Database context</param>
        /// <param name="metricIDs">IDs of metrics whose variants should be returned</param>
        /// <returns>Variants of metrics selected by user</returns>
        public static List<MetricVariant> GetMetricVariants(this User user, ApplicationDbContext context, List<int> metricIDs)
        {
            var variants = new List<MetricVariant>();
            var defaultMVs = context.MetricVariants.Where(mv => metricIDs.Contains(mv.MetricID) && mv.DefaultVariant)
                .ToList();
            var userMVs = context.UserMetricVariants.Include(umv => umv.MetricVariant).
                Where(umv => (umv.UserID == user.Id) && metricIDs.Contains(umv.MetricVariant.MetricID))
                .Select(umv => umv.MetricVariant).ToList();
            foreach (int metricID in metricIDs)
            {
                var userChoice = userMVs.Where(umv => umv.MetricID == metricID).FirstOrDefault();
                variants.Add(userChoice ?? defaultMVs.Where(mv => mv.MetricID == metricID).FirstOrDefault() ?? null);
            }
            return variants;
        }


        /// <summary>
        /// Instance of random, used by SetRandomSettingsForNewUser
        /// </summary>
        private static Random rnd = new Random();

        /// <summary>
        /// Chooses random values od configurable user settings
        /// </summary>
        /// <param name="user">Newly created user</param>
        public static void SetRandomSettingsForNewUser(this User user, ApplicationDbContext context)
        {
            int addBlockRuleView = rnd.Next(Enum.GetValues(typeof(AddBlockRuleView)).Length);
            user.SetAddBlockRuleView(addBlockRuleView);
            int explanationView = rnd.Next(Enum.GetValues(typeof(ExplanationView)).Length);
            user.SetExplanationView(explanationView);
            int metricContributionScoreView = rnd.Next(Enum.GetValues(typeof(MetricContributionScoreView)).Length);
            user.SetMetricContributionScoreView(metricContributionScoreView);
            int previewExplanationView = rnd.Next(Enum.GetValues(typeof(PreviewExplanationView)).Length);
            user.SetPreviewExplanationView(previewExplanationView);
            int metricsView = rnd.Next(Enum.GetValues(typeof(MetricsView)).Length);
            user.SetMetricsView(metricsView);
            context.Add(user);
            context.SaveChanges();
            UserActCache.AddActs(user.Id.ToString(),
                new List<string>
                {
                    ((AddBlockRuleView)addBlockRuleView).ToString(),
                    ((ExplanationView)explanationView).ToString(),
                    ((MetricContributionScoreView)metricContributionScoreView).ToString(),
                    ((PreviewExplanationView)previewExplanationView).ToString(),
                    ((MetricsView)metricsView).ToString()
                },
                context);
        }

    }
}
