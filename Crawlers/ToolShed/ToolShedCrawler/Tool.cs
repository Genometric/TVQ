using Genometric.TVQ.WebService.Model;
using System.Collections.Generic;
using System.IO;

namespace Genometric.TVQ.Crawlers.ToolShedCrawler
{
    public class Tool
    {
        public string ID { set; get; }
        public string Name { set; get; }
        public string Owner { set; get; }

        /// <summary>
        /// Gets a path to a temporary folder
        /// where all the related temporary files are
        /// stored.
        /// </summary>
        public string StagingArea { private set; get; }
        /// <summary>
        /// Sets and gets path to a folder where the 
        /// contents of the downloaded archive are extracted.
        /// </summary>
        public string ArchiveExtractionPath { set; get; }
        /// <summary>
        /// Sets and gets the filename of the downloaded
        /// archive file. For instance, the archive filename 
        /// of a repository downloaded from ToolShed. 
        /// </summary>
        public string ArchiveFilename { set; get; }

        /// <summary>
        /// Sets and gets the filenames of the XML files
        /// extracted from the downloaded archive file. 
        /// </summary>
        public List<string> XMLFiles { set; get; }

        public Tool()
        {
            XMLFiles = new List<string>();
        }

        public void EnsureStagingArea(string sessionPath)
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
    }
}
