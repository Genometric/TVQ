using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Genometric.TVQ.Crawlers.ToolShedCrawler
{
    internal class CommandLineOptions
    {
        private readonly CommandLineApplication _cla;

        private readonly CommandOption _categories = new CommandOption("-c | --categories <value>", CommandOptionType.SingleValue)
        {
            Description = "JSON filename to serialize categories."
        };

        private readonly CommandOption _tools = new CommandOption("-t | --tools <value>", CommandOptionType.SingleValue)
        {
            Description = "JSON filename to serialize tools."
        };

        private readonly CommandOption _publications = new CommandOption("-p | --publications <value>", CommandOptionType.SingleValue)
        {
            Description = "JSON filename to serialize publications."
        };

        public static string HelpOption
        {
            get { return "-? | -h | --help"; }
        }

        public string CategoriesFilename { get { return _categories.Value(); } }
        public string ToolsFilename { get { return _tools.Value(); } }
        public string PublicationsFilename { get { return _publications.Value(); } }

        public CommandLineOptions()
        {
            _cla = new CommandLineApplication
            {
                Name = "ToolShed Crawler",
                Description = "Collects ToolShed's tool information in JSON files.",
                ExtendedHelpText =
                "\n\rDocumentation:\thttps://genometric.github.io/TVQ/" +
                "\n\rSource Code:\thttps://github.com/Genometric/TVQ\n\r"
            };

            _cla.HelpOption(HelpOption);
            _cla.VersionOption("-v | --version", () =>
            {
                return string.Format(
                    "Version {0}",
                    Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            });

            _cla.Options.Add(_categories);
            _cla.Options.Add(_tools);
            _cla.Options.Add(_publications);
        }

        public void Parse(string[] args, out bool helpIsDisplayed)
        {
            _cla.OnExecute(() =>
            {
                var missingRequiredArgs = new List<string>();
                if (!_categories.HasValue())
                    missingRequiredArgs.Add(_categories.LongName);
                if (!_tools.HasValue())
                    missingRequiredArgs.Add(_tools.LongName);
                if (!_publications.HasValue())
                    missingRequiredArgs.Add(_publications.LongName);

                if (missingRequiredArgs.Count > 0)
                {
                    var msgBuilder = new StringBuilder("the following required arguments are missing: ");
                    foreach (var arg in missingRequiredArgs)
                        msgBuilder.Append(arg);
                    throw new ArgumentException(msgBuilder.ToString());
                }
                else
                {
                    return 1;
                }
            });

            var status = _cla.Execute(args);
            helpIsDisplayed = status != 1;
        }
    }
}
