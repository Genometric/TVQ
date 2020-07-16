﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Genometric.TVQ.WebService.Model.JsonConverters
{
    public class BaseJsonConverter : JsonConverter
    {
        private readonly bool _includeNullProperties;
        private readonly Dictionary<string, string> _propertyMappings;

        public BaseJsonConverter()
        {
            _propertyMappings = null;
        }

        public BaseJsonConverter(Dictionary<string, string> propertyMappings,
                                 bool includeNullProperties = false)
        {
            _propertyMappings = propertyMappings;
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
                string name;
                if (_propertyMappings == null)
                    name = jsonProperty.Name;
                else if (!_propertyMappings.TryGetValue(jsonProperty.Name, out name))
                    continue;

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
            serializer.Converters.Add(new StringEnumConverter());

            JObject obj = new JObject();
            var properties = new SortedSet<PropertyInfo>(type.GetProperties(), new PropertyComparer());
            foreach (PropertyInfo prop in properties)
            {
                if (Attribute.GetCustomAttribute(prop, typeof(JsonIgnoreAttribute)) != null)
                    continue;

                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);

                    if (propVal == null)
                    {
                        if (_includeNullProperties)
                            obj.Add(prop.Name, null);
                        else
                            continue;
                    }
                    else if (prop.PropertyType == typeof(DateTime) ||
                             prop.PropertyType == typeof(DateTime?))
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
