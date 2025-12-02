using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    public class WorkTemplate
    {
        public string Id { get; set; }          // 唯一标识 (GUID)
        public string Name { get; set; }        // 模板名称
        public string CategoryId { get; set; }  // 所属分类ID
        
        public string Content { get; set; }     // 模板内容 (原 FormatTemplate)
        
        // 标签集合
        public List<string> Tags { get; set; } = new List<string>();
        
        // 其它辅助字段
        public Dictionary<string, string> Placeholders { get; set; }
        public Dictionary<string, List<string>> Options { get; set; }
    }
}
