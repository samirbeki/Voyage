namespace Voyage.Web.Common
{
    public static class ErrorMessages
    {
        public const string RequiredField =
            "{0} is required.";

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
    }
}
