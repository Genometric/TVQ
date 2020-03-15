using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes
{
    [JsonConverter(typeof(ExternalToolModelJsonConverter))]
    public class DeserializedInfo
    {
        public ToolRepoAssociation ToolRepoAssociation { set; get; }

        public List<ToolPublicationAssociation> ToolPubAssociations { set; get; }

        public List<CategoryRepoAssociation> CategoryRepoAssociations { set; get; }

        public List<string> CategoryIDs
        {
            set
            {
                if (value != null)
                {
                    CategoryRepoAssociations = new List<CategoryRepoAssociation>();
                    foreach(var id in value)
                    {
                        CategoryRepoAssociations.Add(
                            new CategoryRepoAssociation()
                            {
                                Category = new Category(),
                                IDinRepo = id
                            });
                    }
                }
            }
        }

        /// <summary>
        /// This is a helper property to deserialize information from JSON objects.
        /// </summary>
        public List<Publication> Publications
        {
            set { PopulateToolPublicationAssociations(value); }
        }

        /// <summary>
        /// Gets a path to a temporary folder
        /// where all the related temporary files are
        /// stored.
        /// </summary>
        public string StagingArea { private set; get; }

        /// <summary>
        /// Sets and gets the filename of the downloaded
        /// archive file. For instance, the archive filename 
        /// of a repository downloaded from ToolShed. 
        /// </summary>
        public string ArchiveFilename { set; get; }

        /// <summary>
        /// Sets and gets path to a folder where the 
        /// contents of the downloaded archive are extracted.
        /// </summary>
        public string ArchiveExtractionPath { set; get; }

        /// <summary>
        /// Sets and gets the filenames of the XML files
        /// extracted from the downloaded archive file. 
        /// </summary>
        public List<string> XMLFiles { set; get; }

        public DeserializedInfo()
        {
            /// An archive downloaded from ToolShed generally
            /// encompasses less than 5 XML files. 
            XMLFiles = new List<string>(capacity: 5);

            CategoryRepoAssociations = new List<CategoryRepoAssociation>();
        }

        public DeserializedInfo(string toolName, DateTime? dateAddedToRepository, Publication publication) : this()
        {
            ToolRepoAssociation = new ToolRepoAssociation()
            {
                Tool = new Tool() { Name = toolName },
                DateAddedToRepository = dateAddedToRepository
            };

            ToolPubAssociations = new List<ToolPublicationAssociation>()
            {
                new ToolPublicationAssociation() { Publication = publication }
            };
        }

        public void SetStagingArea(string sessionPath)
        {
            do
            {
                StagingArea =
                    sessionPath + Utilities.GetRandomString(8) +
                    Path.DirectorySeparatorChar;
            }
            while (Directory.Exists(StagingArea));
            Directory.CreateDirectory(StagingArea);

            ArchiveFilename = StagingArea + Utilities.GetRandomString(8);

            /// To avoid `path traversal attacks` from malicious software, 
            /// there must be a trailing path separator at the end of the path. 
            ArchiveExtractionPath =
                StagingArea + Utilities.GetRandomString(8) +
                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(ArchiveExtractionPath);
        }

        public void PopulateToolPublicationAssociations(List<Publication> publications)
        {
            if (publications == null)
                return;

            if (ToolPubAssociations == null)
                ToolPubAssociations = new List<ToolPublicationAssociation>();

            foreach (var publication in publications)
            {
                ToolPubAssociations.Add(new ToolPublicationAssociation()
                {
                    Publication = publication
                });
            }
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(
            JsonSerializerSettings toolSettings,
            JsonSerializerSettings repoAssoSettings,
            JsonSerializerSettings publicationSettings = null,
            JsonSerializerSettings categorySettings = null)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(
                    typeof(DeserializedInfo),
                    new ExternalToolModelJsonConverter(
                        toolSettings,
                        repoAssoSettings,
                        publicationSettings,
                        categorySettings))
            };
        }

        public static bool TryDeserialize(
            string json,
            JsonSerializerSettings toolSettings,
            JsonSerializerSettings assoSettings,
            JsonSerializerSettings publicationSettings,
            JsonSerializerSettings categorySettings,
            out DeserializedInfo deserializedInfo)
        {
            deserializedInfo = JsonConvert.DeserializeObject<DeserializedInfo>(
                json, GetJsonSerializerSettings(toolSettings,
                                                assoSettings,
                                                publicationSettings,
                                                categorySettings: categorySettings));
            return true;
        }

        public static bool TryDeserialize(
            string json,
            JsonSerializerSettings toolSettings,
            JsonSerializerSettings assoSettings,
            out List<DeserializedInfo> deserializedInfos)
        {
            deserializedInfos =
                JsonConvert.DeserializeObject<List<DeserializedInfo>>(
                    json, GetJsonSerializerSettings(toolSettings, assoSettings));
            return true;
        }

        public static bool TryDeserialize(YamlStream yamlStream, out DeserializedInfo deserializedInfo)
        {
            if (yamlStream == null)
            {
                deserializedInfo = null;
                return false;
            }

            var mapping = (YamlMappingNode)yamlStream.Documents[0].RootNode;
            var tool = new Tool();
            var publications = new List<Publication>();
            foreach (var entry in mapping.Children)
            {
                switch (entry.Key.ToString())
                {
                    case "package":
                        foreach (var child in ((YamlMappingNode)entry.Value).Children)
                            if (child.Key.ToString() == "name")
                            {
                                tool.Name = child.Value.ToString();
                                break;
                            }
                        break;

                    case "source":
                        if (entry.Value.GetType() == typeof(YamlMappingNode))
                        {
                            foreach (var child in ((YamlMappingNode)entry.Value).Children)
                                if (child.Key.ToString() == "url")
                                {
                                    tool.CodeRepo = child.Value.ToString();
                                    break;
                                }
                        }
                        else if (entry.Value.GetType() == typeof(YamlSequenceNode))
                        {
                            foreach (YamlMappingNode child in ((YamlSequenceNode)entry.Value).Children)
                                foreach (var c in child.Children)
                                    if (c.Key.ToString() == "url")
                                    {
                                        tool.CodeRepo = c.Value.ToString();
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
                                        publications.Add(new Publication()
                                        {
                                            DOI = x.ToString().Split(':')[1]
                                        });
                                        break;
                                    }
                        break;
                }
            }

            deserializedInfo = new DeserializedInfo
            {
                ToolRepoAssociation = new ToolRepoAssociation() { Tool = tool }
            };
            deserializedInfo.PopulateToolPublicationAssociations(publications);
            return true;
        }
    }
}
