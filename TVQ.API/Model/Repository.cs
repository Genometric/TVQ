using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genometric.TVQ.API.Model
{
    public class Repository
    {
        public enum Repo { ToolShed, BioTools };

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        public Repo? Name { set; get; }

        public string URI { set; get; }

        public int ToolCount { set; get; }

        public Repository() { }

        public Uri GetURI()
        {
            return new Uri(URI);
        }
    }
}
