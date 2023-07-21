namespace WebAppForMORecSys.Models.ViewModels
{
    /// <summary>
    /// View model for the user's block management page
    /// </summary>
    public class UserBlockRuleViewModel
    {
        /// <summary>
        /// Blocked items to be shown
        /// </summary>
        public List<Item> Items { get; set; }

        /// <summary>
        /// Blocks on properties. Keys - properties, Values - blocked values of each property
        /// </summary>
        public Dictionary<string, List<string>> StringPropertiesBlocks = new Dictionary<string, List<string>>();

        /// <summary>
        /// User for whom the page is loaded
        /// </summary>
        public User CurrentUser { get; set; }

        /// <summary>
        /// Searched value from user request. Will be set in the textbox (visible only if user block many items)
        /// </summary>
        public string SearchValue = "";

        /// <summary>
        /// Disabled to create new block. Only used in the preview of block addition in App Settings page.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Ratings of the user
        /// </summary>
        public List<Rating> CurrentUserRatings { get; set; }

        /// <summary>
        /// Message to be shown in alert window
        /// </summary>
        public string Message;

    }
}
