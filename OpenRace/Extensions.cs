using System.Text.RegularExpressions;

namespace OpenRace
{
    public static class Extensions
    {
        /// <summary>
        /// Удаляет все пробелы, скобки и тире из номера телефона и приводит его к единому формату  +796212345678
        /// </summary>
        public static bool TryStandardizePhoneNumber(this string? input, out string? output, bool toRussian = true)
        {
            output = null;
            if (string.IsNullOrWhiteSpace(input))
                return false;
			
            output = input.Replace(" ", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "")
                .Trim();

            if (toRussian)
            {
                if(output.Length == 11 && output.StartsWith("7")) //796212345678
                    output = "+" + output;
                else if (output.Length == 10 && !output.StartsWith("+7")) //96212345678
                    output = "+7" + output;
                else if (output.StartsWith("8")) //896212345678
                    output = "+7" + output.Substring(1);
            }

            return output.IsPhoneNumber();
        }

        private static Regex phoneRegex = new Regex(@"\+79\d{2}\d{7}", RegexOptions.Compiled);
        public static bool IsPhoneNumber(this string input, bool strict = true)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return input.Length == 12 && phoneRegex.IsMatch(input);
        }
    }
}