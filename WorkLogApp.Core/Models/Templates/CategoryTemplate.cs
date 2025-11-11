using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class CategoryTemplate
    {
        public string FormatTemplate { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
    }
}