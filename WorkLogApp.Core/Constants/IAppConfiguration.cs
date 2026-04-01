using System;

namespace WorkLogApp.Core.Constants
{
    /// <summary>
    /// 应用程序配置接口
    /// 提供统一的配置访问方式，避免直接使用 ConfigurationManager
    /// </summary>
    public interface IAppConfiguration
    {
        /// <summary>
        /// 获取当前环境名称（dev/prod）
        /// </summary>
        string Environment { get; }

        /// <summary>
        /// 获取数据路径配置
        /// </summary>
        string DataPath { get; }

        /// <summary>
        /// 获取模板路径配置
        /// </summary>
        string TemplatesPath { get; }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <returns>配置文件的完整路径</returns>
        string GetConfigFilePath();

        /// <summary>
        /// 获取数据目录的完整路径
        /// </summary>
        /// <returns>数据目录的完整路径</returns>
        string GetDataDirectory();

        /// <summary>
        /// 获取模板目录的完整路径
        /// </summary>
        /// <returns>模板目录的完整路径</returns>
        string GetTemplatesDirectory();
    }
}
