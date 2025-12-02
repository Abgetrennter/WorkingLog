using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
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
                    var serializer = new JavaScriptSerializer();
                    _store = serializer.Deserialize<TemplateStore>(json);
                    
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
                    var serializer = new JavaScriptSerializer();
                    var json = serializer.Serialize(_store);
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
                // 支持 {字段} 与 {字段:格式}
                result = Regex.Replace(result, "\\{([^:{}]+)(?::([^{}]+))?\\}", match =>
                {
                    var name = match.Groups[1].Value;
                    var format = match.Groups[2].Success ? match.Groups[2].Value : null;
                    object value = null;
                    if (fieldValues.ContainsKey(name)) value = fieldValues[name];

                    if (value == null) return string.Empty;

                    if (format != null)
                    {
                        if (value is DateTime dt)
                            return dt.ToString(format);
                        if (value is string s && DateTime.TryParse(s, out var parsed))
                            return parsed.ToString(format);
                        return Convert.ToString(value);
                    }
                    return Convert.ToString(value);
                });
            }
            
            // System fields
            result = result.Replace("{ItemTitle}", item?.ItemTitle ?? string.Empty);
            // result = result.Replace("{CategoryPath}", ...); // Need to reconstruct if needed

            return result;
        }

        #endregion
    }
}
