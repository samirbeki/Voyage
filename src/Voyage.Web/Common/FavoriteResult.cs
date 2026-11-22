namespace Voyage.Web.Common
{
    public enum FavoriteResult
    {
        Success = 0,
        DestinationNotFound = 1,
        AlreadyExists = 2,
        NotInFavorites = 3,
        LimitReached = 4
    }
}