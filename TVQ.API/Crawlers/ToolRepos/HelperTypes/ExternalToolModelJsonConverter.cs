using Genometric.TVQ.API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes
{
    public class ExternalToolModelJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _propertyMappings;

        public ExternalToolModelJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>
            {
                {"topic", nameof(DeserializedInfo.Categories)},
                {"category_ids", nameof(DeserializedInfo.CategoryIDs)},
                {"publication", nameof(DeserializedInfo.Publications)}

                /// Why not reading Bio.Tools Publication.Metadata?
                /// A JSON object from Bio.Tools contains a field named "meta-data" for 
                /// each publication. This field (at the time of writing this) is not 
                /// set for every publication. Hence, it can be more reliable to 
                /// capture only DOI and/or PubMedID and query details of each 
                /// publication from Scopus. 
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsClass;
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var instance = new DeserializedInfo();
            var props = objectType.GetTypeInfo().DeclaredProperties.ToList();

            JObject obj = JObject.Load(reader);
            foreach (JProperty jsonProperty in obj.Properties())
            {
                if (!_propertyMappings.TryGetValue(jsonProperty.Name, out var name))
                    name = jsonProperty.Name;

                PropertyInfo prop = props.FirstOrDefault(
                    pi => pi.CanWrite && pi.Name == name);

                prop?.SetValue(
                    instance,
                    jsonProperty.Value.ToObject(prop.PropertyType, serializer));
            }

            instance.ToolRepoAssociation = JsonConvert.DeserializeObject<ToolRepoAssociation>(obj.ToString());
            instance.ToolRepoAssociation.Tool = JsonConvert.DeserializeObject<Tool>(obj.ToString());
            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

