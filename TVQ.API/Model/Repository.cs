namespace Genometric.TVQ.API.Model
{
    public class Repository
    {
        public enum Repo { ToolShed };

        public int ID { set; get; }

        public Repo? Name { set; get; }

        public string URI { set; get; }

        public Repository() { }
    }
}
