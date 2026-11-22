using System.Text.RegularExpressions;

namespace Voyage.Web.Common
{
    public static class TextNormalizer
    {
        private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

        public static string SingleLine(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : Whitespace.Replace(value.Trim(), " ");
        }
    }
}
