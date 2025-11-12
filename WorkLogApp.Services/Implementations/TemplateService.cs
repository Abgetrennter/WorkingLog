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
            var catPath = fieldValues != null && fieldValues.TryGetValue("CategoryPath", out var cp)
                ? Convert.ToString(cp) ?? string.Empty
                : string.Empty;
            result = result.Replace("{CategoryPath}", catPath);
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

        public CategoryTemplate GetMergedCategoryTemplate(string categoryName)
        {
            if (_templateRoot == null || _templateRoot.Templates == null) return null;
            if (string.IsNullOrWhiteSpace(categoryName)) return null;

            var parts = categoryName.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var keys = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                keys.Add(string.Join("-", parts, 0, i + 1));
            }

            CategoryTemplate merged = null;
            foreach (var key in keys)
            {
                if (_templateRoot.Templates.TryGetValue(key, out var cat) && cat?.CategoryTemplate != null)
                {
                    var tpl = cat.CategoryTemplate;
                    if (merged == null)
                    {
                        merged = new CategoryTemplate
                        {
                            FormatTemplate = tpl.FormatTemplate ?? string.Empty,
                            Placeholders = tpl.Placeholders != null ? new Dictionary<string, string>(tpl.Placeholders, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                            Options = tpl.Options != null ? CloneOptions(tpl.Options) : new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                        };
                    }
                    else
                    {
                        var fmt = tpl.FormatTemplate;
                        if (!string.IsNullOrWhiteSpace(fmt))
                        {
                            if (string.IsNullOrEmpty(merged.FormatTemplate)) merged.FormatTemplate = fmt;
                            else merged.FormatTemplate = merged.FormatTemplate + Environment.NewLine + Environment.NewLine + fmt;
                        }

                        if (tpl.Placeholders != null)
                        {
                            foreach (var kv in tpl.Placeholders)
                            {
                                merged.Placeholders[kv.Key] = kv.Value;
                            }
                        }

                        if (tpl.Options != null)
                        {
                            foreach (var kv in tpl.Options)
                            {
                                if (!merged.Options.TryGetValue(kv.Key, out var list))
                                {
                                    merged.Options[kv.Key] = new List<string>(kv.Value ?? new List<string>());
                                }
                                else
                                {
                                    var set = new HashSet<string>(list);
                                    foreach (var opt in kv.Value ?? new List<string>())
                                    {
                                        if (set.Add(opt)) list.Add(opt);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return merged;
        }

        private static Dictionary<string, List<string>> CloneOptions(Dictionary<string, List<string>> src)
        {
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in src)
            {
                dict[kv.Key] = kv.Value != null ? new List<string>(kv.Value) : new List<string>();
            }
            return dict;
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