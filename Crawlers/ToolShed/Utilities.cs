using System;
using System.Linq;

namespace Genometric.TVQ.Crawlers.ToolShedCrawler
{
    public static class Utilities
    {
        /// <summary>
        /// Generates a random string of alphanumeric characters, 
        /// starting with a alphabetic character, and using all 
        /// capital letters.
        /// </summary>
        public static string GetRandomString(int length = 25)
        {
            var random = new Random();
            const string alphaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string alphanumChars = alphaChars + "0123456789";
            return
                alphaChars[random.Next(alphaChars.Length)] +
                new string(
                    Enumerable.Repeat(alphanumChars,
                    length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
