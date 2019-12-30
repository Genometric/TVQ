using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Model
{
    public class RepoToolJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _propertyMappings;

        public RepoToolJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>
            {
                {"times_downloaded", nameof(RepoTool.TimesDownloaded)},
                {"user_id", nameof(RepoTool.UserID)},
                {"name", nameof(RepoTool.Name)},
                {"homepage", nameof(RepoTool.Homepage)},
                {"homepage_url", nameof(RepoTool.Homepage)},
                {"owner", nameof(RepoTool.Owner)},
                {"id", nameof(RepoTool.IDinRepo)},
                {"biotoolsID", nameof(RepoTool.IDinRepo)},
                {"remote_repository_url", nameof(RepoTool.CodeRepo)},
                {"description", nameof(RepoTool.Description)},
                {"create_time", nameof(RepoTool.DateAddedToRepository)},
                {"additionDate", nameof(RepoTool.DateAddedToRepository)} // for bio.tools
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

