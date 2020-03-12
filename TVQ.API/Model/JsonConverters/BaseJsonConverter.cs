using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class BaseJsonConverter : JsonConverter
    {
        private readonly bool _includeNullProperties;
        private readonly List<string> _propertiesToIgnore;
        private readonly Dictionary<string, string> _propertyMappings;

        public BaseJsonConverter()
        {
            _propertyMappings = new Dictionary<string, string>();
            _propertiesToIgnore = new List<string>();
        }

        public BaseJsonConverter(Dictionary<string, string> propertyMappings,
                                 List<string> propertiesToIgnore = null,
                                 bool includeNullProperties = false)
        {
            _propertyMappings = propertyMappings;

            // ?? is a null-coalescing operator
            _propertiesToIgnore = propertiesToIgnore ?? new List<string>();

            _includeNullProperties = includeNullProperties;
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
            Contract.Requires(reader != null);
            Contract.Requires(objectType != null);
            Contract.Requires(serializer != null);

            object instance = Activator.CreateInstance(objectType);

            var props = objectType.GetProperties(
                // Bitwise OR
                BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

            JObject obj = JObject.Load(reader);
            foreach (JProperty jsonProperty in obj.Properties())
            {
                if (!_propertyMappings.TryGetValue(jsonProperty.Name, out var name))
                    name = jsonProperty.Name;

                PropertyInfo prop = props.FirstOrDefault(
                    pi => pi.CanWrite && 
                    pi.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                try
                {
                    prop?.SetValue(
                        instance,
                        jsonProperty.Value.ToObject(prop.PropertyType, serializer));
                }
                catch (JsonSerializationException)
                {
                    continue;
                }
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;

            Contract.Requires(writer != null);
            Contract.Requires(serializer != null);

            Type type = value.GetType();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            JObject obj = new JObject();
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);
                    if (_propertiesToIgnore.Contains(prop.Name))
                        continue;

                    if (propVal == null)
                    {
                        if (_includeNullProperties)
                            obj.Add(prop.Name, null);
                        else
                            continue;
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                        obj.Add(prop.Name,
                                // U: Universal full format; e.g., Monday, 09 March 2020 22:46:35
                                ((DateTime)propVal).ToString("U", CultureInfo.InvariantCulture));
                    else
                        obj.Add(prop.Name,
                                JToken.FromObject(propVal, serializer));
                }
            }

            obj.WriteTo(writer);
        }
    }
}
