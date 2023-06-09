﻿using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Web.Helpers;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// Adds new method to be called on item. Mostly setting the json properties
    /// </summary>
    public class ItemHelper
    {
        /// <summary>
        /// </summary>
        /// <param name="item">Item whose property value should be returned</param>
        /// <param name="property">Property that should be returned</param>
        /// <returns>String value of specified property from given item</returns>
        public static string getPropertyStringValueFromJSON(Item item, string property)
        {
            try
            {
                if (item.JSONParams == null) return "";
                JsonObject? Params = (JsonObject?)JsonObject.Parse(item.JSONParams);
                JsonNode jsonNode;
                if (Params != null && Params.TryGetPropertyValue(property, out jsonNode))
                {                    
                    return System.Text.RegularExpressions.Regex.Unescape(jsonNode.ToString());
                }
            }
            catch (Exception e)
            {
                var x = e.Message;
            }
            return "";
        }

        /// <summary>
        /// </summary>
        /// <param name="item">Item whose property value should be returned</param>
        /// <param name="property">Property that should be returned</param>
        /// <returns>List value of specified property from given item</returns>
        public static string[] getPropertyListValueFromJSON(Item item, string property)
        {
            string stringResult = getPropertyStringValueFromJSON(item, property);
            if (!stringResult.IsNullOrEmpty())
            {
                stringResult = stringResult.Replace("[", "").Replace("]", "").Replace("\"", "").Replace(", ", ",")
                    .Replace(" ,", ",").Replace(Environment.NewLine, "");
                var list = stringResult.Split(',').ToList();
                return list.Select(g => g.Trim()).ToArray();
            }
            return new string[0];
        }
    }
}
