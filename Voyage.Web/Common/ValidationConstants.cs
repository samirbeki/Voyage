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
    }
}
