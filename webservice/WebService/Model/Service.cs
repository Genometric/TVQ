using Genometric.TVQ.WebService.Model.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Genometric.TVQ.WebService.Model
{
    /// <summary>
    /// Implementation note: it would be ideal to implement this 
    /// type using generics so that it would have been cleaner to 
    /// implement a property such as `Jobs` (that returns all the 
    /// jobs created for the service), instead of the current 
    /// <see cref="GetJobs{T}"/>method. 
    /// However, due to "object-relational impedance mismatch",
    /// generic types cannot be used here, hence the 
    /// <see cref="GetJobs{T}"/> method is implemented.
    /// </summary>
    [JsonConverter(typeof(BaseJsonConverter))]
    public class Service : BaseModel
    {
        public enum Type { ToolRepoCrawler, LiteratureCrawler, Analysis };

        /// <summary>
        /// Implementation note:
        /// Name should be unique; and an enum type is used to 
        /// avoid using free text and make references/assertions 
        /// simpler. 
        /// </summary>
        public Type Name { set; get; }

        public int MaxDegreeOfParallelism { set; get; }

        public List<T> GetJobs<T>()
        {
            throw new NotImplementedException();
        }
    }
}
