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

#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> CategoryIDs { set; get; }
#pragma warning restore CA2227 // Collection properties should be read only

#pragma warning disable CA2227 // Collection properties should be read only
        public List<Category> Categories { set; get; }
#pragma warning restore CA2227 // Collection properties should be read only

        public DateTime? DateAddedToRepository { set; get; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<Publication> Publications { set;  get; }
#pragma warning restore CA2227 // Collection properties should be read only

        public static bool TryDeserialize(
            string json,
            out ToolRepoAssociation toolRepoAssociation,
            out List<ToolPublicationAssociation> toolPubAssociations)
        {
            // Deserialize the JSON object to a helper type. 
            var repoTool = JsonConvert.DeserializeObject<RepoTool>(json);

            // Convert the helper type to TVQ's model. 
            toolRepoAssociation = new ToolRepoAssociation(repoTool);
            if (repoTool.Publications != null && repoTool.Publications.Count > 0)
            {
                toolPubAssociations = new List<ToolPublicationAssociation>();
                foreach (var pub in repoTool.Publications)
                    toolPubAssociations.Add(new ToolPublicationAssociation() { Publication = pub });
            }
            else
            {
                toolPubAssociations = null;
            }

            return true;
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
