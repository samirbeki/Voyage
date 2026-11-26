namespace Voyage.Services.Helpers
{
    public static class WeatherCodes
    {
        public static (string Description, string Icon) Describe(int code)
        {
            return code switch
            {
                0 => ("Clear sky", "bi-sun"),
                1 => ("Mainly clear", "bi-sun"),
                2 => ("Partly cloudy", "bi-cloud-sun"),
                3 => ("Overcast", "bi-clouds"),
                45 or 48 => ("Fog", "bi-cloud-fog"),
                51 or 53 or 55 => ("Drizzle", "bi-cloud-drizzle"),
                56 or 57 => ("Freezing drizzle", "bi-cloud-sleet"),
                61 or 63 => ("Rain", "bi-cloud-rain"),
                65 => ("Heavy rain", "bi-cloud-rain-heavy"),
                66 or 67 => ("Freezing rain", "bi-cloud-sleet"),
                71 or 73 or 75 or 77 => ("Snow", "bi-cloud-snow"),
                80 or 81 or 82 => ("Rain showers", "bi-cloud-rain-heavy"),
                85 or 86 => ("Snow showers", "bi-cloud-snow"),
                95 => ("Thunderstorm", "bi-cloud-lightning-rain"),
                96 or 99 => ("Thunderstorm with hail", "bi-cloud-lightning-rain"),
                _ => ("Unknown", "bi-cloud")
            };
        }
    }
}
