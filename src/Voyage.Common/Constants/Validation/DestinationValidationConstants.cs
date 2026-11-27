namespace Voyage.Common.Constants.Validation
{
    public static class DestinationValidationConstants
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;

        public const int CountryMinLength = 2;
        public const int CountryMaxLength = 60;

        public const int DescriptionMinLength = 10;
        public const int DescriptionMaxLength = 2000;

        public const double LatitudeMin = -90;
        public const double LatitudeMax = 90;

        public const double LongitudeMin = -180;
        public const double LongitudeMax = 180;
    }
}
