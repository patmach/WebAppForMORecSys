using System.Web.Helpers;
using WebAppForMORecSys.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace WebAppForMORecSys.Helpers
{
    public static class UserHelper
    {
        public static dynamic GetBlockRuleDynamic(User user)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(user.JSONBlockRules??"");
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
        }

        public static void AddStringValueToBlackList(this User user, string name, string value)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if (jsonObj == null)
            {
                user.JSONBlockRules = "{\"" + name + "\":[\"" + value + "\"]}";
                return;
            }
            if (!jsonObj.ContainsKey(name))
            {
                ((JObject)jsonObj).Add(name, new JArray());
            }
            var jarray = (JArray)jsonObj[name];
            if ((!jarray?.ToObject<List<string>>()?.Contains(value)) ?? false)
                ((JArray)jsonObj[name]).Add(value);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);
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
        }


        public static void RemoveStringValueFromBlackList(this User user, string name, string value)
        {
            var jsonObj = GetBlockRuleDynamic(user);
            if ((jsonObj == null) || (!jsonObj.ContainsKey(name)))
            {
                return;
            }
            var listOfValues = ((JArray)jsonObj[name])?.ToObject<List<string>>();
            if ((listOfValues?.Contains(value)) ?? false)
                listOfValues.Remove(value);
            jsonObj[name] = JArray.FromObject(listOfValues);
            user.JSONBlockRules = JsonConvert.SerializeObject(jsonObj);

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
            if ((jsonObj == null) || (jsonObj[name] == null))
            {
                return new List<string>();
            }
            var listOfValues = ((JArray)jsonObj[name])?.ToObject<List<string>>();
            return listOfValues;
        }


    }
}
