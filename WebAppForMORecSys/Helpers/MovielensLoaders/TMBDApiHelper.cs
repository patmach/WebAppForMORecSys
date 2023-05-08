using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace WebAppForMORecSys.Helpers.MovielensLoaders
{
    public static class TMBDApiHelper
    {
        static readonly HttpClient httpClient = new HttpClient();
        static readonly string apiKey = File.ReadAllText("apikeyTMBD.txt");
        public static async Task<string> getMovieDetail(Link link)
        {
            string urlDetail = "https://api.themoviedb.org/3/movie/" + link.TMBDID + "?api_key=" + apiKey;
            using HttpResponseMessage httpResponse = await httpClient.GetAsync(urlDetail);
            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.Write(link.TMBDID + ex.ToString());
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return jsonResponse;
        }
        public static async Task<string> getMovieCredits(Link link)
        {
            string urlCredits = "https://api.themoviedb.org/3/movie/" + link.TMBDID + "/credits?api_key=" + apiKey;
            using HttpResponseMessage httpResponse = await httpClient.GetAsync(urlCredits);
            try
            {
                httpResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.Write(link.TMBDID + ex.ToString());
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            return jsonResponse;
        }
    }
}
