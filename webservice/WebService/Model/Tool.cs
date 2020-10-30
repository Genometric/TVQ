using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.Comparers;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Genometric.TVQ.WebService.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Tool : BaseModel, IEquatable<Tool>
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null)
                {
                    _name = null;
                }
                else
                {
                    _name = RemoveNamePrePostFix(value);
                }
            }
        }

        public string Homepage { set; get; }

        public string CodeRepo { set; get; }

        public string Description { set; get; }

        public virtual ICollection<ToolRepoAssociation> RepoAssociations { set; get; }

        public virtual ICollection<ToolCategoryAssociation> CategoryAssociations { set; get; }

        public virtual ICollection<ToolPublicationAssociation> PublicationAssociations { set; get; }

        public Tool()
        {
            RepoAssociations = new List<ToolRepoAssociation>();
            CategoryAssociations = new List<ToolCategoryAssociation>();
            PublicationAssociations = new List<ToolPublicationAssociation>();
        }

        public SortedList<DateTime, Publication> GetSortedPublications()
        {
            // Do NOT use SortedDictionary because there could be 
            // multiple publications with the same publication date.
            // The custom comparer used here handles duplicates as greater. 
            var rtv = new SortedList<DateTime, Publication>(new DuplicateKeyComparer<DateTime>());

            foreach (var pubAssociation in PublicationAssociations)
            {
                // If year is null, then do not consider the publication.
                int year = pubAssociation.Publication.Year ?? 0;
                if (year == 0)
                    continue;

                rtv.Add(
                    new DateTime(year,
                                 pubAssociation.Publication.Month ?? 1,
                                 pubAssociation.Publication.Day ?? 1),
                    pubAssociation.Publication);
            }

            return rtv;
        }

        public bool Equals([AllowNull] Tool other)
        {
            return ID == other?.ID;
        }

        public override string ToString()
        {
            return ID + "\t" + Name;
        }

        /// <summary>
        /// Removes some common pre- and post-fixes from tool name.
        /// </summary>
        /// <param name="name">Tool name.</param>
        /// <returns>Tool name with trimmed pre- and postfixes.</returns>
        public static string RemoveNamePrePostFix(string name)
        {
            Contract.Requires(name != null);

            name = name.Trim();

            /// The following code removes some common prefixes, 
            /// in order to better match tools between repositories using their names.
            name = Utilities.RemovePrefix(name, "bioconductor-");
            name = Utilities.RemovePrefix(name, "r-");
            name = Utilities.RemovePrefix(name, "perl-");
            name = Utilities.RemovePrefix(name, "ucsc-");

            /// The following code removes version postfix.
            var index = name.LastIndexOf('\\');
            if (index != -1)
                name = name.Remove(index, name.Length);

            return name;
        }
    }
}
