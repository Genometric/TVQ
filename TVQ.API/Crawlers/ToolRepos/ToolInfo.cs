using Genometric.TVQ.API.Model;
using System.Collections.Generic;
using System.IO;

namespace Genometric.TVQ.API.Crawlers.ToolRepos
{
    internal class ToolInfo
    {
        public Tool Tool { get; }

        /// <summary>
        /// Gets a path to a temporary folder
        /// where all the related temporary files are
        /// stored.
        /// </summary>
        public string StagingArea { get; }

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

        public ToolInfo(
            Tool tool, string sessionPath,
            bool createStagingArea = true)
        {
            Tool = tool;
            StagingArea =
                sessionPath + Utilities.GetRandomString() +
                Path.DirectorySeparatorChar;
            if (createStagingArea)
            {
                if (Directory.Exists(StagingArea))
                    Directory.Delete(StagingArea, true);
                Directory.CreateDirectory(StagingArea);
            }

            ArchiveFilename = StagingArea + Utilities.GetRandomString(8);

            /// To avoid `path traversal attacks` from malicious software, 
            /// there must be a trailing path separator at the end of the path. 
            ArchiveExtractionPath =
                StagingArea + Utilities.GetRandomString(8) +
                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(ArchiveExtractionPath);

            /// An archive downloaded from ToolShed generally
            /// encompasses less than 5 XML files. 
            XMLFiles = new List<string>(capacity: 5);
        }
    }
}
