namespace Voyage.Web.Common
{
    public enum PackingResult
    {
        Success = 0,
        TripNotFound = 1,
        ItemNotFound = 2,
        Duplicate = 3,
        LimitReached = 4,
        InvalidCategory = 5
    }
}