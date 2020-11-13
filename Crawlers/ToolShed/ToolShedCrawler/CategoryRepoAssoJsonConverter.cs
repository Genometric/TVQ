using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;

namespace ToolShedCrawler
{
    public class CategoryRepoAssoJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _categoryMappings;
        private readonly Dictionary<string, string> _categoryRepoAssociationMappings;
        private readonly HashSet<string> _propertiesToSerialize;

        public CategoryRepoAssoJsonConverter()
        {
            _categoryMappings = new Dictionary<string, string>
            {
                { "name", nameof(Category.Name) },
                { "term", nameof(Category.Name) },
                { "uri", nameof(Category.URI) },
                { "description", nameof(Category.Description) }
            };

            _categoryRepoAssociationMappings = new Dictionary<string, string>
            {
                { "id", nameof(CategoryRepoAssociation.IDinRepo) },
            };

            _propertiesToSerialize = new HashSet<string>
            {
                nameof(CategoryRepoAssociation.Category),
                nameof(CategoryRepoAssociation.IDinRepo)
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
            var jsonObj = JObject.Load(reader).ToString();
            var association = JsonConvert.DeserializeObject<CategoryRepoAssociation>(
                jsonObj,
                new JsonSerializerSettings
                {
                    ContractResolver = new CustomContractResolver(
                    typeof(CategoryRepoAssociation),
                    new BaseJsonConverter(propertyMappings: _categoryRepoAssociationMappings))
                });

            association.Category = JsonConvert.DeserializeObject<Category>(
                jsonObj,
                new JsonSerializerSettings
                {
                    ContractResolver = new CustomContractResolver(
                        typeof(Category),
                        new BaseJsonConverter(propertyMappings: _categoryMappings))
                });

            return association;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // This serializer can be greatly simplified using the BaseJsonConverter. 

            if (value == null) return;

            Contract.Requires(writer != null);
            Contract.Requires(serializer != null);

            Type type = value.GetType();
            serializer.Converters.Add(new StringEnumConverter());

            JObject obj = new JObject();
            var properties = new SortedSet<PropertyInfo>(type.GetProperties(), new PropertyComparer());
            foreach (PropertyInfo prop in properties)
            {
                if (Attribute.GetCustomAttribute(prop, typeof(JsonIgnoreAttribute)) != null ||
                    !_propertiesToSerialize.Contains(prop.Name))
                    continue;

                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);

                    if (propVal == null)
                        continue;
                    else if (prop.PropertyType == typeof(DateTime) ||
                             prop.PropertyType == typeof(DateTime?))
                        obj.Add(prop.Name,
                                // U: Universal full format; e.g., Monday, 09 March 2020 22:46:35
                                ((DateTime)propVal).ToString("U", CultureInfo.InvariantCulture));
                    else if (prop.Name == nameof(CategoryRepoAssociation.Category))
                    {

                    }
                    else
                    {
                        obj.Add(prop.Name,
                                JToken.FromObject(propVal, serializer));
                    }
                }
            }

            obj.WriteTo(writer);
        }
    }
}
