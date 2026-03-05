using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// 模板服务接口
    /// 提供分类和模板的 CRUD 操作，以及模板渲染功能
    /// </summary>
    public interface ITemplateService
    {
        #region 初始化

        /// <summary>
        /// 从指定路径加载模板数据
        /// </summary>
        /// <param name="templatesJsonPath">模板 JSON 文件路径</param>
        /// <returns>是否加载成功</returns>
        bool LoadTemplates(string templatesJsonPath);

        /// <summary>
        /// 保存模板数据到文件
        /// </summary>
        /// <returns>是否保存成功</returns>
        bool SaveTemplates();

        #endregion

        #region 分类操作

        /// <summary>
        /// 获取所有分类（返回只读列表，防止外部修改内部数据）
        /// </summary>
        /// <returns>分类列表，按 SortOrder 排序</returns>
        IReadOnlyList<Category> GetAllCategories();

        /// <summary>
        /// 根据 ID 获取分类
        /// </summary>
        /// <param name="id">分类 ID</param>
        /// <returns>分类对象，如果不存在则返回 null</returns>
        Category GetCategory(string id);

        /// <summary>
        /// 创建新分类
        /// </summary>
        /// <param name="name">分类名称</param>
        /// <param name="parentId">父分类 ID，null 表示顶级分类</param>
        /// <returns>创建的分类对象</returns>
        Category CreateCategory(string name, string parentId);

        /// <summary>
        /// 更新分类信息
        /// </summary>
        /// <param name="category">要更新的分类对象</param>
        /// <returns>是否更新成功</returns>
        bool UpdateCategory(Category category);

        /// <summary>
        /// 删除分类及其子分类和关联模板
        /// </summary>
        /// <param name="id">要删除的分类 ID</param>
        /// <returns>是否删除成功</returns>
        bool DeleteCategory(string id);

        /// <summary>
        /// 移动分类到新的父分类
        /// </summary>
        /// <param name="id">要移动的分类 ID</param>
        /// <param name="newParentId">新的父分类 ID，null 表示移动到顶级</param>
        /// <returns>是否移动成功</returns>
        bool MoveCategory(string id, string newParentId);

        #endregion

        #region 模板操作

        /// <summary>
        /// 根据分类 ID 获取模板列表（返回只读列表，防止外部修改内部数据）
        /// </summary>
        /// <param name="categoryId">分类 ID</param>
        /// <returns>模板列表</returns>
        IReadOnlyList<WorkTemplate> GetTemplatesByCategory(string categoryId);

        /// <summary>
        /// 根据 ID 获取模板
        /// </summary>
        /// <param name="id">模板 ID</param>
        /// <returns>模板对象，如果不存在则返回 null</returns>
        WorkTemplate GetTemplate(string id);

        /// <summary>
        /// 创建新模板
        /// </summary>
        /// <param name="template">要创建的模板对象</param>
        /// <returns>创建的模板对象</returns>
        WorkTemplate CreateTemplate(WorkTemplate template);

        /// <summary>
        /// 更新模板信息
        /// </summary>
        /// <param name="template">要更新的模板对象</param>
        /// <returns>是否更新成功</returns>
        bool UpdateTemplate(WorkTemplate template);

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="id">要删除的模板 ID</param>
        /// <returns>是否删除成功</returns>
        bool DeleteTemplate(string id);

        #endregion

        #region 渲染

        /// <summary>
        /// 渲染模板内容，替换占位符为实际值
        /// </summary>
        /// <param name="content">模板内容</param>
        /// <param name="values">字段值字典</param>
        /// <param name="item">工作日志项</param>
        /// <returns>渲染后的内容</returns>
        string Render(string content, Dictionary<string, object> values, WorkLogItem item);

        #endregion
    }
}
