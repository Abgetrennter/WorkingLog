using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WorkLogApp.Core.Helpers;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    /// <summary>
    /// 模板服务实现类（CQRS 统一实现）
    /// 实现 ITemplateService 接口，提供模板和分类的 CRUD 操作以及渲染功能
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private TemplateStore _templateStore;
        private string _templatesPath;
        private readonly System.Threading.ReaderWriterLockSlim _rwLock = new System.Threading.ReaderWriterLockSlim();

        public bool LoadTemplates(string templatesJsonPath)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _templatesPath = templatesJsonPath;
                if (!File.Exists(templatesJsonPath))
                {
                    Logger.Warning($"模板文件不存在: {templatesJsonPath}，将使用空存储");
                    _templateStore = new TemplateStore();
                    return true;
                }

                try
                {
                    var json = File.ReadAllText(templatesJsonPath);
                    _templateStore = JsonConvert.DeserializeObject<TemplateStore>(json);

                    // Fallback if deserialization returns null (e.g. empty file)
                    if (_templateStore == null)
                    {
                        Logger.Warning($"模板文件反序列化返回 null: {templatesJsonPath}");
                        _templateStore = new TemplateStore();
                    }

                    // Ensure lists are not null
                    if (_templateStore.Categories == null) _templateStore.Categories = new List<Category>();
                    if (_templateStore.Templates == null) _templateStore.Templates = new List<WorkTemplate>();

                    return true;
                }
                catch (JsonException ex)
                {
                    Logger.Error($"模板文件 JSON 格式错误: {templatesJsonPath}", ex);
                    _templateStore = new TemplateStore();
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"加载模板文件失败: {templatesJsonPath}", ex);
                    _templateStore = new TemplateStore();
                    return false;
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool SaveTemplates()
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (_templateStore == null || string.IsNullOrWhiteSpace(_templatesPath))
                {
                    Logger.Error("保存模板失败: store 为 null 或路径为空");
                    return false;
                }
                try
                {
                    var json = JsonConvert.SerializeObject(_templateStore, Formatting.Indented);
                    var dir = Path.GetDirectoryName(_templatesPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(_templatesPath, json);
                    Logger.Info($"模板已保存: {_templatesPath}");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"保存模板失败: {_templatesPath}", ex);
                    return false;
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 内部保存方法，假设调用者已持有写锁
        /// </summary>
        private bool SaveTemplatesInternal()
        {
            if (_templateStore == null || string.IsNullOrWhiteSpace(_templatesPath))
            {
                Logger.Error("保存模板失败: store 为 null 或路径为空");
                return false;
            }
            try
            {
                var json = JsonConvert.SerializeObject(_templateStore, Formatting.Indented);
                var dir = Path.GetDirectoryName(_templatesPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_templatesPath, json);
                Logger.Info($"模板已保存: {_templatesPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"保存模板失败: {_templatesPath}", ex);
                return false;
            }
        }

        #region Category Operations

        /// <summary>
        /// 获取所有分类（返回只读列表，防止外部修改内部数据）
        /// </summary>
        /// <returns>分类列表，按 SortOrder 排序</returns>
        public IReadOnlyList<Category> GetAllCategories()
        {
            _rwLock.EnterReadLock();
            try
            {
                // 返回只读列表，防止外部修改内部数据
                // UI 可以根据需要构建树形结构
                // 按 SortOrder 排序
                return _templateStore?.Categories.OrderBy(c => c.SortOrder).ToList() ?? new List<Category>();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 根据 ID 获取分类
        /// </summary>
        /// <param name="id">分类 ID</param>
        /// <returns>分类对象，如果不存在则返回 null</returns>
        public Category GetCategory(string id)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _templateStore?.Categories.FirstOrDefault(c => c.Id == id);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public Category CreateCategory(string name, string parentId)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (_templateStore == null) _templateStore = new TemplateStore();

                // Check for duplicate name
                if (_templateStore.Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
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
                _templateStore.Categories.Add(category);
                SaveTemplatesInternal();
                return category;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool UpdateCategory(Category category)
        {
            _rwLock.EnterWriteLock();
            try
            {
                var existing = _templateStore?.Categories.FirstOrDefault(c => c.Id == category.Id);
                if (existing == null) return false;

                // Check for duplicate name if name changed
                if (!string.Equals(existing.Name, category.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (_templateStore.Categories.Any(c => c.Id != category.Id && string.Equals(c.Name, category.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException($"分类名称 '{category.Name}' 已存在。");
                    }
                }

                existing.Name = category.Name;
                existing.SortOrder = category.SortOrder;
                // ParentId usually changed via Move

                return SaveTemplatesInternal();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool DeleteCategory(string id)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (_templateStore == null) return false;

                // Recursive check? For now, simple delete.
                // UI should handle warnings about children.
                // But we should probably delete children too to avoid orphans?
                // Or just delete the node and let orphans be orphans (bad idea).
                // Let's delete the node.

                var toDelete = _templateStore.Categories.FirstOrDefault(c => c.Id == id);
                if (toDelete == null) return false;

                _templateStore.Categories.Remove(toDelete);

                // Also remove templates associated?
                // Maybe we should keep them or nullify their category?
                // Let's remove templates for now to keep it clean.
                _templateStore.Templates.RemoveAll(t => t.CategoryId == id);

                // Remove children recursively?
                // A robust implementation would find all descendants.
                // 收集所有子分类ID，避免递归调用时的锁问题
                var childrenIds = new List<string>();
                CollectChildrenIds(id, childrenIds);
                foreach (var childId in childrenIds)
                {
                    _templateStore.Categories.RemoveAll(c => c.Id == childId);
                    _templateStore.Templates.RemoveAll(t => t.CategoryId == childId);
                }

                return SaveTemplatesInternal();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 辅助方法：收集所有子分类ID
        /// </summary>
        private void CollectChildrenIds(string parentId, List<string> childrenIds)
        {
            var children = _templateStore?.Categories.Where(c => c.ParentId == parentId).ToList();
            if (children == null) return;

            foreach (var child in children)
            {
                childrenIds.Add(child.Id);
                CollectChildrenIds(child.Id, childrenIds);
            }
        }

        public bool MoveCategory(string id, string newParentId)
        {
            _rwLock.EnterWriteLock();
            try
            {
                var cat = _templateStore?.Categories.FirstOrDefault(c => c.Id == id);
                if (cat == null) return false;

                // Prevent circular reference
                if (id == newParentId) return false;

                // Check if newParentId is a child of id (prevent making a node its own descendant)
                if (IsDescendant(id, newParentId)) return false;

                cat.ParentId = newParentId;
                return SaveTemplatesInternal();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        private bool IsDescendant(string potentialAncestorId, string potentialDescendantId)
        {
            if (string.IsNullOrEmpty(potentialDescendantId)) return false;

            // 直接访问 _templateStore避免锁递归
            var current = _templateStore?.Categories.FirstOrDefault(c => c.Id == potentialDescendantId);
            while (current != null)
            {
                if (current.ParentId == potentialAncestorId) return true;
                if (string.IsNullOrEmpty(current.ParentId)) break;
                current = _templateStore?.Categories.FirstOrDefault(c => c.Id == current.ParentId);
            }
            return false;
        }

        #endregion

        #region Template Operations

        /// <summary>
        /// 根据分类 ID 获取模板列表（返回只读列表，防止外部修改内部数据）
        /// </summary>
        /// <param name="categoryId">分类 ID</param>
        /// <returns>模板列表</returns>
        public IReadOnlyList<WorkTemplate> GetTemplatesByCategory(string categoryId)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _templateStore?.Templates.Where(t => t.CategoryId == categoryId).ToList() ?? new List<WorkTemplate>();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 根据 ID 获取模板
        /// </summary>
        /// <param name="id">模板 ID</param>
        /// <returns>模板对象，如果不存在则返回 null</returns>
        public WorkTemplate GetTemplate(string id)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _templateStore?.Templates.FirstOrDefault(t => t.Id == id);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public WorkTemplate CreateTemplate(WorkTemplate template)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (_templateStore == null) _templateStore = new TemplateStore();
                if (string.IsNullOrEmpty(template.Id)) template.Id = Guid.NewGuid().ToString();

                _templateStore.Templates.Add(template);
                SaveTemplatesInternal();
                return template;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool UpdateTemplate(WorkTemplate template)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (_templateStore == null) return false;
                var index = _templateStore.Templates.FindIndex(t => t.Id == template.Id);
                if (index < 0) return false;

                _templateStore.Templates[index] = template;
                return SaveTemplatesInternal();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool DeleteTemplate(string id)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (_templateStore == null) return false;
                var count = _templateStore.Templates.RemoveAll(t => t.Id == id);
                return count > 0 && SaveTemplatesInternal();
            }
            finally
            {
                _rwLock.ExitWriteLock();
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
