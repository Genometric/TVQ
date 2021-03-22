using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Genometric.TVQ.WebService.Crawlers.ToolRepos.HelperTypes
{
    public class BioconductorRelease
    {
        [JsonPropertyName("branch")]
        public string Branch { set; get; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { set; get; }

        [JsonPropertyName("packages")]
        public List<string> Packages { set; get; }
    }
}
