using System.Globalization;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// This class contains method that calls the Recommender API
    /// </summary>
    public class RecommenderCaller
    {
        static HttpClient client = new HttpClient();

        /// <summary>
        /// Gets recommendations - Calling Recommender API
        /// </summary>
        /// <param name="whitelist">List with possible item IDs. Empty if user havent search for anything.</param>
        /// <param name="blacklist">List with IDs of blocked items by user.</param>
        /// <param name="metricsimportance">Each metrics importance. The number should be percentage</param>
        /// <param name="userId">For which user the recommendations should be</param>
        /// <param name="rsURI">URI of Recommender API</param>
        /// <returns>List of recommendations with their metrics contribution score</returns>
        public async static Task<Dictionary<int, double[]>> GetRecommendations(int[] whitelist, int[]blacklist, int[] metricsimportance, int userId, string rsURI)
        {
            RecommenderQuery rq = new RecommenderQuery
            {
                WhiteListItemIDs = whitelist,
                BlackListItemIDs = blacklist.ToArray(),
                Metrics = metricsimportance,
                Count = 20
            };

            JsonContent content = JsonContent.Create(rq);
            HttpResponseMessage response = await client.PostAsync($"{rsURI}getRecommendations/{userId}", content);
            Dictionary<int, int[]> recommendations = new Dictionary<int, int[]>();
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Dictionary<int, double[]>>();
            }
            else
                return new Dictionary<int, double[]>();
        }
    }
}
