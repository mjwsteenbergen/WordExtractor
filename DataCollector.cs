

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using HtmlAgilityPack;

namespace WordExtractor
{
    public class DataCollector {

        Dictionary<string, int> wordCount;

        public DataCollector()
        {
            wordCount = new Dictionary<string, int>();
        }

        public async Task<Dictionary<string, int>> Scrape()
        {

            var categories = new List<string> {
                "https://en.wikipedia.org/wiki/Category:Computer_programming",
                "https://en.wikipedia.org/wiki/Category:Software_development_philosophies",
                "https://en.wikipedia.org/wiki/Category:Software_release",
                "https://en.wikipedia.org/wiki/Category:Web_development",
                "https://en.wikipedia.org/wiki/Category:Software_development",
                "https://en.wikipedia.org/wiki/Category:Software_development_process",
                "https://en.wikipedia.org/wiki/Category:Software_testing",
                "https://en.wikipedia.org/wiki/Category:Tests",
            };
            var seenCats = categories;

            var urls = new List<string>();

            var scrapingTasks = categories.Select(i => Scrape(i)).ToList();
            categories = new List<string> { "https://en.wikipedia.org/w/index.php?title=Special:LongPages&limit=500&offset=0" };

            while (categories.Count > 0)
            {
                while(categories.Count > 0 && seenCats.Contains(categories.First())) 
                {
                    categories.RemoveAt(0);
                }

                var task = await Task.WhenAny(scrapingTasks.ToArray());
                scrapingTasks.Remove(task);

                var result = task.Result;

                urls.AddRange(result.articles);
                categories.AddRange(result.subCategories);
                Console.WriteLine($"New categories: {result.subCategories.Count} \t Urls: {urls.Count} ");

                seenCats.Add(categories.ElementAt(0));
                scrapingTasks.Add(Scrape(categories.ElementAt(0)));
                categories.RemoveAt(0);

                if(urls.Count > 20000) {
                    break;
                }

            }

            await Task.WhenAll(scrapingTasks.ToArray());

            Console.WriteLine($"Scraping {urls.Count} of articles");

            urls = urls.Distinct().ToList();

            var tasks = urls.Take(10).Select(i => ScrapeArticle(i)).ToList();
            urls.RemoveRange(0, 10);

            while(urls.Count > 0)
            {
                int index = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(index);
                tasks.Add(ScrapeArticle(urls.ElementAt(0)));
                urls.RemoveAt(0);
            }

            await Task.WhenAll(tasks.ToArray());

            return wordCount;
        }


        public async Task<(IEnumerable<string> articles, List<string> subCategories)> Scrape(string url) {
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = await web.LoadFromWebAsync(url);

            var tasks2 = htmlDoc.DocumentNode.Descendants("a").Select(i => i.Attributes.FirstOrDefault(j => j.Name == "href")).Where(i => i != null).Select(i => i.Value).ToList();
            var websites  = tasks2.Where(i => i.StartsWith("/wiki/")).Where(i => !i.ToLower().Contains("/wiki/category:")).Where(i => !i.ToLower().Contains("list_of")).Select(i => "https://en.wikipedia.org" + i).ToList();
            var subCategories = tasks2.Where(i => i.StartsWith("/wiki/Category:")).Select(i => "https://en.wikipedia.org" + i).ToList();


            return (websites, subCategories);
        }

        public async Task ScrapeArticle(string url) {
            try {
                HtmlWeb web = new HtmlWeb();

                var htmlDoc = await web.LoadFromWebAsync(url);

                CountWord(htmlDoc.GetElementbyId("content").InnerText);

            } catch {

            }
            
        }

        public void CountWord(string text) {
            var wordSeperators = new List<char> { '\t', '\n', ' ' };
            var forbiddenChar = new List<char> { };

            int lastWord = 0;
            int numberOfWords = 0;
            string nextWord = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (wordSeperators.Contains(text[i])) {
                    if(lastWord >= i) {
                        continue;
                    }
                    Increment(nextWord.Trim());
                    numberOfWords++;
                    lastWord = i + 1;
                    nextWord = "";
                    continue;
                }

                if(text[i] == '\\') {
                    if(text[i+1] == 'u') {
                        nextWord += (char)int.Parse(text.Substring(i + 2, 4), NumberStyles.HexNumber);                        
                        i += 5;
                        continue;
                    } else {
                        i += 1;
                        continue;
                    }
                }

                if(forbiddenChar.Contains(text[i]) || char.IsDigit(text[i]) || char.IsPunctuation(text[i]) || text[i] == '^' || char.IsSeparator(text[i])) {
                    continue;
                }

                nextWord += text[i];
            }
            Console.WriteLine($"Found {numberOfWords}");
        }

        private void Increment(string text)
        {
            text = text.ToLower();
            if(wordCount.ContainsKey(text)) {
                wordCount[text] += 1;
            } else {
                wordCount.Add(text, 1);
            }
        }
    }
}