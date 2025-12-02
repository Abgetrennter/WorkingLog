using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class TemplateStore
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<WorkTemplate> Templates { get; set; } = new List<WorkTemplate>();
    }
}
