using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Model
{
    public class CitationJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _propertyMappings;

        public CitationJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>
            {
                {"id", nameof(Citation.ID) },
                {"publication_id", nameof(Citation.PublicationID) },
                {"count", nameof(Citation.Count) },
                {"accumulated_count", nameof(Citation.AccumulatedCount) },
                {"date", nameof(Citation.Date) },
                {"source", nameof(Citation.Source) },
                {"publication", nameof(Citation.Publication) }
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
            if (value == null) return;

            JObject obj = new JObject();
            Type type = value.GetType();

            foreach (PropertyInfo prop in type.GetProperties())
                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                        obj.Add(
                            prop.Name,
                            prop.Name == nameof(Citation.Date) ?
                            ((DateTime)propVal).ToString("MMMM d, yyyy", CultureInfo.InvariantCulture) :
                            JToken.FromObject(propVal, serializer));
                }

            obj.WriteTo(writer);
        }
    }
}
