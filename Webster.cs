using System.Collections.Generic;
using System.Threading.Tasks;
using ApiLibs;

namespace WordExtractor
{
    internal class Webster : Service
    {
        public Webster() : base("https://raw.githubusercontent.com/")
        {
        }

        public Task<Dictionary<string, string>> Download() => MakeRequest<Dictionary<string,string>>("adambom/dictionary/master/dictionary.json");
    }
}