using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Genometric.TVQ.CLI
{
    public class BioTools
    {
        public List<ExtTool> GetTools(string tmpPath)
        {
            int c = 0;
            Console.Write("\nDownloading archive ...");
            string zipFileName = tmpPath + Path.DirectorySeparatorChar + "archive.zip";

            /*
            new System.Net.WebClient().DownloadFileTaskAsync(
                address: new Uri("https://github.com/bio-tools/content/archive/master.zip"),
                fileName: zipFileName).Wait();
            */

            var extractPath = tmpPath + Path.DirectorySeparatorChar + "ext";
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);
            Directory.CreateDirectory(extractPath);

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
                {
                    //archive.ExtractToDirectory(extractPath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                        if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && !entry.FullName.EndsWith("oeb.json"))
                        {
                            
                            string extractedFileName = Path.GetTempPath() + Utilities.GetRandomString() + ".json";
                            entry.ExtractToFile(extractedFileName);
                            var tool = ExtractTool(extractedFileName);
                            var pub = ExtractPublications(extractedFileName);

                            /*using (StreamWriter writer = new StreamWriter(citationsPath + tool.IDinRepo + ".json"))
                            using (JsonWriter jWriter = new JsonTextWriter(writer))
                                new JsonSerializer().Serialize(jWriter, tool);
*/
                            File.Delete(extractedFileName);
                        }
                }
            }
            catch (InvalidDataException e)
            {
                //_invalidArchive.Add(string.Format("{0}\t{1}", tool.IDinRepo, e.Message));
            }

            /*
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
            */


            return null;
        }

        public Tool ExtractTool(string toolFileName)
        {
            Tool tool;
            using (StreamReader r = new StreamReader(toolFileName))
                tool = JsonConvert.DeserializeObject<Tool>(r.ReadToEnd());
            return tool;
        }

        public List<Publication> ExtractPublications(string toolFileName)
        {
            var pubs = new List<Publication>();
            using (StreamReader r = new StreamReader(toolFileName))
            {
                dynamic array = JsonConvert.DeserializeObject(r.ReadToEnd());
                foreach (JProperty jProperty in array)
                    if (jProperty.Name == "publication")
                    {
                        foreach(JObject pub in jProperty.Value)
                            pubs.Add(JsonConvert.DeserializeObject<Publication>(pub.ToString()));
                        break;
                    }
            }
            return pubs;
        }
    }
}
