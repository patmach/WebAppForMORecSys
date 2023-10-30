namespace WebAppForMORecSys.Settings
{
    /// <summary>
    /// Possibilities on how is explanation seen in preview of Item
    /// </summary>
    public enum PreviewExplanationView
    {     
        FullBorderImage,
        LeftBorderImage,
        TitleColor
    }

    public static class PreviewExplanationViewExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="previewExplanationView"></param>
        /// <returns>Name to display for the enum value</returns>
        public static string ToFriendlyString(this PreviewExplanationView previewExplanationView)
        {
            switch (previewExplanationView)
            {
                case PreviewExplanationView.FullBorderImage:
                    return "Share of metrics on full border";
                case PreviewExplanationView.LeftBorderImage:
                    return "Share of metrics only on left side";
                case PreviewExplanationView.TitleColor:
                    return "Title in best scoring metric color";
            }
            return "";
        }
    }

}


