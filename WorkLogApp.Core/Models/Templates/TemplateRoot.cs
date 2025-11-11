using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class TemplateRoot
    {
        public Dictionary<string, TemplateCategory> Templates { get; set; }
    }
}