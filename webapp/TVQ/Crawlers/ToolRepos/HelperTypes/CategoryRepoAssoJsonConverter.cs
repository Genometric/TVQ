using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Genometric.TVQ.API.Model.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Genometric.TVQ.API.Crawlers.ToolRepos.HelperTypes
{
    public class CategoryRepoAssoJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _categoryMappings;
        private readonly Dictionary<string, string> _categoryRepoAssociationMappings;

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
            throw new NotImplementedException();
        }
    }
}
