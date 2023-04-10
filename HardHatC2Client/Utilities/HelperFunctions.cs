using System.Text.RegularExpressions;

namespace HardHatC2Client.Utilities
{
    public static class HelperFunctions
    {

        /// <summary>
        /// Cleans empty lines from the top and bottom of data and reduces consecutive empty lines down to one line. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The update string with fixed whitespace</returns>
        public static string RemoveDoubleEmptyLines(this string input)
        {
            string pattern = @"(?<=\S)\n{2,}(?=\S)";
            string replacement = "\n";
            return Regex.Replace(input, pattern, replacement);
        }
    }
}
