using System;
using System.IO;
using System.Reflection;
using WorkLogApp.Core.Constants;

namespace WorkLogApp.UI.Helpers
{
    /// <summary>
    /// 管理嵌入式资源的提取和目录创建
    /// </summary>
    public static class ResourceManager
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        private static readonly string BaseNamespace = "WorkLogApp.UI";

        /// <summary>
        /// 确保目录存在，若不存在则创建
        /// </summary>
        public static void EnsureDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 将嵌入式资源提取到指定文件（仅当目标文件不存在时）
        /// </summary>
        /// <param name="resourceRelativePath">资源在程序集中的相对路径（例如 "Configs.dev.config.json"）</param>
        /// <param name="outputFilePath">输出文件的完整路径</param>
        /// <returns>true 表示文件已存在或成功提取；false 表示提取失败</returns>
        public static bool ExtractResourceIfNotExists(string resourceRelativePath, string outputFilePath)
        {
            if (File.Exists(outputFilePath))
            {
                return true;
            }

            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputFilePath);
            EnsureDirectory(outputDir);

            // 构建完整的资源名称
            string resourceName = $"{BaseNamespace}.{resourceRelativePath.Replace('\\', '.')}";
            Stream stream = CurrentAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                // 尝试另一种命名方式（去掉重复的基命名空间）
                resourceName = $"{BaseNamespace}.{resourceRelativePath}";
                stream = CurrentAssembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // 记录错误，但不要抛出异常，以免影响启动
                    // 可以在日志中记录，但暂时忽略
                    return false;
                }
            }

            using (stream)
            using (var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }

            return true;
        }

        /// <summary>
        /// 提取配置文件（Configs/*.json）到基础目录下的 Configs 文件夹
        /// </summary>
        public static void ExtractConfigs(string baseDir)
        {
            var configsDir = Path.Combine(baseDir, AppConstants.ConfigsDirectoryName);
            EnsureDirectory(configsDir);

            ExtractResourceIfNotExists(
                $"{AppConstants.ConfigResourceNamespace}{AppConstants.DevConfigFileName}",
                Path.Combine(configsDir, AppConstants.DevConfigFileName));
            ExtractResourceIfNotExists(
                $"{AppConstants.ConfigResourceNamespace}{AppConstants.ProdConfigFileName}",
                Path.Combine(configsDir, AppConstants.ProdConfigFileName));
        }

        /// <summary>
        /// 提取模板文件到基础目录下的 Templates 文件夹
        /// </summary>
        public static void ExtractTemplates(string baseDir)
        {
            var templatesDir = Path.Combine(baseDir, AppConstants.TemplatesDirectoryName);
            EnsureDirectory(templatesDir);

            ExtractResourceIfNotExists(
                $"{AppConstants.TemplateResourceNamespace}{AppConstants.TemplatesFileName}",
                Path.Combine(templatesDir, AppConstants.TemplatesFileName));
        }

        /// <summary>
        /// 提取 App.config 作为 exe.config 文件（WorkLogApp.UI.exe.config）
        /// </summary>
        public static void ExtractAppConfig(string baseDir, string exeName = "WorkLogApp.UI.exe")
        {
            var configFileName = exeName + ".config";
            var configFilePath = Path.Combine(baseDir, configFileName);

            // 如果已有 exe.config，不再覆盖（用户可能已自定义）
            if (File.Exists(configFilePath))
            {
                return;
            }

            ExtractResourceIfNotExists("App.config", configFilePath);
        }

        /// <summary>
        /// 确保数据目录存在
        /// </summary>
        public static void EnsureDataDirectory(string baseDir, string dataDirName = null)
        {
            var dirName = dataDirName ?? AppConstants.DataDirectoryName;
            var dataDir = Path.Combine(baseDir, dirName);
            EnsureDirectory(dataDir);
        }
    }
}