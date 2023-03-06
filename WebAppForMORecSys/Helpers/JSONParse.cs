using Azure;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.ParseHelpers
{
    public static class JArrayExtensions
    {
        public static JArray Filter(this JArray array, string field, string value)
            => new JArray(array.Children().Where(GenerateFilter(field, value)));

        private static Func<JToken, bool> GenerateFilter(string field, string value)
            => (JToken token) => string.Equals(token[field].Value<string>(), value, StringComparison.OrdinalIgnoreCase);
    }
    public static class JSONParse
    {
        public static void AddDetailsToMovie(string response, Item movie)
        {
            JsonObject jsonResponse = (JsonObject)JsonObject.Parse(response);
            JsonNode overview;
            JsonNode image;
            JsonNode releaseDate;
            jsonResponse.TryGetPropertyValue("overview", out overview);
            jsonResponse.TryGetPropertyValue("poster_path", out image);
            jsonResponse.TryGetPropertyValue("release_date", out releaseDate);
            movie.ShortDescription = overview?.ToString();
            if (image!= null)
                movie.ImageURL = "https://image.tmdb.org/t/p/w500/" + image.ToString();
            if (releaseDate!= null) 
                movie.JSONParams = movie.JSONParams + ",\n\"ReleaseDate\":\"" + releaseDate + '"';

        }
        public static void AddCastToMovie(string response, Item movie)
        {
            JsonObject jsonResponse = (JsonObject)JsonObject.Parse(response);
            JsonNode crew;
            jsonResponse.TryGetPropertyValue("crew", out crew);
            JsonNode cast;
            jsonResponse.TryGetPropertyValue("cast", out cast);
            if (crew != null)
            {
                var crewString = crew.ToString();
                JArray crewArr = JArray.Parse(crewString);
                JArray directorObject = crewArr.Filter("job", "Director");
                string director = directorObject?.First?.GetValue<string>("name");
                if (director != null)
                    movie.JSONParams = movie.JSONParams + (",\n\"Director\":\"" + director.Replace("\"","") + '"');                
            }
            if(cast!= null)
            {
                JArray castArr = JArray.Parse(cast.ToString());
                var castNames = castArr.Select(s => s.GetValue<string>("name"));
                StringBuilder castNamesSB = new StringBuilder();
                foreach (var castName in castNames)
                {
                    castNamesSB.Append("\"");
                    castNamesSB.Append(castName.Replace("\"",""));
                    castNamesSB.Append("\",");
                }
                if (castNamesSB.Length > 0)
                {
                    castNamesSB.Length--;
                }
                movie.JSONParams = movie.JSONParams + (",\n\"Actors\":[" + castNamesSB.ToString()+ "]");
            }

        }
       

    }
}
