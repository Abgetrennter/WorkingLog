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
    public class ImportWizardForm : Form
    {
        private readonly ListView _previewList;
        private readonly Button _btnChoose;
        private readonly Button _btnImport;
        private readonly Label _lblFile;
        private string _sourcePath;
        private List<WorkLogItem> _imported;

        public ImportWizardForm()
        {
            Text = "导入向导（基础版）";
            Width = 800;
            Height = 600;

            var top = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            _lblFile = new Label { Text = "未选择文件", AutoSize = true, Location = new Point(8, 16) };
            _btnChoose = new Button { Text = "选择文件", Width = 100, Height = 30, Location = new Point(620, 10) };
            _btnChoose.Click += OnChooseFile;
            top.Controls.Add(_lblFile);
            top.Controls.Add(_btnChoose);

            _previewList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            _previewList.Columns.Add("日期", 120);
            _previewList.Columns.Add("标题", 260);
            _previewList.Columns.Add("状态", 120);
            _previewList.Columns.Add("标签", 200);

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
            _btnImport = new Button { Text = "开始导入", Width = 120, Height = 34, Enabled = false };
            _btnImport.Click += OnImport;
            bottom.Controls.Add(_btnImport);

            Controls.Add(_previewList);
            Controls.Add(bottom);
            Controls.Add(top);

            // 应用统一样式（字体、缩放、抗锯齿）
            UIStyleManager.ApplyVisualEnhancements(this, 1.25f);
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
                var items = svc.ImportFromFile(_sourcePath) ?? Enumerable.Empty<WorkLogItem>();
                _imported = items.ToList();
                _previewList.BeginUpdate();
                _previewList.Items.Clear();
                foreach (var it in _imported.Take(10))
                {
                    var lv = new ListViewItem(new[]
                    {
                        it.LogDate.ToString("yyyy-MM-dd"),
                        it.ItemTitle ?? string.Empty,
                        it.Status.ToString(),
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

                // 按月份分组写入
                var groups = _imported.GroupBy(it => new { it.LogDate.Year, it.LogDate.Month });
                int total = 0;
                foreach (var g in groups)
                {
                    var monthDate = new DateTime(g.Key.Year, g.Key.Month, 1);
                    var list = g.ToList();
                    total += list.Count;
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
    }
}