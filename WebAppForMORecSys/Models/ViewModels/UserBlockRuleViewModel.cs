namespace WebAppForMORecSys.Models.ViewModels
{
    public class UserBlockRuleViewModel
    {
        public List<Item> Items { get; set; }
        public Dictionary<string, List<string>> StringPropertiesBlocks = new Dictionary<string, List<string>>();
        public User CurrentUser { get; set; }
        public string SearchValue = "";

        public bool Disabled { get; set; }
        public List<Rating> CurrentUserRatings { get; set; }

    }
}
