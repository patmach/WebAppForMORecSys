using Azure;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    /// <summary>
    /// Extension class that allows to filter inside JSON Array
    /// </summary>
    public static class JArrayExtensions
    {
        /// <summary>
        /// Gets the sub-JSON array from JSON array array according to the searched fields
        /// </summary>
        /// <param name="array">JSON Array to be searched</param>
        /// <param name="field">Field of JSON Array that should be searched</param>
        /// <param name="subfield">Subfield of field containing another JSON Array</param>
        /// <returns>The sub-JSON array from JSON array array</returns>
        public static JArray Filter(this JArray array, string field, string subfield)
            => new JArray(array.Children().Where(GenerateFilter(field, subfield)));

        /// <summary>
        /// </summary>
        /// <param name="field">Field of JSON Array that should be searched</param>
        /// <param name="subfield">Subfield of field containing another JSON Array</param>
        /// <returns>Filter that will find sub-array under field and subfield</returns>
        private static Func<JToken, bool> GenerateFilter(string field, string subfield)
            => (token) => string.Equals(token[field].Value<string>(), subfield, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Class that parses responses from TMBD API
    /// </summary>
    public static class JSONParse
    {
        /// <summary>
        /// Parses movie details TMBD API response and saves the info to given movie
        /// </summary>
        /// <param name="response">Response from TMBD API</param>
        /// <param name="movie">Movie to which the information should be added</param>
        public static void AddDetailsToMovie(string response, Item movie)
        {
            JsonObject jsonResponse = (JsonObject)JsonNode.Parse(response);
            JsonNode overview;
            JsonNode image;
            JsonNode releaseDate;
            jsonResponse.TryGetPropertyValue("overview", out overview);
            jsonResponse.TryGetPropertyValue("poster_path", out image);
            jsonResponse.TryGetPropertyValue("release_date", out releaseDate);
            movie.ShortDescription = overview?.ToString();
            if (image != null)
                movie.ImageURL = "https://image.tmdb.org/t/p/w500/" + image.ToString();
            if (releaseDate != null)
                movie.JSONParams = movie.JSONParams + ",\n\"ReleaseDate\":\"" + releaseDate + '"';

        }

        /// <summary>
        /// Parses movie credits TMBD API response and saves the info to given movie
        /// </summary>
        /// <param name="response">Response from TMBD API</param>
        /// <param name="movie">Movie to which the information should be added</param>
        public static void AddCastToMovie(string response, Item movie)
        {
            JsonObject jsonResponse = (JsonObject)JsonNode.Parse(response);
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
                    movie.JSONParams = movie.JSONParams + ",\n\"Director\":\"" + director.Replace("\"", "") + '"';
            }
            if (cast != null)
            {
                JArray castArr = JArray.Parse(cast.ToString());
                var castNames = castArr.Select(s => s.GetValue<string>("name"));
                StringBuilder castNamesSB = new StringBuilder();
                foreach (var castName in castNames)
                {
                    castNamesSB.Append("\"");
                    castNamesSB.Append(castName.Replace("\"", ""));
                    castNamesSB.Append("\",");
                }
                if (castNamesSB.Length > 0)
                {
                    castNamesSB.Length--;
                }
                movie.JSONParams = movie.JSONParams + ",\n\"Actors\":[" + castNamesSB.ToString() + "]";
            }

        }


    }
}
