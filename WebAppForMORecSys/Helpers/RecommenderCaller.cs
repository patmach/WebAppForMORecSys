using System.Globalization;
using WebAppForMORecSys.Models;
using WebAppForMORecSys.Models.ViewModels;
using WebAppForMORecSys.Settings;

namespace WebAppForMORecSys.Helpers
{
    /// <summary>
    /// This class contains method that calls the Recommender API
    /// </summary>
    public class RecommenderCaller
    {
        /// <summary>
        /// Http client used for sending requests
        /// </summary>
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
        public async static Task<Dictionary<int, double[]>> GetRecommendations(int[] whitelist, int[]blacklist,
            int[] metricsimportance, int userId, string rsURI, string[] metricVariantsCodes, int[] currentList)
        {
            RecommenderQuery rq = new RecommenderQuery
            {
                WhiteListItemIDs = whitelist,
                BlackListItemIDs = blacklist.ToArray(),
                Metrics = metricsimportance,
                Count = SystemParameters.LengthOfRecommendationsList,
                MetricVariantsCodes = metricVariantsCodes,
                CurrentListItemIDs = currentList
            };

            JsonContent content = JsonContent.Create(rq);
#if DEBUG
            rsURI = "http://127.0.0.1:5000/";
#endif
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
