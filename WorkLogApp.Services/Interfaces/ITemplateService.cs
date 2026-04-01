using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// 模板服务接口（CQRS 统一接口）
    /// 继承自 ITemplateQueryService 和 ITemplateCommandService，提供完整的 CRUD 操作
    /// 以及模板渲染功能
    /// </summary>
    public interface ITemplateService : ITemplateQueryService, ITemplateCommandService
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
