namespace Voyage.Web.Common
{
    public static class FavoriteTexts
    {
        public static string SavedBy(int count) => count switch
        {
            0 => "Be the first to save this destination",
            1 => "Saved by 1 traveler",
            _ => $"Saved by {count} travelers"
        };
    }
}