

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
            var list = await Task.WhenAll(
                Scrape("https://en.wikipedia.org/wiki/Category:Computer_programming"),
                Scrape("https://en.wikipedia.org/wiki/Category:Software_development_philosophies"),
                Scrape("https://en.wikipedia.org/wiki/Category:Software_release"),
                Scrape("https://en.wikipedia.org/wiki/Category:Web_development"),
                Scrape("https://en.wikipedia.org/wiki/Category:Software_development"),
                Scrape("https://en.wikipedia.org/wiki/Category:Software_development_process"),
                Scrape("https://en.wikipedia.org/w/index.php?title=Special:LongPages&limit=500&offset=0"));

            var urls = list.SelectMany(i => i).ToList();

            var tasks = urls.Take(10).Select(i => ScrapeArticle(i)).ToList();
            urls.RemoveRange(0, 10);

            while(urls.Count > 0)
            {
                int index = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(index);
                tasks.Add(ScrapeArticle(urls.ElementAt(0)));
                urls.RemoveAt(0);
            }

            return wordCount;
        }


        public async Task<IEnumerable<string>> Scrape(string url) {
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = await web.LoadFromWebAsync(url);

            var tasks2 = htmlDoc.DocumentNode.Descendants("a").Select(i => i.Attributes.FirstOrDefault(j => j.Name == "href")).Where(i => i != null).Select(i => i.Value);
            var websites  = tasks2.Where(i => i.StartsWith("/wiki/")).Where(i => !i.ToLower().Contains("list_of")).Select(i => "https://en.wikipedia.org" + i);

            return websites;
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
            text.Length.Print();

            string nextWord = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (wordSeperators.Contains(text[i])) {
                    if(lastWord >= i) {
                        continue;
                    }
                    Increment(nextWord);
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

                if(forbiddenChar.Contains(text[i]) || char.IsDigit(text[i]) || char.IsPunctuation(text[i])) {
                    continue;
                }

                nextWord += text[i];
            }
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