using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Implementations;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class ItemEditForm : Form
    {
        private readonly WorkLogItem _item;
        
        // 设计期支持：提供无参构造，便于设计器实例化
        public ItemEditForm()
        {
            _item = new WorkLogItem { LogDate = DateTime.Now.Date };
            InitializeComponent();
            UIStyleManager.ApplyVisualEnhancements(this);
        }

        
        public ItemEditForm(WorkLogItem item, string initialContent)
        {
            _item = item ?? new WorkLogItem { LogDate = DateTime.Now.Date };
            InitializeComponent();

            _titleBox.Text = _item.ItemTitle ?? string.Empty;
            _summaryBox.Text = _item.DailySummary ?? string.Empty;
            _contentBox.Text = initialContent ?? _item.ItemContent ?? string.Empty;

            // 应用统一样式并设置 1.5 倍行距
            UIStyleManager.ApplyVisualEnhancements(this);
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                _item.ItemTitle = _titleBox.Text?.Trim();
                _item.ItemContent = _contentBox.Text ?? string.Empty;
                _item.DailySummary = _summaryBox.Text ?? string.Empty;

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

                // Excel 导出（按月文件、按日 Sheet）
                IImportExportService exportService = new ImportExportService();
                exportService.ExportMonth(_item.LogDate, new[] { _item }, dataDir);

                // 可选：文本备份，便于直接查看
                var safeTitle = string.IsNullOrWhiteSpace(_item.ItemTitle) ? "untitled" : SanitizeFileName(_item.ItemTitle);
                var fileName = $"{_item.LogDate:yyyy-MM-dd}_{safeTitle}.txt";
                var filePath = Path.Combine(dataDir, fileName);
                File.WriteAllText(filePath, _item.ItemContent);

                MessageBox.Show(this, $"已保存到 Excel 与文本备份:\n{Path.Combine(dataDir, "worklog_" + _item.LogDate.ToString("yyyyMM") + ".xlsx")}\n{filePath}", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SanitizeFileName(string name)
        {
            // 移除 Windows 非法文件名字符
            var invalid = new string(Path.GetInvalidFileNameChars());
            var pattern = "[" + Regex.Escape(invalid) + "]";
            return Regex.Replace(name, pattern, "_");
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}