using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Model
{
    public class ToolRepoAssociationJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _propertyMappings;

        public ToolRepoAssociationJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>
            {
                {"times_downloaded", nameof(ToolRepoAssociation.TimesDownloaded)},
                {"user_id", nameof(ToolRepoAssociation.UserID)},
                {"id", nameof(ToolRepoAssociation.IDinRepo)},
                {"biotoolsID", nameof(ToolRepoAssociation.IDinRepo)},
                {"create_time", nameof(ToolRepoAssociation.DateAddedToRepository)},
                {"additionDate", nameof(ToolRepoAssociation.DateAddedToRepository)} // for bio.tools
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
            object instance = Activator.CreateInstance(objectType);
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

            return instance;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            JObject obj = new JObject();
            Type type = value.GetType();

            foreach (PropertyInfo prop in type.GetProperties())
                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                        obj.Add(prop.Name, JToken.FromObject(propVal, serializer));
                }

            obj.WriteTo(writer);
        }
    }
}
