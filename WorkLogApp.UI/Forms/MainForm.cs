using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Core.Enums;
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
            
            // Initialize templates immediately if needed
            // Assume paths are handled by service or Program.cs. 
            // Program.cs should ideally load templates.
            // Let's load here to be safe if not loaded.
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var tplPath = Path.Combine(baseDir, "Templates", "data.json");
            if (!_templateService.LoadTemplates(tplPath))
            {
                // Maybe fallback to old templates.json if data.json missing?
                // No, requirements said "rewrite", so we start fresh.
            }

            InitializeComponent();
            IconHelper.ApplyIcon(this);

            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);

            // 运行时：为列表添加双击编辑事件
            if (!UIStyleManager.IsDesignMode)
            {
                _listView.DoubleClick += OnListViewDoubleClick;
                _listView.AllowDrop = true;
                _listView.ItemDrag += OnListViewItemDrag;
                _listView.DragEnter += OnListViewDragEnter;
                _listView.DragOver += OnListViewDragOver;
                _listView.DragDrop += OnListViewDragDrop;

                /*
                var cms = new ContextMenuStrip();
                var miMilestone = new ToolStripMenuItem("追加里程碑...");
                var miDone = new ToolStripMenuItem("标记为已完成");
                miMilestone.Click += OnAppendMilestoneClick;
                miDone.Click += OnMarkDoneClick;
                cms.Items.Add(miMilestone);
                cms.Items.Add(miDone);
                _listView.ContextMenuStrip = cms;
                */

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
                        "会议;需求",
                        DateTime.Today.AddHours(9).ToString("yyyy-MM-dd HH:mm"),
                        DateTime.Today.AddHours(10).ToString("yyyy-MM-dd HH:mm")
                    }));
                    _listView.Items.Add(new ListViewItem(new[]
                    {
                        DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                        "示例：接口联调",
                        "修复返回格式，补充缺失字段",
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

                if (!_chkShowByMonth.Checked)
                {
                    // Auto-inherit logic
                    CheckAndInheritItems(svc, dataDir);

                    // 显示当天
                    _currentItems = _allMonthItems
                        .Where(d => d.LogDate.Date == selectedDate.Date)
                        .SelectMany(d => d.Items)
                        .OrderBy(i => i.SortOrder)
                        .ToList();
                }
                else
                {
                    // 显示全月
                    _currentItems = _allMonthItems
                        .SelectMany(d => d.Items)
                        .OrderBy(i => i.LogDate)
                        .ThenBy(i => i.SortOrder)
                        .ToList();
                }

                BindListView(_currentItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "加载数据失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindListView(System.Collections.Generic.List<WorkLogItem> items)
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();
            foreach (var item in items)
            {
                var lv = new ListViewItem(item.LogDate.ToString("yyyy-MM-dd")) { Tag = item };
                lv.SubItems.Add(item.ItemTitle ?? "");
                lv.SubItems.Add(item.ItemContent ?? "");
                lv.SubItems.Add(item.Tags ?? "");
                lv.SubItems.Add(item.StartTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                lv.SubItems.Add(item.EndTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                _listView.Items.Add(lv);
            }
            _listView.EndUpdate();
        }

        private void OnListViewDoubleClick(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;
            var item = _listView.SelectedItems[0].Tag as WorkLogItem;
            if (item == null) return;

            using (var form = new ItemEditForm(item, null))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 修改后保存
                    OnSaveClick(null, null);
                }
            }
        }

        private void CheckAndInheritItems(IImportExportService svc, string dataDir)
        {
            var targetDate = _dayPicker.Value.Date;
            var targetLog = _allMonthItems.FirstOrDefault(x => x.LogDate.Date == targetDate);
            
            // If today already has items, do nothing
            if (targetLog != null && targetLog.Items.Any()) return;
            
            var prevDate = targetDate.AddDays(-1);
            List<WorkLogItem> prevItems = null;

            // Check if prevDate is in current loaded month
            if (prevDate.Month == targetDate.Month && prevDate.Year == targetDate.Year)
            {
                var prevLog = _allMonthItems.FirstOrDefault(x => x.LogDate.Date == prevDate);
                if (prevLog != null) prevItems = prevLog.Items;
            }
            else
            {
                // Load previous month
                var prevMonthRef = new DateTime(prevDate.Year, prevDate.Month, 1);
                var prevMonthDays = svc.ImportMonth(prevMonthRef, dataDir);
                if (prevMonthDays != null)
                {
                    var prevLog = prevMonthDays.FirstOrDefault(x => x.LogDate.Date == prevDate);
                    if (prevLog != null) prevItems = prevLog.Items;
                }
            }

            if (prevItems != null && prevItems.Any())
            {
                // Only inherit 'Doing' items
                var inherited = prevItems.Where(i => i.Status == StatusEnum.Doing).ToList();
                if (inherited.Any())
                {
                    if (targetLog == null)
                    {
                        targetLog = new WorkLog { LogDate = targetDate, Items = new List<WorkLogItem>() };
                        _allMonthItems.Add(targetLog);
                    }

                    foreach (var item in inherited)
                    {
                        targetLog.Items.Add(new WorkLogItem
                        {
                            LogDate = targetDate,
                            ItemTitle = item.ItemTitle,
                            ItemContent = item.ItemContent,
                            CategoryId = item.CategoryId,
                            Status = StatusEnum.Doing,
                            Tags = item.Tags,
                            SortOrder = item.SortOrder
                        });
                    }

                    // Save changes
                    var monthRef = new DateTime(targetDate.Year, targetDate.Month, 1);
                    svc.RewriteMonth(monthRef, _allMonthItems, dataDir);
                }
            }
        }

        private void OnScopeToggled(object sender, EventArgs e)
        {
            SetScopeUI();
            RefreshItems();
        }

        private void SetScopeUI()
        {
            if (_chkShowByMonth.Checked)
            {
                _dayPicker.Visible = false;
                _monthPicker.Visible = true;
            }
            else
            {
                _dayPicker.Visible = true;
                _monthPicker.Visible = false;
            }
        }

        private void OnDayPickerValueChanged(object sender, EventArgs e)
        {
            if (!_chkShowByMonth.Checked)
            {
                RefreshItems();
            }
        }

        private void OnListViewItemDrag(object sender, ItemDragEventArgs e)
        {
            _listView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void OnListViewDragOver(object sender, DragEventArgs e)
        {
            var pt = _listView.PointToClient(new Point(e.X, e.Y));
            var item = _listView.GetItemAt(pt.X, pt.Y);
            if (item != null)
            {
                item.EnsureVisible();
            }
        }

        private void OnListViewDragDrop(object sender, DragEventArgs e)
        {
            var pt = _listView.PointToClient(new Point(e.X, e.Y));
            var targetItem = _listView.GetItemAt(pt.X, pt.Y);
            var draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

            if (draggedItem != null && targetItem != null && draggedItem != targetItem)
            {
                var items = _currentItems; // 当前显示的列表引用
                var srcLog = draggedItem.Tag as WorkLogItem;
                var dstLog = targetItem.Tag as WorkLogItem;

                if (srcLog != null && dstLog != null)
                {
                    // 简单交换 SortOrder？或者插入排序
                    // 这里做简单的插入排序逻辑：
                    // 在 UI 上移动位置
                    var targetIndex = targetItem.Index;
                    _listView.Items.Remove(draggedItem);
                    _listView.Items.Insert(targetIndex, draggedItem);
                    
                    // 触发保存逻辑会重新计算 SortOrder
                    // 但需要先更新内存中的 _currentItems 顺序
                    // _currentItems 只是一个 View，真实数据在 _allMonthItems
                    // 我们只需要标记顺序变了，在 Save 时会根据 ListView 顺序重写 SortOrder
                }
            }
        }

        private void UpdateSortOrderByCurrentView()
        {
            // 根据 ListView 中的顺序，更新 item.SortOrder
            for (int i = 0; i < _listView.Items.Count; i++)
            {
                var lv = _listView.Items[i];
                var item = lv.Tag as WorkLogItem;
                if (item != null)
                {
                    item.SortOrder = i;
                }
            }
        }

        // Legacy Event Handlers
        private void _monthPicker_ValueChanged(object sender, EventArgs e)
        {
            RefreshItems();
        }

        private void OnDailySummaryClick(object sender, EventArgs e)
        {
            var date = _dayPicker.Value.Date;
            var log = _allMonthItems.FirstOrDefault(x => x.LogDate.Date == date);
            var items = log?.Items ?? new List<WorkLogItem>();
            var summary = log?.DailySummary;

            using (var form = new DailySummaryForm(items, summary))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (log == null)
                    {
                        log = new WorkLog { LogDate = date, Items = new List<WorkLogItem>() };
                        _allMonthItems.Add(log);
                    }
                    log.DailySummary = form.SummaryText;
                    OnSaveClick(null, null);
                }
            }
        }

        private void OnOpenFileLocationClick(object sender, EventArgs e)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            catch { }
        }
    }
}
