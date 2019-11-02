using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordExtractor
{
    public class WordInferer
    {
        public Dictionary<string, int> Dictionary { get; }

        private ConcurrentDictionary<string, (Int64 score, string best)> scorer;
        public WordInferer(Dictionary<string, int> dictionary)
        {
            scorer = new ConcurrentDictionary<string, (Int64 score, string best)>();
            Dictionary = dictionary;
        }

        public (Int64 score, string text) Solve(string text)
        {
            if(Dictionary.ContainsKey(text.ToLower()))
            {
                return (Dictionary[text.ToLower()] * (int) Math.Pow(text.Length, 6), text);
            }

            if(scorer.ContainsKey(text)) {
                return scorer[text];
            }

            if(text.Length == 1) {
                return (int.MinValue / 10, text);
            }

            List<(Int64 score, string text)> possibilities = new List<(Int64 score, string text)>();

            List<Task<(Int64 score, string text)>> tasks = new List<Task<(Int64 score, string text)>>();

            for (int i = 1; i < text.Length; i++)
            {
                possibilities.Add(Slv(i, text));
            }


            // (int score, string text) best = (await Task.WhenAll(tasks.ToArray())).OrderByDescending(i => i.score).First();
            (Int64 score, string text) best = possibilities.OrderByDescending(i => i.score).First();

            scorer.TryAdd(text, best);

            return best;
        }

        private (Int64 score, string text) Slv(int i, string text)
        {
            var start = Solve(text.Substring(0, i));
            var end = Solve(text.Substring(i, text.Length - i));

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