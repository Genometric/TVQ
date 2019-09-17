using Genometric.TVQ.API.Model;
using Genometric.TVQ.CLI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TVQ.CLI
{
    public class ToolShed
    {
        private List<string> _invalidXMLs;
        public ReadOnlyCollection<string> InvalidXMLs
        {
            get
            {
                return _invalidXMLs.AsReadOnly();
            }
        }

        private List<string> _invalidArchive;
        public ReadOnlyCollection<string> InvalidArchives
        {
            get
            {
                return _invalidArchive.AsReadOnly();
            }
        }

        public ToolShed()
        {
            _invalidXMLs = new List<string>();
            _invalidArchive = new List<string>();
        }

        public async Task<List<Tool>> GetToolsList()
        {
            Console.Write("Getting tools list from the ToolShed ... ");
            var _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync("https://toolshed.g2.bx.psu.edu/api/repositories");
            string content;
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();
            else
                /// TODO: replace with an exception.
                return new List<Tool>();

            var tools = JsonConvert.DeserializeObject<List<Tool>>(content);
            Console.WriteLine("Done!");
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

        public void ExtractCitation(string downloadPath, string citationsFileName, List<ExtTool> tools)
        {
            var zipFiles = Directory.GetFiles(downloadPath);

            Console.WriteLine("Extracting wrappers ...");
            int c = 0;
            foreach (var zipFile in zipFiles)
            {
                Console.Write(string.Format("\r\tProcessing archive {0}/{1}", ++c, zipFiles.Length));
                var tool = tools.Find(x => x.IDinRepo == Path.GetFileNameWithoutExtension(zipFile));
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipFile))
                        foreach (ZipArchiveEntry entry in archive.Entries)
                            if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            {
                                string extractedFileName = Path.GetTempPath() + RandomString() + ".xml";
                                entry.ExtractToFile(extractedFileName);
                                ExtractPublications(extractedFileName, tool);

                                using (StreamWriter writer = new StreamWriter(citationsFileName + tool.IDinRepo + ".json"))
                                using (JsonWriter jWriter = new JsonTextWriter(writer))
                                    new JsonSerializer().Serialize(jWriter, tool);

                                File.Delete(extractedFileName);
                            }
                }
                catch (InvalidDataException e)
                {
                    _invalidArchive.Add(string.Format("{0}\t{1}", tool.IDinRepo, e.Message));
                }
                catch (Exception e)
                {
                    _invalidArchive.Add(string.Format("{0}\t{1}", tool.IDinRepo, e.Message));
                }
            }

            if (_invalidArchive.Count > 0)
            {
                var fileName = AppDomain.CurrentDomain.BaseDirectory + "SkippedArchives.txt";
                using (var writer = new StreamWriter(fileName))
                    foreach (var item in _invalidArchive)
                        writer.WriteLine(item);
                Console.WriteLine(string.Format(
                    "\n\n\tSkipped {0} invalid archives. " +
                    "Details logged in file {1}.", _invalidArchive.Count, fileName));
            }

            if (_invalidXMLs.Count > 0)
            {
                var fileName = AppDomain.CurrentDomain.BaseDirectory + "SkippedXMLs.txt";
                using (var writer = new StreamWriter(fileName))
                    foreach (var item in _invalidXMLs)
                        writer.WriteLine(item);
                Console.WriteLine(string.Format(
                    "\tSkipped {0} invalid XML files. " +
                    "Details logged in file {1}.\n\n", _invalidXMLs.Count, fileName));
            }
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
                _invalidXMLs.Add(string.Format("{0}\t{1}", tool.IDinRepo, e.Message));
            }
        }

        public static string RandomString(int length=25)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
