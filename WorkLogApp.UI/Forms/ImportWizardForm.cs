using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Constants;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class ImportWizardForm : Form
    {
        private readonly IImportExportService _importExportService;
        private string _sourcePath;
        private List<WorkLog> _imported;

        // 设计期支持：提供无参构造，便于设计器实例化
        public ImportWizardForm()
        {
            // 设计时：使用空服务实例
            if (UIStyleManager.IsDesignMode)
            {
                _importExportService = null;
            }
            else
            {
                // 运行时：通过 DI 容器获取
                throw new InvalidOperationException("请使用带参数的构造函数进行依赖注入");
            }

            InitializeComponent();
            IconHelper.ApplyIcon(this);
            // 应用统一样式（字体、缩放、抗锯齿）
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);

            InitToolTips();

            // 设计期：填充示例文件名与预览项，便于在设计器中查看列表布局
            if (UIStyleManager.IsDesignMode)
            {
                try
                {
                    _lblFile.Text = "示例.xlsx";
                    _previewList.BeginUpdate();
                    _previewList.Items.Clear();
                    _previewList.Items.Add(new ListViewItem(new[] { DateTime.Today.ToString("yyyy-MM-dd"), "示例：周会记录", "团队;沟通", "Done", "10:00-11:00" }));
                    _previewList.Items.Add(new ListViewItem(new[] { DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), "示例：Bug 修复", "缺陷;修复", "Doing", "14:00-" }));
                    _previewList.EndUpdate();
                    _btnImport.Enabled = false;
                }
                catch { }
            }
        }

        public ImportWizardForm(IImportExportService importExportService)
        {
            _importExportService = importExportService;
            InitializeComponent();
            IconHelper.ApplyIcon(this);
            // 应用统一样式（字体、缩放、抗锯齿）
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
            InitToolTips();
        }

        private void InitToolTips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_btnChoose, "选择要导入的Excel文件");
            toolTip.SetToolTip(_btnImport, "执行导入操作");
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

        /// <summary>
        /// 预览文件内容
        /// </summary>
        private void PreviewFile()
        {
            try
            {
                IImportExportService service = _importExportService;
                var days = service.ImportFromFile(_sourcePath) ?? Enumerable.Empty<WorkLog>();
                _imported = days.ToList();
                _previewList.BeginUpdate();
                _previewList.Items.Clear();
                foreach (var it in _imported.SelectMany(d => d.Items ?? new List<WorkLogItem>()).Take(10))
                {
                    var lv = new ListViewItem(new[]
                    {
                        it.LogDate.ToString("yyyy-MM-dd"),
                        it.ItemTitle ?? string.Empty,
                        it.Tags ?? string.Empty,
                        it.Status.ToString(),
                        $"{(it.StartTime.HasValue ? it.StartTime.Value.ToString("HH:mm") : "")}-{(it.EndTime.HasValue ? it.EndTime.Value.ToString("HH:mm") : "")}"
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

        /// <summary>
        /// 执行导入操作
        /// </summary>
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
                var dataDir = Path.Combine(baseDir, AppConstants.DataDirectoryName);
                Directory.CreateDirectory(dataDir);
                IImportExportService service = _importExportService;

                // 按月份分组写入（按天聚合）
                var groups = _imported.GroupBy(d => new { d.LogDate.Year, d.LogDate.Month });
                int total = 0;
                foreach (var g in groups)
                {
                    var monthDate = new DateTime(g.Key.Year, g.Key.Month, 1);
                    var list = g.ToList();
                    total += list.SelectMany(d => d.Items ?? new List<WorkLogItem>()).Count();
                    service.ExportMonth(monthDate, list, dataDir);
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