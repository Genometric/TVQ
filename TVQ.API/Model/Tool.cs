using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.Comparers;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Genometric.TVQ.API.Model
{
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Tool : BaseModel, IEquatable<Tool>
    {
        public string Name { set; get; }

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

            try
            {
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
            }
            catch (Exception e)
            {

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
    }
}
