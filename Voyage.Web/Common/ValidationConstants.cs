namespace Voyage.Web.Common
{
    public static class ValidationConstants
    {
        public const int CategoryNameMinLength = 2;
        public const int CategoryNameMaxLength = 50;
        public const int CategoryDescriptionMaxLength = 500;

        public const int DestinationNameMinLength = 2;
        public const int DestinationNameMaxLength = 100;

        public const int DestinationCountryMinLength = 2;
        public const int DestinationCountryMaxLength = 60;

        public const int DestinationDescriptionMinLength = 10;
        public const int DestinationDescriptionMaxLength = 2000;

        public const int ImageUrlMaxLength = 2048;

        public const int TripTitleMinLength = 3;
        public const int TripTitleMaxLength = 100;
        public const int TripDescriptionMaxLength = 2000;

        public const string TripBudgetMinValue = "0.01";
        public const string TripBudgetMaxValue = "1000000";
        public const int TripMaxDays = 365;

        public const int ExpenseTitleMinLength = 2;
        public const int ExpenseTitleMaxLength = 100;
        public const string ExpenseAmountMin = "0.01";
        public const string ExpenseAmountMax = "1000000";

        public const int ReviewRatingMin = 1;
        public const int ReviewRatingMax = 5;
        public const int ReviewCommentMinLength = 10;
        public const int ReviewCommentMaxLength = 1000;

        public const int ModerationReviewsPageSize = 10;

        public const int FavoritesMaxPerUser = 100;
        public const int FavoritesPageSize = 9;

        public const int SearchMaxLength = 100;
    }
}