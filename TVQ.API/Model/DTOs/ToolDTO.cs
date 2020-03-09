namespace Genometric.TVQ.API.Model.DTOs
{
    public class ToolDTO : BaseModel
    {
        public string Name { get; }

        public ToolDTO(Tool tool)
        {
            ID = tool.ID;
            Name = tool.Name;
        }
    }
}
