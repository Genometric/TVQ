using Genometric.TVQ.API.Crawlers.ToolRepos;
using Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

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

        public List<BioToolsTopic> Topics { set; get; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> CategoryIDs { set; get; }
#pragma warning restore CA2227 // Collection properties should be read only

#pragma warning disable CA2227 // Collection properties should be read only
        public List<Category> Categories { set; get; }
#pragma warning restore CA2227 // Collection properties should be read only

        public DateTime? DateAddedToRepository { set; get; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<Publication> Publications { set; get; }
#pragma warning restore CA2227 // Collection properties should be read only

        public static bool TryDeserialize(
            string json,
            string sessionPath,
            out ToolInfo toolInfo)
        {
            // Deserialize the JSON object to a helper type. 
            var repoTool = JsonConvert.DeserializeObject<RepoTool>(json);

            // Convert the helper type to TVQ's model. 
            var toolRepoAssociation = new ToolRepoAssociation(repoTool);
            var toolPubAssociations = new List<ToolPublicationAssociation>();

            if (repoTool.Publications != null && repoTool.Publications.Count > 0)
                foreach (var pub in repoTool.Publications)
                    toolPubAssociations.Add(new ToolPublicationAssociation() { Publication = pub });
            else
                toolPubAssociations = null;

            toolInfo = new ToolInfo(toolRepoAssociation, toolPubAssociations, sessionPath);

            if (repoTool.Topics != null)
            {
                foreach (var topic in repoTool.Topics)
                {
                    toolInfo.Categories.Add(new Category()
                    {
                        Name = topic.Term,
                        URI = topic.URI
                    });
                }
            }

            return true;
        }

        public static bool TryDeserialize(
            YamlStream yamlStream,
            out ToolRepoAssociation toolRepoAssociation,
            out List<ToolPublicationAssociation> toolPubAssociations)
        {
            toolRepoAssociation = null;
            toolPubAssociations = null;
            if (yamlStream == null)
                return false;

            var mapping = (YamlMappingNode)yamlStream.Documents[0].RootNode;
            var repoTool = new RepoTool() { Publications = new List<Publication>() };
            foreach (var entry in mapping.Children)
            {
                switch (entry.Key.ToString())
                {
                    case "package":
                        foreach (var child in ((YamlMappingNode)entry.Value).Children)
                            if (child.Key.ToString() == "name")
                            {
                                repoTool.Name = child.Value.ToString();
                                break;
                            }
                        break;

                    case "source":
                        if (entry.Value.GetType() == typeof(YamlMappingNode))
                        {
                            foreach (var child in ((YamlMappingNode)entry.Value).Children)
                                if (child.Key.ToString() == "url")
                                {
                                    repoTool.CodeRepo = child.Value.ToString();
                                    break;
                                }
                        }
                        else if (entry.Value.GetType() == typeof(YamlSequenceNode))
                        {
                            foreach (YamlMappingNode child in ((YamlSequenceNode)entry.Value).Children)
                                foreach (var c in child.Children)
                                    if (c.Key.ToString() == "url")
                                    {
                                        repoTool.CodeRepo = c.Value.ToString();
                                        break;
                                    }
                        }
                        break;

                    case "extra":
                        foreach (var child in ((YamlMappingNode)entry.Value).Children)
                            if (child.Key.ToString() == "identifiers")
                                foreach (var x in ((YamlSequenceNode)child.Value).Children)
                                    if (x.ToString().StartsWith("doi:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        repoTool.Publications.Add(new Publication()
                                        {
                                            DOI = x.ToString().Split(':')[1]
                                        });
                                        break;
                                    }
                        break;
                }
            }

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
