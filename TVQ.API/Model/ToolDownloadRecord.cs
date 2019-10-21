using System;

namespace Genometric.TVQ.API.Model
{
    public class ToolDownloadRecord
    {
        public int ID { set; get; }

        public int ToolID { set; get; }

        public int Count { set; get; }

        public DateTime Date { set; get; }

        public Tool Tool { set; get; }

        public ToolDownloadRecord() { }
    }
}
