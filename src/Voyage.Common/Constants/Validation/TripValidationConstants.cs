namespace Voyage.Common.Constants.Validation
{
    public static class TripValidationConstants
    {
        public const int TitleMinLength = 3;
        public const int TitleMaxLength = 100;
        public const int DescriptionMaxLength = 2000;

        public const string BudgetMinValue = "0.01";
        public const string BudgetMaxValue = "1000000";
        public const int BudgetWarningPercent = 90;

        public const int MaxDays = 365;
    }
}
