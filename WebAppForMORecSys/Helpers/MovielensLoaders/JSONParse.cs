using Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.DependencyResolver;
using NuGet.ProjectModel;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Web.Helpers;
using WebAppForMORecSys.Models;
using static System.Net.Mime.MediaTypeNames;

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

        /// <summary>
        /// Parses movie videos TMBD API response and saves the youtube key to trailer for given movie
        /// </summary>
        /// <param name="response">Response from TMBD API</param>
        /// <param name="movie">Movie to which the information should be added</param>
        public static void AddYoutubeKeyToMovie(string response, Item movie)
        {
            JsonObject jsonResponse = (JsonObject)JsonNode.Parse(response);
            JsonNode results;
            if (jsonResponse.TryGetPropertyValue("results", out results))
            {
                JsonArray jArr = results.AsArray();
                foreach (JsonNode node in jArr)
                {
                    var nodeobj = node.AsObject();
                    JsonNode site;
                    if (!nodeobj.TryGetPropertyValue("site", out site))
                        continue;
                    JsonNode type;
                    if (!nodeobj.TryGetPropertyValue("type", out type))
                        continue;
                    if ((site.ToString().ToLower() == "youtube") && (type.ToString().ToLower() == "trailer"))
                    {
                        JsonNode key;
                        if(!nodeobj.TryGetPropertyValue("key", out key))
                            continue;
                        if (key != null)
                        {
                            try
                            {
                             
                                JsonObject jparams = (JsonObject)JsonNode.Parse(movie.JSONParams);
                                jparams.Remove("YoutubeKey");
                                jparams["YoutubeKey"] = key.ToString();
                                movie.JSONParams = jparams.ToJsonString();
                                break;
                            }
                            catch (Exception e)
                            {
                                break;
                            }
                            
                        }

                    }
                }
            }
        }
            /*
        public static List<Rating> DeserializeRatings()
        {

            var ratingsFile = "Resources/genome_2021/ratings.json";
            string json = "";
            using (var reader = new StreamReader(ratingsFile))
            {
                json = reader.ReadToEnd();
            }
            List<DeserializedRating> dsRatings = JsonConvert.DeserializeObject<List<DeserializedRating>>(json);
            var ratings = dsRatings.Select(dr => new Rating
            {
                UserID = dr.user_id,
                ItemID = dr.item_id,
                RatingScore = (byte)(dr.rating * 2.0),
                Date = new DateTime(2021, 12, 1)
            }).ToList();
            return ratings;
        }

        public static List<Metadata> DeserializeMetadata()
        {

            var metadataFile = "Resources/genome_2021/metadata.json";
            string json = "";
            using (var reader = new StreamReader(metadataFile))
            {
                json = reader.ReadToEnd();
            }
            List<Metadata> metadata = JsonConvert.DeserializeObject<List<Metadata>>(json);
            return metadata;
        }
            */
     
    }
    /*
    public class Metadata
    {
        public int item_id;
        public string imdbId;
    }

    public class DeserializedRating
    {
        public int item_id;
        public int user_id;
        public double rating;
    }
    */
}
