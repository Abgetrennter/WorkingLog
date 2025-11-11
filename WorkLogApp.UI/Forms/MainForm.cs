using System;
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
    public partial class MainForm : Form
    {
        private readonly ITemplateService _templateService;

        // 设计期支持：提供无参构造，方便设计器实例化
        public MainForm() : this(new TemplateService())
        {
        }

        public MainForm(ITemplateService templateService)
        {
            _templateService = templateService;
            InitializeComponent();

            // 设计期：填充 ListView 示例数据，便于在设计器中预览布局
            if (UIStyleManager.IsDesignMode)
            {
                try
                {
                    _listView.BeginUpdate();
                    _listView.Items.Clear();
                    _listView.Items.Add(new ListViewItem(new[]
                    {
                        DateTime.Today.ToString("yyyy-MM-dd"),
                        "示例：需求评审会议",
                        "讨论近期版本目标与测试范围……",
                        "InProgress",
                        "会议;需求",
                        DateTime.Today.AddHours(9).ToString("yyyy-MM-dd HH:mm"),
                        DateTime.Today.AddHours(10).ToString("yyyy-MM-dd HH:mm")
                    }));
                    _listView.Items.Add(new ListViewItem(new[]
                    {
                        DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                        "示例：接口联调",
                        "修复返回格式，补充缺失字段",
                        "Done",
                        "研发;联调",
                        DateTime.Today.AddDays(-1).AddHours(14).ToString("yyyy-MM-dd HH:mm"),
                        DateTime.Today.AddDays(-1).AddHours(16).ToString("yyyy-MM-dd HH:mm")
                    }));
                    _listView.EndUpdate();
                }
                catch { }
                return;
            }
        }

        private void OnCreateItemClick(object sender, EventArgs e)
        {
            using (var form = new ItemCreateForm(_templateService))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
                // 保存后刷新当前月份数据
                RefreshMonthItems();
            }
        }

        private void OnImportMonthClick(object sender, EventArgs e)
        {
            RefreshMonthItems();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 打开应用时自动读取当前月份数据
            RefreshMonthItems();
        }

        private void OnMergeOtherClick(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*";
                    dlg.Title = "选择要合并的工作日志 Excel";
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    var sourcePath = dlg.FileName;
                    IImportExportService svc = new ImportExportService();
                    var imported = svc.ImportFromFile(sourcePath) ?? Enumerable.Empty<WorkLogItem>();

                    // 仅合并当前月份的数据
                    var month = _monthPicker.Value;
                    var monthItems = imported.Where(it => it.LogDate.Year == month.Year && it.LogDate.Month == month.Month).ToList();
                    if (monthItems.Count == 0)
                    {
                        MessageBox.Show(this, "所选文件中无当前月份的数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var dataDir = Path.Combine(baseDir, "Data");
                    Directory.CreateDirectory(dataDir);

                    // 追加写入到本地月份文件
                    svc.ExportMonth(month, monthItems, dataDir);
                    RefreshMonthItems();
                    MessageBox.Show(this, "已合并并刷新当前月份列表。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "合并失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCategoryManageClick(object sender, EventArgs e)
        {
            using (var form = new CategoryManageForm(_templateService))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
            }
        }

        private void OnImportWizardClick(object sender, EventArgs e)
        {
            using (var form = new ImportWizardForm())
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
                RefreshMonthItems();
            }
        }

        private void RefreshMonthItems()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);

                var month = _monthPicker.Value;
                IImportExportService svc = new ImportExportService();
                var items = svc.ImportMonth(month, dataDir) ?? Enumerable.Empty<WorkLogItem>();
                BindItems(items);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "导入失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindItems(System.Collections.Generic.IEnumerable<WorkLogItem> items)
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();
            foreach (var it in items)
            {
                var content = (it.ItemContent ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
                if (content.Length > 200) content = content.Substring(0, 200) + "...";
                var lv = new ListViewItem(new[]
                {
                    it.LogDate.ToString("yyyy-MM-dd"),
                    it.ItemTitle ?? string.Empty,
                    content,
                    it.Status.ToString(),
                    it.Tags ?? string.Empty,
                    it.StartTime.HasValue ? it.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                    it.EndTime.HasValue ? it.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty
                });
                _listView.Items.Add(lv);
            }
            _listView.EndUpdate();
        }

        private void _monthPicker_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}