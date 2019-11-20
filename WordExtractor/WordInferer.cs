using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordExtractor
{
    public class WordInferer
    {
        public Dictionary<string, int> Dictionary { get; }

        private static readonly int NOT_FOUND_SCORE = -100;

        private ConcurrentDictionary<string, (Int64 score, string best)> scorer;
        public WordInferer(Dictionary<string, int> dictionary)
        {
            scorer = new ConcurrentDictionary<string, (Int64 score, string best)>();
            Dictionary = dictionary;
        }

        public async Task<string> Infer(string text) {
            string res = (await Solve(text)).text;

            return res.Replace(" .", ".").Replace(" ,", ",").Replace(" !", "!").Replace("( ", "(").Replace(" )", ")").Replace(" - ", "-").Replace(" : ", ": ").Replace(" : ", ": ").Replace("“ ", "“").Replace(" ”","”");
        }

        public async Task<(Int64 score, string text)> Solve(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (NOT_FOUND_SCORE, "");
            }

            if(Dictionary.ContainsKey(text.ToLower()))
            {
                return (Dictionary[text.ToLower()] * (int) Math.Pow(text.Length, 2), text);
            }

            if(Regex.IsMatch(text, @"(http(s)?:\/\/.)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)")) {
                return (2, text);
            }

            if(scorer.ContainsKey(text)) {
                return scorer[text];
            }

            if(text.Length == 1) {
                if(char.IsPunctuation(text[0])) {
                    return (0, text);
                }

                return (NOT_FOUND_SCORE, text);
            }

            List<(Int64 score, string text)> possibilities = new List<(Int64 score, string text)>();

            List<Task<(Int64 score, string text)>> tasks = new List<Task<(Int64 score, string text)>>();

            for (int i = 1; i < text.Length; i++)
            {
                tasks.Add(Slv(i, text));
            }

            possibilities.Add((NOT_FOUND_SCORE, text));


            (Int64 score, string text) best = (await Task.WhenAll(tasks.ToArray())).Append((NOT_FOUND_SCORE, text)).OrderByDescending(i => i.score).ThenBy(i => i.text.Length).First();
            // (Int64 score, string text) best = possibilities.OrderByDescending(i => i.score).ThenBy(i => i.text.Length).First();

            scorer.TryAdd(text, best);

            return best;
        }

        private async Task<(Int64 score, string text)> Slv(int i, string text)
        {
            var start = await Solve(text.Substring(0, i));
            var end = await Solve(text.Substring(i, text.Length - i));

            var finalScore = (start.score + end.score);

            if(finalScore > 0 && (start.score < 0 && end.score < 0)) {
                // Console.WriteLine($"{start.score}, {end.score}");
                finalScore = int.MinValue;
            }

            if (finalScore < 0 && (start.score > 0 && end.score > 0))
            {
                throw new Exception($"{start.score} Score too high");
            }

            return ((finalScore), start.text + " " + end.text);
        }
    }
}