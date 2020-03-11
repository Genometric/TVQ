using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.API.Model.JsonConverters
{
    public class CustomContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, JsonConverter> _converters;

        public CustomContractResolver(Type converterType, JsonConverter converter)
        {
            _converters = new Dictionary<Type, JsonConverter>
            {
                {converterType, converter }
            };
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);
            if (_converters.TryGetValue(objectType, out JsonConverter converter))
                contract.Converter = converter;
            
            return contract;
        }
    }
}
