using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using ApiLibs.General;

namespace WordExtractor
{
    class Program
    {
        static async Task Main(bool collect, bool wrangle, string infer, string count)
        {
            if(collect) {
                DataCollector dataCollector = new DataCollector();
                var dict = await dataCollector.Scrape();

                await new Memory
                {
                    Application = "WordCount"
                }.Write("all-words.json", dict);

                dict.Count.Print();
            }

            if(wrangle) {
                var dict = await new Memory
                {
                    Application = "WordCount"
                }.Read<Dictionary<string, int>>("all-words.json");


                Wrangle.Wrangler(dict);

                await new Memory
                {
                    Application = "WordCount"
                }.Write("wrangle-words.json", dict);

                dict.OrderByDescending(i => i.Value).Take(10).Select(i => $"{i.Key} {i.Value}").Print();

                dict.Count.Print();
            }

            if(infer != null) {
                var dict = await new Memory
                {
                    Application = "WordCount"
                }.Read<Dictionary<string, int>>("wrangle-words.json");

                var best = await new WordInferer(dict).Solve(infer);

                Console.WriteLine($"\"{best.text}\" had {best.score}");
            }

            if(count != null) {
                var dict = await new Memory
                {
                    Application = "WordCount"
                }.Read<Dictionary<string, int>>("wrangle-words.json");

                if(dict.ContainsKey(count.ToLower())) {
                    Console.WriteLine($"{count}: {dict[count.ToLower()]}");
                } else {
                    Console.WriteLine($"\"{count}\" is not present in the dictionary");
                }
            }
        }
    }

    public static class Extensions {
        public static void Print(this object obj)
        {
            Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions()
            {
                WriteIndented = true,
            }));
        }
    }
}
