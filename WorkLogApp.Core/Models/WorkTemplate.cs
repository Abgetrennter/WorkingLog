using System;
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
        
        // 其它辅助字段（向后兼容）
        public Dictionary<string, string> Placeholders { get; set; }
        public Dictionary<string, List<string>> Options { get; set; }
        
        /// <summary>
        /// 结构化字段定义（新格式，优先使用）
        /// </summary>
        public List<TemplateField> Fields { get; set; } = new List<TemplateField>();
        
        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = "1.0";
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 获取有效的字段定义列表
        /// 优先使用 Fields，如果为空则转换 Placeholders
        /// </summary>
        public List<TemplateField> GetEffectiveFields()
        {
            if (Fields != null && Fields.Count > 0)
            {
                return Fields;
            }
            
            // 从旧格式 Placeholders 转换
            var fields = new List<TemplateField>();
            if (Placeholders != null)
            {
                int order = 0;
                foreach (var kv in Placeholders)
                {
                    var field = new TemplateField
                    {
                        Key = kv.Key,
                        Name = kv.Key,
                        Type = kv.Value?.ToLowerInvariant() ?? "text",
                        Order = order++
                    };
                    
                    // 从 Options 中填充选项
                    if (Options != null && Options.TryGetValue(kv.Key, out var opts))
                    {
                        field.Options = opts;
                    }
                    
                    fields.Add(field);
                }
            }
            return fields;
        }
    }
}
