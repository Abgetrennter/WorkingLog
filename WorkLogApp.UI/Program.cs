using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SimpleInjector;
using WorkLogApp.Core.Helpers;
using WorkLogApp.Core.Constants;
using WorkLogApp.Services.Implementations;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.UI;
using WorkLogApp.UI.Helpers;
using WorkLogApp.UI.Forms;

namespace WorkLogApp.UI
{
    static class Program
    {
        private static Container _container;

        /// <summary>
        /// 获取 DI 容器实例（供设计时支持使用）
        /// </summary>
        public static Container Container => _container;

        // P/Invoke声明：设置DPI感知
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            // 初始化日志系统
            try
            {
                Logger.Initialize();
                Logger.Info("应用程序启动");
            }
            catch (Exception ex)
            {
                // 如果日志初始化失败，至少输出到调试器
                System.Diagnostics.Debug.WriteLine($"日志初始化失败: {ex.Message}");
            }

            // 设置高DPI感知模式（支持PerMonitorV2）
            if (Environment.OSVersion.Version.Major >= 6) // Windows Vista及以上
            {
                SetProcessDPIAware();
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 初始化全局样式（字体、缩放、渲染）
            UIStyleManager.Initialize();
 
            // 全局异常捕获，避免启动时静默失败
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                var logMsg = $"UI线程异常: {e.Exception?.Message}";
                Logger.Error(logMsg, e.Exception);
                MessageBox.Show(
                    $"操作过程中发生错误:\n{e.Exception?.Message}\n\n详细信息已记录到日志文件。",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                var logMsg = $"未处理异常: {ex?.Message}";
                Logger.Error(logMsg, ex);
                MessageBox.Show(
                    $"应用程序遇到严重错误:\n{ex?.Message}\n\n详细信息已记录到日志文件。",
                    "严重错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };
 
            // 注册应用程序退出时的资源清理
            Application.ApplicationExit += (s, e) =>
            {
                Logger.Info("应用程序退出，清理资源");
                UIStyleManager.Dispose();
                Logger.Dispose();
            };

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // 释放嵌入式资源
                ResourceManager.ExtractAppConfig(baseDir);
                ResourceManager.ExtractConfigs(baseDir);
                ResourceManager.ExtractTemplates(baseDir);
                ResourceManager.EnsureDataDirectory(baseDir);
 
                var env = ConfigurationManager.AppSettings["ConfigEnvironment"] ?? "dev";
                var configPath = Path.Combine(baseDir, AppConstants.ConfigsDirectoryName, $"{env}.config.json");
                
                var relativeTplPath = ConfigurationManager.AppSettings[AppConstants.TemplatesPathConfigKey]
                    ?? Path.Combine(AppConstants.TemplatesDirectoryName, AppConstants.TemplatesFileName);
                var templatesPath = Path.Combine(baseDir, relativeTplPath);
 
                // 设置依赖注入容器
                _container = new Container();
                ConfigureServices(_container, templatesPath);
                // 禁用验证以避免可释放瞬态组件警告
                // _container.Verify();
 
                // 解析主窗体
                var main = _container.GetInstance<MainForm>();
                UIStyleManager.ApplyVisualEnhancements(main);
                Application.Run(main);
            }
            catch (Exception ex)
            {
                Logger.Error("应用程序启动失败", ex);
                MessageBox.Show(
                    $"应用程序启动失败:\n{ex.Message}\n\n详细信息已记录到日志文件。",
                    "启动错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void ConfigureServices(Container container, string templatesPath)
        {
            // 禁用自动验证，避免可释放瞬态组件警告
            container.Options.EnableAutoVerification = false;

            // 注册服务为单例（共享实例）
            container.Register<ITemplateService, TemplateService>(Lifestyle.Singleton);
            container.Register<IPdfExportService, PdfExportService>(Lifestyle.Singleton);
            container.Register<IWordExportService, WordExportService>(Lifestyle.Singleton);
            container.Register<IImportExportService, ImportExportService>(Lifestyle.Singleton);

            // 注册 MainForm 为每次解析时新建（瞬态），指定使用带依赖的构造函数
            container.Register<MainForm>(() => new MainForm(
                container.GetInstance<ITemplateService>(),
                container.GetInstance<IImportExportService>(),
                container.GetInstance<IPdfExportService>(),
                container.GetInstance<IWordExportService>()));

            // 注册其他窗体为瞬态，使用带依赖的构造函数
            container.Register<ItemCreateForm>(() => new ItemCreateForm(
                container.GetInstance<ITemplateService>()));
            container.Register<ItemEditForm>(() => new ItemEditForm());
            container.Register<CategoryManageForm>(() => new CategoryManageForm(
                container.GetInstance<ITemplateService>()));
            container.Register<ImportWizardForm>(() => new ImportWizardForm(
                container.GetInstance<IImportExportService>()));
            container.Register<TodoForm>(() => new TodoForm());
            // Note: DailySummaryForm has constructor parameters and is created directly in MainForm

            // 获取 TemplateService 实例并加载模板
            var templateService = container.GetInstance<ITemplateService>();
            templateService.LoadTemplates(templatesPath);
        }
    }
}