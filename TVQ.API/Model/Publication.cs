using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genometric.TVQ.API.Model
{
    public class Publication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        public int ToolId { set; get; }

        public int ExternalID { set; get; }

        public string Title { set; get; }

        public string Year { set; get; }

        public int CitedBy { set; get; }

        public string DOI { set; get; }

        public string Citation { set; get; }
    }
}
