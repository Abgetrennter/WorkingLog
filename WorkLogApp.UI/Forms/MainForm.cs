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
        private System.Collections.Generic.List<WorkLog> _allMonthItems = new System.Collections.Generic.List<WorkLog>();

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
                _listView.AllowDrop = true;
                _listView.ItemDrag += OnListViewItemDrag;
                _listView.DragEnter += OnListViewDragEnter;
                _listView.DragOver += OnListViewDragOver;
                _listView.DragDrop += OnListViewDragDrop;

                _monthPicker.Format = DateTimePickerFormat.Custom;
                _monthPicker.CustomFormat = "yyyy-MM";
                _monthPicker.ShowUpDown = true;

                _dayPicker.Value = DateTime.Today;
                _chkShowByMonth.CheckedChanged += OnScopeToggled;
                _dayPicker.ValueChanged += OnDayPickerValueChanged;
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
            RefreshItems();
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                // 根据当前视图更新 SortOrder
                UpdateSortOrderByCurrentView();

                // 保存到当月 Excel
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);

                var selectedDate = _dayPicker.Value;
                var monthRef = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                if (_chkShowByMonth.Checked)
                {
                    monthRef = new DateTime(_monthPicker.Value.Year, _monthPicker.Value.Month, 1);
                }

                IImportExportService svc = new ImportExportService();
                svc.RewriteMonth(monthRef, _allMonthItems, dataDir);

                // 保存后重新绑定，以反映可能的排序变化
                RefreshItems();
                MessageBox.Show(this, "已保存排序并更新当月 Excel。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (UIStyleManager.IsDesignMode) return;
            _chkShowByMonth.Checked = false;
            _dayPicker.Value = DateTime.Today;
            _monthPicker.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            SetScopeUI();
            RefreshItems();
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
            RefreshItems();
        }

        private void RefreshItems()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);

                var selectedDate = _dayPicker.Value;
                var monthRef = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                if (_chkShowByMonth.Checked)
                {
                    monthRef = new DateTime(_monthPicker.Value.Year, _monthPicker.Value.Month, 1);
                }

                IImportExportService svc = new ImportExportService();
                var monthDays = svc.ImportMonth(monthRef, dataDir) ?? Enumerable.Empty<WorkLog>();
                _allMonthItems = monthDays.ToList();

                if (_chkShowByMonth.Checked)
                {
                    _currentItems = _allMonthItems.SelectMany(d => d.Items ?? new System.Collections.Generic.List<WorkLogItem>()).ToList();
                }
                else
                {
                    _currentItems = _allMonthItems.Where(d => d.LogDate.Date == selectedDate.Date)
                        .SelectMany(d => d.Items ?? new System.Collections.Generic.List<WorkLogItem>())
                        .ToList();
                }

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
            if (_chkShowByMonth.Checked)
            {
                RefreshItems();
            }
        }

        private void OnDayPickerValueChanged(object sender, EventArgs e)
        {
            if (!_chkShowByMonth.Checked)
            {
                RefreshItems();
            }
        }

        private void OnScopeToggled(object sender, EventArgs e)
        {
            SetScopeUI();
            RefreshItems();
        }

        private void SetScopeUI()
        {
            var byMonth = _chkShowByMonth.Checked;
            _monthPicker.Visible = byMonth;
            _dayPicker.Visible = !byMonth;
            _btnDailySummary.Visible = !byMonth;
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
                        svc.RewriteMonth(month, _allMonthItems, dataDir);
                        RefreshItems();
                        MessageBox.Show(this, "已保存修改并重新生成当月 Excel。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "打开编辑窗口失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDailySummaryClick(object sender, EventArgs e)
        {
            var selectedDate = _dayPicker.Value.Date;
            var day = _allMonthItems.FirstOrDefault(d => d.LogDate.Date == selectedDate);
            var existing = day?.DailySummary;

            var fallback = string.Join("；", _currentItems.Select(it => it.ItemTitle).Where(s => !string.IsNullOrWhiteSpace(s)));
            var initial = string.IsNullOrWhiteSpace(existing) ? fallback : existing;

            using (var form = new DailySummaryForm(day?.Items ?? new System.Collections.Generic.List<WorkLogItem>(), existing))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                var result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    var text = form.SummaryText ?? string.Empty;
                    ApplyDailySummary(selectedDate, text);
                    OnSaveClick(null, EventArgs.Empty);
                }
            }
        }

        private void ApplyDailySummary(DateTime date, string summary)
        {
            var day = _allMonthItems.FirstOrDefault(d => d.LogDate.Date == date.Date);
            if (day == null)
            {
                day = new WorkLog { LogDate = date.Date };
                _allMonthItems.Add(day);
            }
            day.DailySummary = summary ?? string.Empty;
        }

        private void OnListViewItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is ListViewItem it)
            {
                _listView.DoDragDrop(it, DragDropEffects.Move);
            }
        }

        private void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void OnListViewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void OnListViewDragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ListViewItem))) return;
            var dragged = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            var p = _listView.PointToClient(new Point(e.X, e.Y));
            var target = _listView.GetItemAt(p.X, p.Y);
            int fromIndex = dragged.Index;
            int toIndex = target != null ? target.Index : _listView.Items.Count - 1;
            if (toIndex == fromIndex) return;

            _listView.BeginUpdate();
            _listView.Items.RemoveAt(fromIndex);
            if (toIndex > fromIndex) toIndex--;
            _listView.Items.Insert(toIndex, dragged);
            _listView.EndUpdate();

            var ordered = _listView.Items
                .Cast<ListViewItem>()
                .Select(x => x.Tag as WorkLogItem)
                .Where(x => x != null)
                .ToList();
            _currentItems = ordered;
            //OnSaveClick(null, EventArgs.Empty);
        }

        /// <summary>
        /// 根据当前视图更新 SortOrder：
        /// - 日视图：仅更新该日的 SortOrder 为当前显示顺序（1..N）
        /// - 月视图：按当前列表顺序为每个日期分别编号（每个日期从 1..N）
        /// </summary>
        private void UpdateSortOrderByCurrentView()
        {
            if (_currentItems == null || _currentItems.Count == 0) return;

            if (!_chkShowByMonth.Checked)
            {
                // 日视图：当前列表只包含当天
                int order = 1;
                foreach (var it in _currentItems)
                {
                    it.SortOrder = order++;
                }
                return;
            }

            // 月视图：按照当前列表顺序为每个日期单独递增编号
            var counters = new System.Collections.Generic.Dictionary<DateTime, int>();
            foreach (var it in _currentItems)
            {
                var key = it.LogDate.Date;
                if (!counters.TryGetValue(key, out var cnt)) cnt = 0;
                cnt += 1;
                counters[key] = cnt;
                it.SortOrder = cnt;
            }
        }
    }
}