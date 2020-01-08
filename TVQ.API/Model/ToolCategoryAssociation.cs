using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Model
{
    public class ToolCategoryAssociation
    {
        public int ID { set; get; }
        public Tool Tool { set; get; }
        public Category Category { set; get; }
    }
}
