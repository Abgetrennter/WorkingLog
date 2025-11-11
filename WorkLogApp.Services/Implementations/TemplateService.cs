using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class TemplateService : ITemplateService
    {
        private TemplateRoot _templateRoot;
        private string _templatesPath;

        public bool LoadTemplates(string templatesJsonPath)
        {
            if (!File.Exists(templatesJsonPath)) return false;
            var json = File.ReadAllText(templatesJsonPath);
            var serializer = new JavaScriptSerializer();
            _templateRoot = serializer.Deserialize<TemplateRoot>(json);
            _templatesPath = templatesJsonPath;
            return _templateRoot?.Templates != null;
        }

        public bool SaveTemplates()
        {
            if (_templateRoot == null || _templateRoot.Templates == null) return false;
            if (string.IsNullOrWhiteSpace(_templatesPath)) return false;
            try
            {
                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(_templateRoot);
                var dir = Path.GetDirectoryName(_templatesPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_templatesPath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Render(string formatTemplate, Dictionary<string, object> fieldValues, WorkLogItem item)
        {
            var result = formatTemplate ?? string.Empty;
            if (fieldValues != null)
            {
                // 支持 {字段} 与 {字段:格式}（主要用于日期时间）
                result = Regex.Replace(result, "\\{([^:{}]+)(?::([^{}]+))?\\}", match =>
                {
                    var name = match.Groups[1].Value;
                    var format = match.Groups[2].Success ? match.Groups[2].Value : null;
                    object value = null;
                    if (fieldValues.ContainsKey(name)) value = fieldValues[name];

                    if (value == null) return string.Empty;

                    if (format != null)
                    {
                        // 日期格式化优先
                        if (value is DateTime dt)
                            return dt.ToString(format);
                        // 尝试解析字符串为日期
                        if (value is string s && DateTime.TryParse(s, out var parsed))
                            return parsed.ToString(format);
                        // 其他类型按 ToString 输出
                        return Convert.ToString(value);
                    }
                    return Convert.ToString(value);
                });
            }
            // 系统字段占位符示例
            result = result.Replace("{ItemTitle}", item?.ItemTitle ?? string.Empty);
            return result;
        }

        public CategoryTemplate GetCategoryTemplate(string categoryName)
        {
            if (_templateRoot == null || _templateRoot.Templates == null) return null;
            if (_templateRoot.Templates.TryGetValue(categoryName, out var cat))
            {
                return cat?.CategoryTemplate;
            }
            return null;
        }

        public IEnumerable<string> GetCategoryNames()
        {
            if (_templateRoot?.Templates == null) yield break;
            foreach (var key in _templateRoot.Templates.Keys)
                yield return key;
        }

        public bool AddOrUpdateCategoryTemplate(string categoryName, CategoryTemplate template)
        {
            if (_templateRoot == null) _templateRoot = new TemplateRoot { Templates = new Dictionary<string, TemplateCategory>() };
            if (_templateRoot.Templates == null) _templateRoot.Templates = new Dictionary<string, TemplateCategory>();
            _templateRoot.Templates[categoryName] = new TemplateCategory { CategoryTemplate = template };
            return true;
        }

        public bool RemoveCategory(string categoryName)
        {
            if (_templateRoot?.Templates == null) return false;
            return _templateRoot.Templates.Remove(categoryName);
        }
    }
}