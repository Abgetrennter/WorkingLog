using System;
using WorkLogApp.Core.Helpers;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Services.Implementations;
using SimpleInjector;

namespace WorkLogApp.UI.Helpers
{
    /// <summary>
    /// 服务工厂类
    /// 用于统一创建和获取服务实例，消除重复的服务实例化代码
    /// </summary>
    public static class ServiceFactory
    {
        /// <summary>
        /// 获取导入导出服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static IImportExportService GetImportExportService()
        {
            return GetImportExportService(Program.Container);
        }

        /// <summary>
        /// 获取导入导出服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static IImportExportService GetImportExportService(Container container)
        {
            if (container != null)
            {
                try
                {
                    return container.GetInstance<IImportExportService>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"从容器获取 IImportExportService 失败: {ex.Message}");
                }
            }

            // 容器不可用时，创建默认实例
            Logger.Debug("创建默认 IImportExportService 实例");
            var pdfService = new PdfExportService();
            var wordService = new WordExportService();
            return new ImportExportService(pdfService, wordService);
        }

        /// <summary>
        /// 获取 PDF 导出服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static IPdfExportService GetPdfExportService()
        {
            return GetPdfExportService(Program.Container);
        }

        /// <summary>
        /// 获取 PDF 导出服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static IPdfExportService GetPdfExportService(Container container)
        {
            if (container != null)
            {
                try
                {
                    return container.GetInstance<IPdfExportService>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"从容器获取 IPdfExportService 失败: {ex.Message}");
                }
            }

            // 容器不可用时，创建默认实例
            Logger.Debug("创建默认 IPdfExportService 实例");
            return new PdfExportService();
        }

        /// <summary>
        /// 获取 Word 导出服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static IWordExportService GetWordExportService()
        {
            return GetWordExportService(Program.Container);
        }

        /// <summary>
        /// 获取 Word 导出服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static IWordExportService GetWordExportService(Container container)
        {
            if (container != null)
            {
                try
                {
                    return container.GetInstance<IWordExportService>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"从容器获取 IWordExportService 失败: {ex.Message}");
                }
            }

            // 容器不可用时，创建默认实例
            Logger.Debug("创建默认 IWordExportService 实例");
            return new WordExportService();
        }

        /// <summary>
        /// 获取模板服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static ITemplateService GetTemplateService()
        {
            return GetTemplateService(Program.Container);
        }

        /// <summary>
        /// 获取模板服务实例
        /// 优先从 DI 容器获取，容器不可用时创建新实例
        /// </summary>
        public static ITemplateService GetTemplateService(Container container)
        {
            if (container != null)
            {
                try
                {
                    return container.GetInstance<ITemplateService>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"从容器获取 ITemplateService 失败: {ex.Message}");
                }
            }

            // 容器不可用时，创建默认实例
            Logger.Debug("创建默认 ITemplateService 实例");
            return new TemplateService();
        }
    }
}
