using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Model
{
    public class RepoCrawlingJob : BaseJob
    {
        public Repository Repository { set; get; }
    }
}
