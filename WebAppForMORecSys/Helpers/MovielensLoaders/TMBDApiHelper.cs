using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    /// <summary>
    /// Calls TMBD API and returns its responses
    /// </summary>
    public static class TMBDApiHelper
    {
        /// <summary>
        /// Instance of class that provides HTTP communication
        /// </summary>
        static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Api key of this app saved in file
        /// </summary>
        static readonly string apiKey = File.ReadAllText("apikeyTMBD.txt");

        /// <summary>
        /// Calls API for movie detail
        /// </summary>
        /// <param name="TMBDID">ID of movie in TMBD </param>
        /// <returns>API response with movie detail</returns>
        public static async Task<string> getMovieDetail(string TMBDID)
        {
            string urlDetail = "https://api.themoviedb.org/3/movie/" + TMBDID + "?api_key=" + apiKey;
            using HttpResponseMessage httpResponse = await httpClient.GetAsync(urlDetail);
            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.Write(TMBDID + ex.ToString());
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        /// <summary>
        /// Calls API for movie credits 
        /// </summary>
        /// <param name="TMBDID">ID of movie in TMBD </param>
        /// <returns>API response with movie credits</returns>
        public static async Task<string> getMovieCredits(string TMBDID)
        {
            string urlCredits = "https://api.themoviedb.org/3/movie/" + TMBDID + "/credits?api_key=" + apiKey;
            using HttpResponseMessage httpResponse = await httpClient.GetAsync(urlCredits);
            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.Write(TMBDID + ex.ToString());
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return jsonResponse;
        }
    }
}
