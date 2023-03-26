namespace WebAppForMORecSys.Models.ViewModels
{
    public class UserBlockRule
    {
        public List<Item> Items { get; set; }
        public Dictionary<string, List<string>> StringPropertiesBlocks = new Dictionary<string, List<string>>();
        public User CurrentUser { get; set; }
        public string SearchValue = "";
        public List<Rating> CurrentUserRatings { get; set; }
    }
}
