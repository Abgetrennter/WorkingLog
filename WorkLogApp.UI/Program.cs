using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using WorkLogApp.Services.Implementations;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI
{
    static class Program
    {
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
                var env = ConfigurationManager.AppSettings["ConfigEnvironment"] ?? "dev";
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var configPath = Path.Combine(baseDir, "Configs", $"{env}.config.json");
                
                var relativeTplPath = ConfigurationManager.AppSettings["TemplatesPath"] ?? "Templates\\templates.json";
                var templatesPath = Path.Combine(baseDir, relativeTplPath);

                var templateService = new TemplateService();
                templateService.LoadTemplates(templatesPath);

                var importExportService = new ImportExportService();
                var main = new Forms.MainForm(templateService, importExportService);
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
    }
}