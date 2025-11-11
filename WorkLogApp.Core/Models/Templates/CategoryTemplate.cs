using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class CategoryTemplate
    {
        public string FormatTemplate { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
        // 可选：为 select/checkbox 类型提供选项列表
        public Dictionary<string, List<string>> Options { get; set; }
    }
}