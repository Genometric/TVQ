﻿namespace Genometric.TVQ.API.Model.DTOs
{
    public class ToolDTO
    {
        public int ID { get; }

        public string Name { get; }

        public ToolDTO(Tool tool)
        {
            ID = tool.ID;
            Name = tool.Name;
        }
    }
}