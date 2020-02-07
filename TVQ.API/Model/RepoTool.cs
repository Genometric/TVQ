using Genometric.TVQ.API.Crawlers.ToolRepos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model
{
    /// <summary>
    /// This type is a helper type, to help convert a tool JSON object 
    /// obtained from tool repositories (e.g., ToolShed) to TVQ tool type. 
    /// This helper is used since there is no direct mapping from tool in 
    /// JSON format to the TVQ Tool type. For instance, some information 
    /// in the JSON object are stored in ToolRepoAssociation type and 
    /// some information as stored in the Tool type. 
    /// 
    /// TODO: see if this can be handled at (De)Serialization step 
    /// without needing this helper type.
    /// </summary>
    [JsonConverter(typeof(RepoToolJsonConverter))]
    public class RepoTool
    {
        public string IDinRepo { set; get; }

        public string Name { set; get; }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

        public string Owner { set; get; }

        public string UserID { set; get; }

        public string Description { set; get; }

        public int? TimesDownloaded { set; get; }

        public List<string> CategoryIDs { set; get; }

        public List<Category> Categories { set; get; }

        public DateTime? DateAddedToRepository { set; get; }

        public static ToolRepoAssociation DeserializeTool(string json)
        {
            var repoTool = JsonConvert.DeserializeObject<RepoTool>(json);
            return new ToolRepoAssociation(repoTool);
        }

        public static List<ToolInfo> DeserializeTools(string json, string sessionPath)
        {
            var repoTools = new List<RepoTool>(JsonConvert.DeserializeObject<List<RepoTool>>(json));
            var infos = new List<ToolInfo>(repoTools.Count);
            foreach (var repoTool in repoTools)
                infos.Add(
                    new ToolInfo(new ToolRepoAssociation(repoTool), null, sessionPath)
                    {
                        CategoryIDs = repoTool.CategoryIDs
                    });

            return infos;
        }
    }
}
