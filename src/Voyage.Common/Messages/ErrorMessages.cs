namespace Voyage.Common.Messages
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

        public const string DecimalFormat =
            "Use a dot for decimals, e.g. 12.50.";

        public const string TripBudgetRange =
            "Budget must be between {0} and {1}.";

        public const string TripBudgetDecimals =
            "Budget can have at most 2 decimal places.";

        public const string BudgetRange =
            "{0} must be between {1} and {2}.";

        public const string EndDateBeforeStartDate =
            "End date cannot be before the start date.";

        public const string TripTooLong =
            "A trip cannot be longer than {0} days.";

        public const string DestinationsRequired =
            "Select at least one destination.";

        public const string DestinationNotFound =
            "One or more of the selected destinations do not exist.";

        public const string StartDateInPast =
            "Start date cannot be in the past.";

        public const string TripNotFound =
            "The trip does not exist.";

        public const string CategoryExists =
            "A category with this name already exists.";

        public const string CategoryHasDestinations =
            "This category cannot be deleted because it has destinations.";

        public const string ExpenseTitleLength =
            "Title must be between {2} and {1} characters.";

        public const string ExpenseAmountRange =
            "Amount must be between {1} and {2}.";

        public const string ExpenseDateOutsideTrip =
            "The date must be between the start and end date of the trip.";

        public const string ExpenseAmountDecimals =
            "Amount can have at most 2 decimal places.";

        public const string ExpenseAmountFormat = DecimalFormat;

        public const string TripDatesExcludeExpenses =
            "Some expenses fall outside these dates. Edit or delete them first.";

        public const string ReviewRatingRange =
            "Rating must be between {1} and {2}.";

        public const string ReviewCommentLength =
            "Comment must be between {2} and {1} characters.";

        public const string ReviewNoLongerExists =
            "This review no longer exists. It may have been deleted already.";

        public const string ReviewRatingRequired =
            "Please choose a rating from 1 to 5 stars.";

        public const string ReviewAlreadyExists =
            "You have already reviewed this destination.";

        public const string ReviewCouldNotBeSaved =
            "Your review could not be saved. Please check the form.";

        public const string FavoriteAlreadyExists =
            "This destination is already in your favorites.";

        public const string FavoriteNotFound =
            "This destination is not in your favorites.";

        public const string FavoritesLimitReached =
            "You can save up to {0} favorite destinations. Remove one to add another.";

        public const string PackingItemNameLength =
            "Item name must be between {2} and {1} characters.";

        public const string PackingItemQuantityRange =
            "Quantity must be between {1} and {2}.";

        public const string PackingItemCategoryInvalid =
            "Please select a valid category.";

        public const string PackingItemDuplicate =
            "This item is already in your packing list.";

        public const string PackingItemsLimitReached =
            "A trip can have up to {0} packing items.";

        public const string PackingItemNotFound =
            "The packing item does not exist.";

        public const string PackingItemNameRequired =
            "Item name is required.";

        public const string EssentialsPartiallyAdded =
            "{0} item(s) added. {1} could not be added because a trip can have up to {2} items.";

        public const string PackingConcurrentChange =
            "Your packing list was changed at the same time. Please try again.";

        public const string TravelExpertChangeFailed =
            "The role could not be changed. Please refresh the page and try again.";

        public const string ExpertTipContentLength =
            "Your tip must be between {2} and {1} characters.";

        public const string ExpertTipAlreadyExists =
            "You have already shared a tip for this destination. You can edit it instead.";

        public const string ExpertTipCouldNotBeSaved =
            "Your tip could not be saved. Please check the text and try again.";

        public const string ExpertTipNoLongerExists =
            "This expert tip no longer exists.";
    }
}
