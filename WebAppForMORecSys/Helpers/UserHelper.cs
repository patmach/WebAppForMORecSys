using System.Web.Helpers;
using WebAppForMORecSys.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using WebAppForMORecSys.Cache;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Helpers
{
    public static class UserHelper
    {
        private static dynamic GetBlockRuleDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.JSONBlockRules??"");
            return jsonObj;

        }

        private static dynamic GetUserChoicesDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.UserChoices ?? "");
            return jsonObj;

        }

        private static dynamic GetJSONFilterDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.JSONFilter ?? "");
            return jsonObj;

        }

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

        public static void AddStringValueToBlackList(this User user, string name, string value)
        {
            var jsonObj = AddStringValue(GetBlockRuleDynamic(user), user, name, value);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);

            BlockedItemsCache.RemoveBlockedItemIdsForUser(user.Id.ToString());
        }

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


        public static void RemoveStringValueFromBlackList(this User user, string name, string value)
        {
            var jsonObj = RemoveStringValue(GetBlockRuleDynamic(user), user, name, value);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);
            BlockedItemsCache.RemoveBlockedItemIdsForUser(user.Id.ToString());
        }

        public static void RemoveStringValueFromUserChoices(this User user, string name, string value)
        {
            var jsonObj = RemoveStringValue(GetUserChoicesDynamic(user), user, name, value);
            user.UserChoices = JsonConvert.SerializeObject(jsonObj);
        }

        public static void RemoveStringValueFromJSONFilter(this User user, string name, string value)
        {
            var jsonObj = RemoveStringValue(GetJSONFilterDynamic(user), user, name, value);
            user.JSONFilter = JsonConvert.SerializeObject(jsonObj);
        }

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

        public static List<string> GetStringValuesInBlackList(this User user, string name)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            return GetStringValues(user, name, jsonObj);
        }

        private static List<string> GetStringValuesInUserChoices(this User user, string name)
        {
            var jsonObj = GetUserChoicesDynamic(user);
            return GetStringValues(user, name, jsonObj);
        }

        private static List<string> GetStringValuesInJSONFilter(this User user, string name)
        {
            var jsonObj = GetJSONFilterDynamic(user);
            return GetStringValues(user, name, jsonObj);
        }

        private static List<string> GetStringValues(User user, string name, dynamic jsonObj) {
            if ((jsonObj == null) || (jsonObj[name] == null))
            {
                return new List<string>();
            }
            var listOfValues = ((JArray)jsonObj[name])?.ToObject<List<string>>();
            return listOfValues;
        
        }

        private static string GetStringValueInUserChoices(this User user, string name)
        {
            var jsonObj = GetUserChoicesDynamic(user);
            if ((jsonObj == null) || (jsonObj[name] == null))
            {
                return null;
            }
            return ((JToken)jsonObj[name]).ToString();
        }


        private static void SetStringValueToUserChoices(this User user, string name, string value)
        {
            var jsonObj = SetStringValue(GetUserChoicesDynamic(user), user, name, value);
            user.UserChoices = JsonConvert.SerializeObject(jsonObj);
        }

        private static void AddStringValueToUserChoices(this User user, string name, string value, bool uniqueValues = true)
        {
            var jsonObj = AddStringValue(GetUserChoicesDynamic(user), user, name, value, uniqueValues);
            user.UserChoices = JsonConvert.SerializeObject(jsonObj);
        }

        private static void AddStringValueToJSONFilter(this User user, string name, string value, bool uniqueValues = true)
        {
            var jsonObj = AddStringValue(GetJSONFilterDynamic(user), user, name, value, uniqueValues);
            user.JSONFilter = JsonConvert.SerializeObject(jsonObj);
        }

        public static void SetMetricsView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "MetricsView", value.ToString());
        }

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

        public static void SetAddBlockRuleView(this User user, int value)
        {
            SetStringValueToUserChoices(user, "AddBlockRuleView", value.ToString());
        }

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

        public static string[] GetColors(this User user)
        {
            var colors = GetStringValuesInUserChoices(user, "Colors");
            if ((colors == null) || (colors.Count == 0))
                return SystemParameters.Colors;
            return colors.ToArray(); 
        }

        public static Dictionary<Metric,string> GetMetricsToColors(this User user)
        {
            var colors = GetStringValuesInUserChoices(user, "Colors");
            if ((colors == null) || (colors.Count == 0))
                colors = SystemParameters.Colors.ToList();
            var metrics = SystemParameters.MetricsToColors.Keys.ToList();
            return Enumerable.Range(0, metrics.Count).ToDictionary(i => metrics[i], i => colors[i]);
        }

        public static string[] GetMetricsImportance(this User user)
        {
            var metricsImportance = GetStringValuesInJSONFilter(user, "metricsImportance");
            if ((metricsImportance == null) || (metricsImportance.Count == 0))
                return null;
            return metricsImportance.ToArray();
        }

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

    }
}
