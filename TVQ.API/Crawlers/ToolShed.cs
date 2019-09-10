using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Crawlers
{
    internal class ToolShed
    {
        private readonly HttpClient _client;

        public ToolShed()
        {
            _client = new HttpClient();
        }

        public async Task<List<Tool>> Crawl(Repository repo)
        {
            HttpResponseMessage response = await _client.GetAsync(repo.URI);
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();
            else
                /// TODO: replace with an exception.
                return new List<Tool>();

            var tools = JsonConvert.DeserializeObject<List<Tool>>(content);
            foreach (var tool in tools)
                tool.Repo = repo;
            repo.ToolCount += tools.Count;

            return tools;
        }
    }
}
