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

        private ConcurrentDictionary<string, (int score, string best)> scorer;
        public WordInferer(Dictionary<string, int> dictionary)
        {
            scorer = new ConcurrentDictionary<string, (int score, string best)>();
            Dictionary = dictionary;
        }

        public async Task<(int score, string text)> Solve(string text)
        {
            if(Dictionary.ContainsKey(text.ToLower()))
            {
                return (Dictionary[text.ToLower()], text);
            }

            if(scorer.ContainsKey(text)) {
                return scorer[text];
            }

            if(text.Length == 1) {
                return (0, text);
            }

            List<(int score, string text)> possibilities = new List<(int score, string text)>();

            List<Task<(int score, string text)>> tasks = new List<Task<(int score, string text)>>();

            for (int i = 1; i < text.Length; i++)
            {
                tasks.Add(Slv(i, text));
            }


            (int score, string text) best = (await Task.WhenAll(tasks.ToArray())).OrderByDescending(i => i.score).First();

            scorer.TryAdd(text, best);

            return best;
        }

        private async Task<(int score, string text)> Slv(int i, string text)
        {
            var start = await Solve(text.Substring(0, i));
            var end = await Solve(text.Substring(i, text.Length - i));
            return (start.score + end.score, start.text + " " + end.text);
        }
    }
}