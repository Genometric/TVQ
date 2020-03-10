using Genometric.TVQ.API.Model.Associations;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class ToolRepoAssociationJsonConverter : BaseJsonConverter
    {
        public ToolRepoAssociationJsonConverter() : base(
            propertyMappings: new Dictionary<string, string>
            {
                {"times_downloaded", nameof(ToolRepoAssociation.TimesDownloaded)},
                {"user_id", nameof(ToolRepoAssociation.UserID)},
                {"id", nameof(ToolRepoAssociation.IDinRepo)},
                {"biotoolsID", nameof(ToolRepoAssociation.IDinRepo)},
                {"create_time", nameof(ToolRepoAssociation.DateAddedToRepository)},
                {"additionDate", nameof(ToolRepoAssociation.DateAddedToRepository)} // for bio.tools
            })
        { }
    }
}
