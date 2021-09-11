using System.Text.RegularExpressions;

namespace OpenWasteMapUK.Utilities
{
    public static class StringExtensions
    {
        private static readonly Regex Regex = new Regex(@"\s+");
        public static string RemoveWhitespace(this string input)
        {
            return Regex.Replace(input, "");
        }

    }
}
