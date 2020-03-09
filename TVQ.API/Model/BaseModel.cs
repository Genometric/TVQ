using System;

namespace Genometric.TVQ.API.Model
{
    public class BaseModel
    {
        public int ID { set; get; }

        public DateTime CreateDate { set; get; }

        public DateTime LastUpdate { set; get; }
    }
}
