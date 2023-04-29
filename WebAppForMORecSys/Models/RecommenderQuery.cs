namespace WebAppForMORecSys.Models
{
    public class RecommenderQuery
    {
        public int[] WhiteListItemIDs { get; set; }
        public int[] BlackListItemIDs { get; set; }
        public int? Count { get; set; }
        public int[] Metrics { get; set; }
    }
}
