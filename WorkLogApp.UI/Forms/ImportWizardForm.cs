using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Services.Implementations;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class ImportWizardForm : Form
    {
        private string _sourcePath;
        private List<WorkLog> _imported;

        public ImportWizardForm()
        {
            InitializeComponent();
            // 应用统一样式（字体、缩放、抗锯齿）
            UIStyleManager.ApplyVisualEnhancements(this);

            // 设计期：填充示例文件名与预览项，便于在设计器中查看列表布局
            if (UIStyleManager.IsDesignMode)
            {
                try
                {
                    _lblFile.Text = "示例.xlsx";
                    _previewList.BeginUpdate();
                    _previewList.Items.Clear();
                    _previewList.Items.Add(new ListViewItem(new[] { DateTime.Today.ToString("yyyy-MM-dd"), "示例：周会记录", "团队;沟通" }));
                    _previewList.Items.Add(new ListViewItem(new[] { DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), "示例：Bug 修复", "缺陷;修复" }));
                    _previewList.EndUpdate();
                    _btnImport.Enabled = false;
                }
                catch { }
                return;
            }
        }

        private void OnChooseFile(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*";
                dlg.Title = "选择要导入的工作日志 Excel";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _sourcePath = dlg.FileName;
                _lblFile.Text = _sourcePath;
                PreviewFile();
            }
        }

        private void PreviewFile()
        {
            try
            {
                IImportExportService svc = new ImportExportService();
                var days = svc.ImportFromFile(_sourcePath) ?? Enumerable.Empty<WorkLog>();
                _imported = days.ToList();
                _previewList.BeginUpdate();
                _previewList.Items.Clear();
                foreach (var it in _imported.SelectMany(d => d.Items ?? new List<WorkLogItem>()).Take(10))
                {
                    var lv = new ListViewItem(new[]
                    {
                        it.LogDate.ToString("yyyy-MM-dd"),
                        it.ItemTitle ?? string.Empty,
                        it.Tags ?? string.Empty
                    });
                    _previewList.Items.Add(lv);
                }
                _previewList.EndUpdate();
                _btnImport.Enabled = _imported.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "预览失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnImport.Enabled = false;
            }
        }

        private void OnImport(object sender, EventArgs e)
        {
            if (_imported == null || _imported.Count == 0)
            {
                MessageBox.Show(this, "没有可导入的数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);
                IImportExportService svc = new ImportExportService();

                // 按月份分组写入（按天聚合）
                var groups = _imported.GroupBy(d => new { d.LogDate.Year, d.LogDate.Month });
                int total = 0;
                foreach (var g in groups)
                {
                    var monthDate = new DateTime(g.Key.Year, g.Key.Month, 1);
                    var list = g.ToList();
                    total += list.SelectMany(d => d.Items ?? new List<WorkLogItem>()).Count();
                    svc.ExportMonth(monthDate, list, dataDir);
                }
                MessageBox.Show(this, $"导入完成，共导入 {total} 条记录。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "导入失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void topPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}