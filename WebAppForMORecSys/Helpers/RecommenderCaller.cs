using System.Globalization;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Helpers
{
    public class RecommenderCaller
    {
        static HttpClient client = new HttpClient();
        public async static Task<Dictionary<int, int[]>> GetRecommendations(int[] whitelist, int[]blacklist, int[] metricsimportance, int userId, string rsURI)
        {
            RecommenderQuery rq = new RecommenderQuery
            {
                WhiteListItemIDs = whitelist,
                BlackListItemIDs = blacklist.ToArray(),
                Metrics = metricsimportance,
                Count = 50
            };

            JsonContent content = JsonContent.Create(rq);
            HttpResponseMessage response = await client.PostAsync($"{rsURI}getRecommendations/{userId}", content);
            Dictionary<int, int[]> recommendations = new Dictionary<int, int[]>();
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Dictionary<int, int[]>>();
            }
            else
                return new Dictionary<int, int[]>();
        }
    }
}
