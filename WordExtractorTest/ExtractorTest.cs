using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using ApiLibs.General;
using WordExtractor;
using System.Text.Json;

namespace WordExtractorTest
{
    public class PasswordTest
    {
        [Test]
        public async Task Execute()
        {
            var dict = await new Memory
            {
                Application = "WordCount"
            }.Read<Dictionary<string, int>>("wrangle-words.json");

            var myLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            Console.WriteLine(myLocation);
            
            var input = await new Memory(Path.GetDirectoryName(myLocation) + "\\").Read("sentences.json", new Dictionary<string, string> { {"inputiscool", "input is cool"}});

            var res = (await input.Keys.Select(i => new WordInferer(dict).Infer(i)).WhenAll()).Zip(input.Values).Select(i => (i.First, i.Second, score: Score(i.Second, i.First))).Select(i => (text: i.score == 0 ? i.Second : $"{i.First}|{i.Second}", i.score)).ToDictionary(i => i.text, i => i.score);

            Assert.Fail(res.ToJson());
        }

        Dictionary<(int firstI, int secondI), int> dict = new Dictionary<(int firstI, int secondI), int>();

        private int Score(string first, string second, int firstI = 0, int secondI = 0, int score = 0)
        {
            while(firstI != first.Length && secondI != second.Length) {
                if(first[firstI] == second[secondI]) {
                    firstI += 1;
                    secondI += 1;
                }
                else {
                    return dict.GetOrCalculate((firstI,secondI), (v) => Math.Min(Score(first, second, firstI, secondI + 1, score + 1), Score(first, second, firstI + 1, secondI, score + 1)));
                }
            }

            return score;
        }
    }

    public static class Extensions {

        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }

        public static void Print(this object obj)
        {
            Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }

        public static string CombineWithNewLine(this IEnumerable<string> IEnumerable)
        {
            return IEnumerable.Combine((i, j) => i + "\n" + j);
        }
        public static string CombineWithSpace(this IEnumerable<string> IEnumerable)
        {
            return IEnumerable.Combine((i, j) => i + " " + j);
        }

        public static T Combine<T>(this IEnumerable<T> IEnumerable, Func<T, T, T> func)
        {
            var count = 0;
            foreach (var item in IEnumerable)
            {
                count++;

                if (count > 2)
                {
                    break;
                }
            }

            if (count > 2)
            {
                return IEnumerable.Aggregate(func);
            }
            else if (count == 1)
            {
                return IEnumerable.First();
            }
            else
            {
                return default(T);
            }
        }

        public static T GetOrCalculate<V,T>(this Dictionary<V,T> dictionary, V key, Func<V, T> func) {
            if(dictionary.ContainsKey(key)) {
                return dictionary[key];
            }

            var value = func(key);
            dictionary.Add(key, value);

            return value;
        }

        public async static Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> IEnumerable)
        {
            return (await Task.WhenAll(IEnumerable));
        }

        public static void Foreach<T>(this IEnumerable<T> IEnumerable, Action<T> func)
        {
            foreach (var item in IEnumerable)
            {
                func(item);
            }
        }
    }
}