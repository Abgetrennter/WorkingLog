using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// 模板命令服务接口（CQRS - Command 侧）
    /// 仅提供写入操作（创建、更新、删除），不包含任何查询方法
    /// </summary>
    public interface ITemplateCommandService
    {
        #region 分类命令

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

        #region 模板命令

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
    }
}
