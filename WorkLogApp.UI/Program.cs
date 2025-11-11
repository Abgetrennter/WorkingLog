using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using WorkLogApp.Services.Implementations;

namespace WorkLogApp.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var env = ConfigurationManager.AppSettings["ConfigEnvironment"] ?? "dev";
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(baseDir, "Configs", $"{env}.config.json");
            var templatesPath = Path.Combine(baseDir, "Templates", "templates.json");

            var templateService = new TemplateService();
            // 仅做加载存在性检查，后续完善为真实 JSON 解析
            templateService.LoadTemplates(templatesPath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.MainForm(templateService));
        }
    }
}