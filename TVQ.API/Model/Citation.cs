using System;

namespace Genometric.TVQ.API.Model
{
    public class Citation
    {
        public enum InfoSource { Scopus };

        public int ID { set; get; }

        public int PublicationID { set; get; }

        public int Count { set; get; }

        public DateTime Date { set; get; }

        public InfoSource? Source { set; get; }

        public virtual Publication Publication { set; get; }
    }
}
