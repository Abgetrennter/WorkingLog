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
        private System.Collections.Generic.List<WorkLogItem> _currentItems = new System.Collections.Generic.List<WorkLogItem>();

        // 设计期支持：提供无参构造，方便设计器实例化
        public MainForm() : this(new TemplateService())
        {
        }

        public MainForm(ITemplateService templateService)
        {
            _templateService = templateService;
            InitializeComponent();

            // 运行时：为列表添加双击编辑事件
            if (!UIStyleManager.IsDesignMode)
            {
                _listView.DoubleClick += OnListViewDoubleClick;
            }

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
                _currentItems = items.ToList();
                BindItems(_currentItems);
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
                lv.Tag = it;
                _listView.Items.Add(lv);
            }
            _listView.EndUpdate();
        }

        private void _monthPicker_ValueChanged(object sender, EventArgs e)
        {

        }

        private void OnListViewDoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (_listView.SelectedItems.Count == 0) return;
                var lv = _listView.SelectedItems[0];
                var item = lv.Tag as WorkLogItem;
                if (item == null)
                {
                    MessageBox.Show(this, "无法定位原始数据项。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var form = new ItemEditForm(item, null))
                {
                    form.StartPosition = FormStartPosition.CenterParent;
                    var result = form.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        // 覆盖导出当前月份数据并刷新列表
                        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        var dataDir = Path.Combine(baseDir, "Data");
                        Directory.CreateDirectory(dataDir);
                        var month = _monthPicker.Value;
                        IImportExportService svc = new ImportExportService();
                        svc.RewriteMonth(month, _currentItems, dataDir);
                        RefreshMonthItems();
                        MessageBox.Show(this, "已保存修改并重新生成当月 Excel。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "打开编辑窗口失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}