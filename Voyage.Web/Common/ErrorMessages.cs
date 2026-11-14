namespace Voyage.Web.Common
{
    public static class ErrorMessages
    {
        public const string RequiredField =
            "{0} is required.";

        public const string ValueRequired =
            "This field is required.";

        public const string InvalidValue =
            "Please enter a valid value.";

        public const string StringLengthRange =
            "{0} must be between {2} and {1} characters.";

        public const string MaxLengthField =
            "{0} cannot be longer than {1} characters.";

        public const string InvalidImageUrl =
            "Enter a full link (https://...) or a path starting with /images/.";

        public const string CategoryRequired =
            "Please select a category.";

        public const string CategoryNotFound =
            "The selected category does not exist.";

        public const string DestinationExists =
            "A destination with this name already exists in this country.";

        public const string BudgetRange =
            "{0} must be between {1} and {2}.";

        public const string EndDateBeforeStartDate =
            "End date cannot be before the start date.";

        public const string TripTooLong =
            "A trip cannot be longer than 365 days.";

        public const string DestinationsRequired =
            "Select at least one destination.";

        public const string DestinationNotFound =
            "One or more of the selected destinations do not exist.";

        public const string StartDateInPast =
            "Start date cannot be in the past.";

        public const string CategoryExists =
            "A category with this name already exists.";

        public const string CategoryHasDestinations =
            "This category cannot be deleted because it has destinations.";
    }
}