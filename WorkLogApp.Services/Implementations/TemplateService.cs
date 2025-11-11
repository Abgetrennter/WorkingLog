using System.Collections.Generic;
using System.IO;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class TemplateService : ITemplateService
    {
        public bool LoadTemplates(string templatesJsonPath)
        {
            // 仅检查文件是否存在；后续接入 Newtonsoft.Json 解析
            return File.Exists(templatesJsonPath);
        }

        public string Render(string formatTemplate, Dictionary<string, object> fieldValues, WorkLogItem item)
        {
            var result = formatTemplate ?? string.Empty;
            if (fieldValues != null)
            {
                foreach (var kv in fieldValues)
                {
                    var placeholder = "{" + kv.Key + "}";
                    result = result.Replace(placeholder, kv.Value?.ToString() ?? string.Empty);
                }
            }
            // 系统字段占位符示例
            result = result.Replace("{ItemTitle}", item?.ItemTitle ?? string.Empty);
            return result;
        }
    }
}