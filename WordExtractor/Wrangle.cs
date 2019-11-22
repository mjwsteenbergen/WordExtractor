using System;
using System.Collections.Generic;
using System.Linq;

namespace WordExtractor
{
    public class Wrangle
    {
        public static void Wrangler(Dictionary<string, int> dict) {

            List<string> goodbye = new List<string>();
            List<string> twoWords = new List<string> { "of", "in", "to", "on", "is", "by", "as", "at", "an", "it", "be", "or", "us", "he", "no", "up", "so", "if", "do", "my", "go", "me", "am" };

            foreach (var key in dict.Keys.ToList())
            {
                dict[key] = (int) Math.Log10(dict[key]);

                // if (dict[key] < 20)
                // {
                    // goodbye.Add(key);
                // }

                if (key.Length == 2 && !twoWords.Contains(key)) {
                    goodbye.Add(key);
                }
            }

            foreach (var key in goodbye)
            {
                dict.Remove(key);
            }

            Console.WriteLine($"Removed {goodbye.Count} items");

            SetSingleLetters(dict);
        }

        private static void SetSingleLetters(Dictionary<string, int> dict)
        {
            dict.Remove("");
            dict.Remove("b");
            dict.Remove("c");
            dict.Remove("d");
            dict.Remove("e");
            dict.Remove("f");
            dict.Remove("g");
            dict.Remove("h");
            dict.Remove("j");
            dict.Remove("k");
            dict.Remove("l");
            dict.Remove("m");
            dict.Remove("n");
            dict.Remove("o");
            dict.Remove("p");
            dict.Remove("q");
            dict.Remove("r");
            dict.Remove("s");
            dict.Remove("t");
            dict.Remove("u");
            dict.Remove("v");
            dict.Remove("w");
            dict.Remove("x");
            dict.Remove("y");
            dict.Remove("z");
        }
    }
}