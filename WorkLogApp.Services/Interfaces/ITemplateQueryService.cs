using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// 模板查询服务接口（CQRS - Query 侧）
    /// 仅提供只读操作，不包含任何修改数据的方法
    /// </summary>
    public interface ITemplateQueryService
    {
        #region 分类查询

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

        #endregion

        #region 模板查询

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

        #endregion
    }
}
