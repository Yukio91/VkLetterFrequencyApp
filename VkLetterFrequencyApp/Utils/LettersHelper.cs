using System;
using System.Collections.Generic;
using System.Linq;

namespace VkLetterFrequencyApp.Utils
{
    public static class LettersHelper
    {
        public static Dictionary<char, double> Frequency(string text, int digits = 5)
        {
            var letters = text.ToLower().Where(Char.IsLetter).OrderBy(c => c).ToList();

            double countAll = letters.Count;
            return letters.GroupBy(c => c)
                .ToDictionary(g => g.Key,
                    g => digits > 0 ? Math.Round(g.Count() / countAll, digits) : Math.Round(g.Count() / countAll));
        }
    }
}
