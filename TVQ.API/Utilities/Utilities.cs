using System;
using System.Linq;

namespace Genometric.TVQ.API.Model
{
    public static class Utilities
    {
        public static string GetRandomString(int length = 25)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
