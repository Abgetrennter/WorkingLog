using System;
using System.IO;

namespace WorkLogApp.Core.Constants
{
    /// <summary>
    /// 应用程序配置实现类
    /// 提供统一的配置访问方式，避免直接使用 ConfigurationManager
    /// 注意：Core 层不直接依赖 System.Configuration，配置值通过构造函数传入
    /// </summary>
    public class AppConfiguration : IAppConfiguration
    {
        private readonly string _baseDirectory;
        private readonly string _environment;
        private readonly string _dataPath;
        private readonly string _templatesPath;

        /// <summary>
        /// 初始化 AppConfiguration 类的新实例
        /// </summary>
        /// <param name="baseDirectory">应用程序基础目录</param>
        /// <param name="environment">环境名称（dev/prod）</param>
        /// <param name="dataPath">数据路径</param>
        /// <param name="templatesPath">模板路径</param>
        public AppConfiguration(string baseDirectory, string environment, string dataPath, string templatesPath)
        {
            _baseDirectory = baseDirectory;
            _environment = environment ?? AppConstants.DefaultEnvironment;
            _dataPath = dataPath ?? AppConstants.DataDirectoryName;
            _templatesPath = templatesPath ?? AppConstants.TemplatesDirectoryName;
        }

        /// <summary>
        /// 初始化 AppConfiguration 类的新实例（使用默认配置）
        /// </summary>
        /// <param name="baseDirectory">应用程序基础目录</param>
        public AppConfiguration(string baseDirectory) : this(baseDirectory, AppConstants.DefaultEnvironment, AppConstants.DataDirectoryName, AppConstants.TemplatesDirectoryName)
        {
        }

        /// <summary>
        /// 获取当前环境名称（dev/prod）
        /// </summary>
        public string Environment => _environment;

        /// <summary>
        /// 获取数据路径配置
        /// </summary>
        public string DataPath => _dataPath;

        /// <summary>
        /// 获取模板路径配置
        /// </summary>
        public string TemplatesPath => _templatesPath;

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <returns>配置文件的完整路径</returns>
        public string GetConfigFilePath()
        {
            return Path.Combine(_baseDirectory, AppConstants.ConfigsDirectoryName, $"{_environment}.config.json");
        }

        /// <summary>
        /// 获取数据目录的完整路径
        /// </summary>
        /// <returns>数据目录的完整路径</returns>
        public string GetDataDirectory()
        {
            return Path.Combine(_baseDirectory, _dataPath);
        }

        /// <summary>
        /// 获取模板目录的完整路径
        /// </summary>
        /// <returns>模板目录的完整路径</returns>
        public string GetTemplatesDirectory()
        {
            return Path.Combine(_baseDirectory, _templatesPath);
        }
    }
}
