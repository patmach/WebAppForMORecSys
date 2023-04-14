namespace WebAppForMORecSys.Models
{
    public class RecommenderQuery
    {
        public int[] PossibleItems { get; set; }
        public int? Count { get; set; }
        public int[] Metrics { get; set; }
    }
}
