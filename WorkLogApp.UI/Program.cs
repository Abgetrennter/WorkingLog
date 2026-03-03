using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using SimpleInjector;
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

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 初始化全局样式（字体、缩放、渲染）
            UIStyleManager.Initialize();

            // 全局异常捕获，避免启动时静默失败
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var logDir = Path.Combine(baseDir, "Logs");
                    Directory.CreateDirectory(logDir);
                    File.WriteAllText(Path.Combine(logDir, "thread_exception.log"), e.Exception?.ToString());
                }
                catch { }
                MessageBox.Show($"UI线程异常:\n{e.Exception}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var logDir = Path.Combine(baseDir, "Logs");
                    Directory.CreateDirectory(logDir);
                    File.WriteAllText(Path.Combine(logDir, "unhandled_exception.log"), ex?.ToString());
                }
                catch { }
                MessageBox.Show($"未处理异常:\n{ex}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var configPath = Path.Combine(baseDir, "Configs", $"{env}.config.json");
                
                var relativeTplPath = ConfigurationManager.AppSettings["TemplatesPath"] ?? "Templates\\templates.json";
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
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var logDir = Path.Combine(baseDir, "Logs");
                    Directory.CreateDirectory(logDir);
                    File.WriteAllText(Path.Combine(logDir, "startup_error.log"), ex.ToString());
                }
                catch { }
                MessageBox.Show($"启动失败:\n{ex}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                container.GetInstance<IImportExportService>()));

            // 注册其他可能需要注入的窗体
            container.Register<ItemCreateForm>(() => new ItemCreateForm(
                container.GetInstance<ITemplateService>()));
            container.Register<CategoryManageForm>(() => new CategoryManageForm(
                container.GetInstance<ITemplateService>()));
            // 注意：其他窗体可能也需要注册，但暂时先处理这两个

            // 获取 TemplateService 实例并加载模板
            var templateService = container.GetInstance<ITemplateService>();
            templateService.LoadTemplates(templatesPath);
        }
    }
}