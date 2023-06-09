namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Class that repersents data that are used when calling the Recommender API
    /// </summary>
    public class RecommenderQuery
    {
        /// <summary>
        /// IDs of possible items
        /// </summary>
        public int[] WhiteListItemIDs { get; set; }

        /// <summary>
        /// IDs of items that shouldn't be returned
        /// </summary>
        public int[] BlackListItemIDs { get; set; }

        /// <summary>
        /// Number of items that should be returned
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// Metrics importance specified by user
        /// </summary>
        public int[] Metrics { get; set; }
    }
}
