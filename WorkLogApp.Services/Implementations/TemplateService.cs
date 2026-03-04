using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class TemplateService : ITemplateService
    {
        private TemplateStore _store;
        private string _templatesPath;
        private readonly object _lock = new object();

        public bool LoadTemplates(string templatesJsonPath)
        {
            lock (_lock)
            {
                _templatesPath = templatesJsonPath;
                if (!File.Exists(templatesJsonPath))
                {
                    // Initialize empty store if file doesn't exist
                    _store = new TemplateStore();
                    return true;
                }

                try
                {
                    var json = File.ReadAllText(templatesJsonPath);
                    _store = JsonConvert.DeserializeObject<TemplateStore>(json);
                    
                    // Fallback if deserialization returns null (e.g. empty file)
                    if (_store == null) _store = new TemplateStore();
                    
                    // Ensure lists are not null
                    if (_store.Categories == null) _store.Categories = new List<Category>();
                    if (_store.Templates == null) _store.Templates = new List<WorkTemplate>();
                    
                    return true;
                }
                catch
                {
                    // On error (e.g. format mismatch), init empty
                    _store = new TemplateStore();
                    return false;
                }
            }
        }

        public bool SaveTemplates()
        {
            lock (_lock)
            {
                if (_store == null || string.IsNullOrWhiteSpace(_templatesPath)) return false;
                try
                {
                    var json = JsonConvert.SerializeObject(_store, Formatting.Indented);
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
        }

        #region Category Operations

        public List<Category> GetAllCategories()
        {
            lock (_lock)
            {
                // Return flat list, UI can build tree
                // Or we could build tree here if needed, but let's return all flat data
                // Sort by SortOrder
                return _store?.Categories.OrderBy(c => c.SortOrder).ToList() ?? new List<Category>();
            }
        }

        public Category GetCategory(string id)
        {
            lock (_lock)
            {
                return _store?.Categories.FirstOrDefault(c => c.Id == id);
            }
        }

        public Category CreateCategory(string name, string parentId)
        {
            lock (_lock)
            {
                if (_store == null) _store = new TemplateStore();

                // Check for duplicate name
                if (_store.Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"分类名称 '{name}' 已存在。");
                }

                var category = new Category
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    ParentId = parentId,
                    SortOrder = 0 // Default to top or bottom? Let's say 0.
                };
                _store.Categories.Add(category);
                SaveTemplates();
                return category;
            }
        }

        public bool UpdateCategory(Category category)
        {
            lock (_lock)
            {
                var existing = _store?.Categories.FirstOrDefault(c => c.Id == category.Id);
                if (existing == null) return false;

                // Check for duplicate name if name changed
                if (!string.Equals(existing.Name, category.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (_store.Categories.Any(c => c.Id != category.Id && string.Equals(c.Name, category.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException($"分类名称 '{category.Name}' 已存在。");
                    }
                }

                existing.Name = category.Name;
                existing.SortOrder = category.SortOrder;
                // ParentId usually changed via Move
                
                return SaveTemplates();
            }
        }

        public bool DeleteCategory(string id)
        {
            lock (_lock)
            {
                if (_store == null) return false;
                
                // Recursive check? For now, simple delete. 
                // UI should handle warnings about children.
                // But we should probably delete children too to avoid orphans?
                // Or just delete the node and let orphans be orphans (bad idea).
                // Let's delete the node.
                
                var toDelete = _store.Categories.FirstOrDefault(c => c.Id == id);
                if (toDelete == null) return false;

                _store.Categories.Remove(toDelete);
                
                // Also remove templates associated? 
                // Maybe we should keep them or nullify their category?
                // Let's remove templates for now to keep it clean.
                _store.Templates.RemoveAll(t => t.CategoryId == id);

                // Remove children recursively? 
                // A robust implementation would find all descendants.
                var children = _store.Categories.Where(c => c.ParentId == id).ToList();
                foreach(var child in children)
                {
                    DeleteCategory(child.Id); // Recursive delete
                }

                return SaveTemplates();
            }
        }

        public bool MoveCategory(string id, string newParentId)
        {
            lock (_lock)
            {
                var cat = _store?.Categories.FirstOrDefault(c => c.Id == id);
                if (cat == null) return false;
                
                // Prevent circular reference
                if (id == newParentId) return false;
                
                // TODO: Check if newParentId is a child of id (prevent making a node its own descendant)
                if (IsDescendant(id, newParentId)) return false;

                cat.ParentId = newParentId;
                return SaveTemplates();
            }
        }

        private bool IsDescendant(string potentialAncestorId, string potentialDescendantId)
        {
            if (string.IsNullOrEmpty(potentialDescendantId)) return false;
            var current = GetCategory(potentialDescendantId);
            while (current != null)
            {
                if (current.ParentId == potentialAncestorId) return true;
                if (string.IsNullOrEmpty(current.ParentId)) break;
                current = GetCategory(current.ParentId);
            }
            return false;
        }

        #endregion

        #region Template Operations

        public List<WorkTemplate> GetTemplatesByCategory(string categoryId)
        {
            lock (_lock)
            {
                return _store?.Templates.Where(t => t.CategoryId == categoryId).ToList() ?? new List<WorkTemplate>();
            }
        }

        public WorkTemplate GetTemplate(string id)
        {
            lock (_lock)
            {
                return _store?.Templates.FirstOrDefault(t => t.Id == id);
            }
        }

        public WorkTemplate CreateTemplate(WorkTemplate template)
        {
            lock (_lock)
            {
                if (_store == null) _store = new TemplateStore();
                if (string.IsNullOrEmpty(template.Id)) template.Id = Guid.NewGuid().ToString();
                
                _store.Templates.Add(template);
                SaveTemplates();
                return template;
            }
        }

        public bool UpdateTemplate(WorkTemplate template)
        {
            lock (_lock)
            {
                if (_store == null) return false;
                var index = _store.Templates.FindIndex(t => t.Id == template.Id);
                if (index < 0) return false;

                _store.Templates[index] = template;
                return SaveTemplates();
            }
        }

        public bool DeleteTemplate(string id)
        {
            lock (_lock)
            {
                if (_store == null) return false;
                var count = _store.Templates.RemoveAll(t => t.Id == id);
                return count > 0 && SaveTemplates();
            }
        }

        #endregion

        #region Rendering

        public string Render(string content, Dictionary<string, object> fieldValues, WorkLogItem item)
        {
            var result = content ?? string.Empty;
            if (fieldValues != null)
            {
                // 支持 {字段} 与 {字段:格式} 与 {字段|函数}
                result = Regex.Replace(result, "\\{([^:{}|]+)(?::([^{}|]+))?(?:\\|([^{}]+))?\\}", match =>
                {
                    var name = match.Groups[1].Value.Trim();
                    var format = match.Groups[2].Success ? match.Groups[2].Value : null;
                    var function = match.Groups[3].Success ? match.Groups[3].Value : null;
                    
                    object value = null;
                    if (fieldValues.ContainsKey(name)) value = fieldValues[name];

                    if (value == null) return string.Empty;

                    // 应用格式
                    string formattedValue = Convert.ToString(value);
                    if (format != null)
                    {
                        if (value is DateTime dt)
                            formattedValue = dt.ToString(format);
                        else if (value is string s && DateTime.TryParse(s, out var parsed))
                            formattedValue = parsed.ToString(format);
                    }

                    // 应用函数
                    if (function != null)
                    {
                        formattedValue = ApplyFunction(formattedValue, function, fieldValues);
                    }

                    return formattedValue;
                });
            }
            
            // System fields
            result = result.Replace("{ItemTitle}", item?.ItemTitle ?? string.Empty);
            result = result.Replace("{LogDate}", item?.LogDate.ToString("yyyy-MM-dd") ?? string.Empty);
            result = result.Replace("{Today}", DateTime.Now.ToString("yyyy-MM-dd"));
            result = result.Replace("{Now}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            return result;
        }

        /// <summary>
        /// 应用函数转换
        /// </summary>
        private string ApplyFunction(string value, string function, Dictionary<string, object> fieldValues)
        {
            var parts = function.Split(':');
            var funcName = parts[0].Trim().ToLowerInvariant();
            var args = parts.Length > 1 ? parts[1].Split(',') : new string[0];

            switch (funcName)
            {
                case "trim":
                    return value.Trim();
                
                case "upper":
                    return value.ToUpperInvariant();
                
                case "lower":
                    return value.ToLowerInvariant();
                
                case "lines":
                    // 将句号分隔的文本转换为多行
                    return value.Replace("。", "。\n").Replace("；", "；\n");
                
                case "duration":
                    // 计算两个时间之间的时长
                    if (args.Length > 0 && fieldValues.TryGetValue(args[0].Trim(), out var endValue))
                    {
                        if (DateTime.TryParse(value, out var start) && DateTime.TryParse(Convert.ToString(endValue), out var end))
                        {
                            var duration = end - start;
                            if (duration.TotalHours >= 1)
                                return $"{duration.TotalHours:F1}小时";
                            else
                                return $"{duration.TotalMinutes:F0}分钟";
                        }
                    }
                    return value;
                
                case "format":
                    // 自定义格式
                    if (args.Length > 0)
                    {
                        if (DateTime.TryParse(value, out var dt))
                            return dt.ToString(args[0]);
                    }
                    return value;
                
                case "default":
                    // 如果为空则使用默认值
                    if (string.IsNullOrWhiteSpace(value) && args.Length > 0)
                        return args[0];
                    return value;
                
                case "prefix":
                    // 添加前缀
                    if (args.Length > 0 && !string.IsNullOrWhiteSpace(value))
                        return args[0] + value;
                    return value;
                
                case "suffix":
                    // 添加后缀
                    if (args.Length > 0 && !string.IsNullOrWhiteSpace(value))
                        return value + args[0];
                    return value;
                
                case "hideif":
                    // 如果等于指定值则返回空
                    if (args.Length > 0 && value == args[0])
                        return string.Empty;
                    return value;
                
                default:
                    return value;
            }
        }

        /// <summary>
        /// 渲染模板（使用结构化字段定义）
        /// </summary>
        public string RenderTemplate(WorkTemplate template, Dictionary<string, object> fieldValues, WorkLogItem item)
        {
            if (template == null) return string.Empty;
            return Render(template.Content, fieldValues, item);
        }

        #endregion
    }
}
