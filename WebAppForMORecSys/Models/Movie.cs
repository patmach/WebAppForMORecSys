using System.Collections.Generic;
using System.Text.Json.Nodes;


namespace WebAppForMORecSys.Models
{
    public class Movie : Item
    {
        public string Director => getPropertyStringValueFromJSON("Director");
        public string Scriptwriter => getPropertyStringValueFromJSON("Scriptwriter");
        public string[] Actors => getPropertyListValueFromJSON("Actors");
        public DateTime ReleaseDate => DateTime.Parse(getPropertyStringValueFromJSON("ReleaseDate"));
        public int[] Genres => Array.ConvertAll(getPropertyListValueFromJSON("Genres"), s => int.Parse(s));


        public string getPropertyStringValueFromJSON(string property)
        {
            JsonObject? Params = (JsonObject?)JsonObject.Parse(JSONParams);
            JsonNode jsonNode; 
            if(Params != null && Params.TryGetPropertyValue(property, out jsonNode))
            {
                return jsonNode.ToString();
            }
            return null;
        }

        public string[] getPropertyListValueFromJSON(string property)
        {
            string stringResult = getPropertyStringValueFromJSON(property);
            if (stringResult !=null)
            {
                return stringResult.Split(',');
            }
            return new string[0];
        }
    }
}
