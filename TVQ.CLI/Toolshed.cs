using Genometric.TVQ.API.Model;
using Genometric.TVQ.CLI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TVQ.CLI
{
    public class ToolShed
    {
        public async Task<List<Tool>> GetToolsList()
        {
            Console.Write("Getting tools list ... ");
            var _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync("https://toolshed.g2.bx.psu.edu/api/repositories");
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();
            else
                /// TODO: replace with an exception.
                return new List<Tool>();

            var tools = JsonConvert.DeserializeObject<List<Tool>>(content);
            Console.WriteLine("\tDone!");
            return tools;
        }

        public async Task DownloadArchives(List<Tool> tools, string downloadPath)
        {
            int c = 0;
            Console.Write("\nDownloading tools:");
            foreach (var tool in tools)
            {
                Console.Write(string.Format("\n{0}/{1}:\t{2}", c++, tools.Count, tool.Name));

                var downloader = new System.Net.WebClient().DownloadFileTaskAsync(
                    address: new Uri(string.Format(
                        "https://toolshed.g2.bx.psu.edu/repos/{0}/{1}/archive/tip.zip",
                        tool.Owner,
                        tool.Name)),
                    fileName: downloadPath + tool.IDinRepo + ".zip");

                await downloader;
            }
        }

        public async Task ExtractCitation(string downloadPath, string citationsFileName, List<ExtTool> tools)
        {
            var zipFiles = Directory.GetFiles(downloadPath);

            foreach (var zipFile in zipFiles)
            {
                var tool = tools.Find(x => x.IDinRepo == Path.GetFileNameWithoutExtension(zipFile));
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipFile))
                        foreach (ZipArchiveEntry entry in archive.Entries)
                            if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            {
                                var extractedFileName = Path.GetTempFileName() + new Random().Next(100, 100000) + ".xml";
                                entry.ExtractToFile(extractedFileName);

                                ExtractPublications(extractedFileName, tool);

                                using (StreamWriter writer = new StreamWriter(citationsFileName + tool.IDinRepo + ".json"))
                                    using (JsonWriter jWriter = new JsonTextWriter(writer))
                                {
                                    new JsonSerializer().Serialize(jWriter, tool);
                                }   
                            }
                }
                catch (InvalidDataException e)
                {
                    /// This exception is thrown when the Zip archive
                    /// cannot be read.
                }
                catch (Exception e)
                {

                }
            }

            var xmlFiles = new List<string>();
        }

        private void ExtractPublications(string xmlFile, ExtTool tool)
        {
            try
            {
                XElement toolDoc = XElement.Load(xmlFile);

                foreach (var item in toolDoc.Elements("citations").Descendants())
                {
                    if (item.Attribute("type") != null)
                    {
                        switch (item.Attribute("type").Value.Trim().ToLower())
                        {
                            case "doi":
                                tool.Publications.Add(new Publication()
                                {
                                    ToolId = tool.Id,
                                    DOI = item.Value
                                });
                                break;

                            case "bibtex":
                                tool.Publications.Add(new Publication()
                                {
                                    ToolId = tool.Id,
                                    Citation = item.Value
                                });
                                break;
                        }
                    }
                }
            }
            catch (System.Xml.XmlException e)
            {
                /// This exception may happen if the XML 
                /// file has multiple roots.
            }
        }
    }
}
