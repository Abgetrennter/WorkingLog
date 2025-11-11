using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Implementations;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Forms
{
    public class ItemEditForm : Form
    {
        private readonly WorkLogItem _item;
        private readonly TextBox _titleBox;
        private readonly TextBox _contentBox;
        private readonly Button _btnSave;
        private readonly Button _btnCancel;

        public ItemEditForm(WorkLogItem item, string initialContent)
        {
            _item = item ?? new WorkLogItem { LogDate = DateTime.Now.Date };
            Text = "编辑日志事项";
            Width = 900;
            Height = 650;

            var lblTitle = new Label { Text = "标题：", Left = 10, Top = 15, AutoSize = true };
            _titleBox = new TextBox { Left = 60, Top = 10, Width = 800 };
            _titleBox.Text = _item.ItemTitle ?? string.Empty;

            var lblContent = new Label { Text = "内容：", Left = 10, Top = 45, AutoSize = true };
            _contentBox = new TextBox { Left = 10, Top = 70, Width = 850, Height = 480, Multiline = true, ScrollBars = ScrollBars.Both };
            _contentBox.Text = initialContent ?? _item.ItemContent ?? string.Empty;

            _btnSave = new Button { Text = "保存", Left = 10, Top = 565, Width = 100, Height = 35 };
            _btnCancel = new Button { Text = "取消", Left = 120, Top = 565, Width = 100, Height = 35 };
            _btnSave.Click += OnSaveClick;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblTitle);
            Controls.Add(_titleBox);
            Controls.Add(lblContent);
            Controls.Add(_contentBox);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                _item.ItemTitle = _titleBox.Text?.Trim();
                _item.ItemContent = _contentBox.Text ?? string.Empty;

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

                // CSV 导出（按月文件）
                IImportExportService exportService = new ImportExportService();
                exportService.ExportMonth(_item.LogDate, new[] { _item }, dataDir);

                // 可选：文本备份，便于直接查看
                var safeTitle = string.IsNullOrWhiteSpace(_item.ItemTitle) ? "untitled" : SanitizeFileName(_item.ItemTitle);
                var fileName = $"{_item.LogDate:yyyy-MM-dd}_{safeTitle}.txt";
                var filePath = Path.Combine(dataDir, fileName);
                File.WriteAllText(filePath, _item.ItemContent);

                MessageBox.Show(this, $"已保存到 CSV 与文本备份:\n{Path.Combine(dataDir, "worklog_" + _item.LogDate.ToString("yyyyMM") + ".csv")}\n{filePath}", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}