namespace Voyage.Common.Constants.Validation
{
    public static class PackingItemValidationConstants
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;

        public const int QuantityMin = 1;
        public const int QuantityMax = 99;

        public const int MaxPerTrip = 200;

        public const int ReminderDaysBeforeStart = 7;
        public const int MaxReminders = 3;
    }
}
