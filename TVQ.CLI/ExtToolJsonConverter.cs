using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.CLI
{
    public class ExtToolJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _propertyMappings;

        public ExtToolJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>
            {
                {"Id", nameof(ExtTool.ID)},
                {"CodeRepo", nameof(ExtTool.CodeRepo)},
                {"Description", nameof(ExtTool.Description)},
                {"Homepage", nameof(ExtTool.Homepage)},
                {"IDinRepo", nameof(ExtTool.IDinRepo)},
                {"Name", nameof(ExtTool.Name)},
                {"Owner", nameof(ExtTool.Owner)},
                {"TimesDownloaded", nameof(ExtTool.TimesDownloaded)},
                {"UserID", nameof(ExtTool.UserID)},

                /// May not need the following two.
                {"RepositoryID", nameof(ExtTool.RepoID)}
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
            IEnumerable<PropertyInfo> props = GetAllProperties(objectType);

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

        IEnumerable<PropertyInfo> GetAllProperties(Type T)
        {
            IEnumerable<PropertyInfo> props = T.GetTypeInfo().DeclaredProperties.ToList();
            if (T.GetTypeInfo().BaseType != null)
                props = props.Concat(GetAllProperties(T.GetTypeInfo().BaseType));
            return props;
        }
    }
}
