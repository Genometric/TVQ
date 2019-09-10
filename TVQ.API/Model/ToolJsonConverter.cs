using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Model
{
    public class ToolJsonConverter : JsonConverter
    {
        private Dictionary<string, string> _propertyMappings;

        public ToolJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>
            {
                {"times_downloaded", nameof(Tool.TimesDownloaded)},
                {"user_id", nameof(Tool.UserID)},
                {"name", nameof(Tool.Name)},
                {"homepage_url", nameof(Tool.Homepage)},
                {"owner", nameof(Tool.Owner)},
                {"id", nameof(Tool.IDinRepo)},
                {"remote_repository_url", nameof(Tool.CodeRepo)},
                {"description", nameof(Tool.Description)}
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
            throw new NotImplementedException();
        }
    }
}
