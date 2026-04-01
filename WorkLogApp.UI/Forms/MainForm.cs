using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkLogApp.Core.Constants;
using WorkLogApp.Core.Models;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly ITemplateService _templateService;
        private readonly IImportExportService _importExportService;
        private readonly IPdfExportService _pdfExportService;
        private readonly IWordExportService _wordExportService;
        private System.Collections.Generic.List<WorkLogItem> _allItems = new System.Collections.Generic.List<WorkLogItem>();
        private System.Collections.Generic.List<WorkLog> _allMonthItems = new System.Collections.Generic.List<WorkLog>();
        private ComboBox _statusFilterComboBox;

        // 设计期支持：提供无参构造，方便设计器实例化
        public MainForm()
        {
            // 设计时：使用空服务实例
            if (UIStyleManager.IsDesignMode)
            {
                _templateService = null;
                _importExportService = null;
                _pdfExportService = null;
                _wordExportService = null;
            }
            else
            {
                // 运行时：通过 DI 容器获取（由 Program 创建）
                throw new InvalidOperationException("请使用带参数的构造函数进行依赖注入");
            }
        }

        public MainForm(
            ITemplateService templateService,
            IImportExportService importExportService,
            IPdfExportService pdfExportService,
            IWordExportService wordExportService)
        {
            _templateService = templateService;
            _importExportService = importExportService;
            _pdfExportService = pdfExportService;
            _wordExportService = wordExportService;
            
            InitializeComponent();
            InitToolTips();
            IconHelper.ApplyIcon(this);

            // 应用 Fluent Design 主题
            FluentStyleManager.ApplyFluentTheme(this);
            
            // 配置 Fluent ToolBar
            SetupFluentToolBar();

            // 运行时：为列表添加双击编辑事件
            if (!UIStyleManager.IsDesignMode)
            {
                _listView.DoubleClick += OnListViewDoubleClick;
                _listView.AllowDrop = true;
                _listView.ItemDrag += OnListViewItemDrag;
                _listView.DragEnter += OnListViewDragEnter;
                _listView.DragOver += OnListViewDragOver;
                _listView.DragDrop += OnListViewDragDrop;

                // 右键菜单快捷操作
                var cms = new ContextMenuStrip();
                
                // 状态流转子菜单
                var miChangeStatus = new ToolStripMenuItem("更改状态");
                var miTodo = new ToolStripMenuItem("待办");
                var miDoing = new ToolStripMenuItem("进行中");
                var miDone = new ToolStripMenuItem("已完成");
                var miBlocked = new ToolStripMenuItem("阻塞");
                var miCancelled = new ToolStripMenuItem("已取消");
                
                miTodo.Click += (s, e) => OnChangeStatusClick(StatusEnum.Todo);
                miDoing.Click += (s, e) => OnChangeStatusClick(StatusEnum.Doing);
                miDone.Click += (s, e) => OnChangeStatusClick(StatusEnum.Done);
                miBlocked.Click += (s, e) => OnChangeStatusClick(StatusEnum.Blocked);
                miCancelled.Click += (s, e) => OnChangeStatusClick(StatusEnum.Cancelled);
                
                miChangeStatus.DropDownItems.Add(miTodo);
                miChangeStatus.DropDownItems.Add(miDoing);
                miChangeStatus.DropDownItems.Add(miDone);
                miChangeStatus.DropDownItems.Add(miBlocked);
                miChangeStatus.DropDownItems.Add(miCancelled);
                
                cms.Items.Add(miChangeStatus);
                cms.Items.Add(new ToolStripSeparator());
                
                // 追加进展
                var miAppendProgress = new ToolStripMenuItem("追加进展...");
                miAppendProgress.Click += OnAppendProgressClick;
                cms.Items.Add(miAppendProgress);
                
                // 删除项
                var miDelete = new ToolStripMenuItem("删除");
                miDelete.Click += OnDeleteItemClick;
                cms.Items.Add(miDelete);
                
                _listView.ContextMenuStrip = cms;

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
                        "Doing",
                        "讨论近期版本目标与测试范围……",
                        "会议;需求",
                        DateTime.Today.AddHours(9).ToString("yyyy-MM-dd HH:mm"),
                        DateTime.Today.AddHours(10).ToString("yyyy-MM-dd HH:mm")
                    }));
                    _listView.Items.Add(new ListViewItem(new[]
                    {
                        DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                        "示例：接口联调",
                        "Done",
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

        /// <summary>
        /// 窗体大小改变时调整列宽
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustListViewColumnWidths();
        }

        /// <summary>
        /// 根据窗体大小自适应调整ListView列宽
        /// </summary>
        private void AdjustListViewColumnWidths()
        {
            if (_listView == null || _listView.Columns.Count == 0) return;

            // 获取可用宽度（减去滚动条宽度）
            int availableWidth = _listView.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            if (availableWidth <= 0) return;

            // 计算最小列宽总和（7列：日期120、标题150、状态80、内容200、标签100、开始100、结束100）
            int minTotalWidth = 120 + 150 + 80 + 200 + 100 + 100 + 100;

            // 确保可用宽度至少为最小宽度，避免极端情况下的异常
            availableWidth = Math.Max(availableWidth, minTotalWidth);

            // 按比例分配列宽
            double scaleFactor = (double)availableWidth / minTotalWidth;

            _listView.Columns[0].Width = (int)(120 * scaleFactor);  // 日期
            _listView.Columns[1].Width = (int)(150 * scaleFactor);  // 标题
            _listView.Columns[2].Width = Math.Max(60, (int)(80 * scaleFactor));   // 状态（最小60）
            _listView.Columns[3].Width = (int)(200 * scaleFactor);  // 内容
            _listView.Columns[4].Width = (int)(100 * scaleFactor);  // 标签
            _listView.Columns[5].Width = (int)(100 * scaleFactor);  // 开始
            _listView.Columns[6].Width = (int)(100 * scaleFactor);  // 结束
        }

        /// <summary>
        /// 配置 Fluent 风格的工具栏
        /// </summary>
        private void SetupFluentToolBar()
        {
            if (UIStyleManager.IsDesignMode) return;

            // 清空现有控件
            _toolBar.LeftGroup.Controls.Clear();
            _toolBar.CenterGroup.Controls.Clear();
            _toolBar.RightGroup.Controls.Clear();

            // 左侧：主要操作按钮
            var btnCreate = _toolBar.CreatePrimaryButton("+ 创建事项");
            btnCreate.Click += OnCreateItemClick;
            _toolBar.AddToLeft(btnCreate);

            var btnTodo = _toolBar.CreateSecondaryButton("待办事项");
            btnTodo.Click += OnTodoClick;
            _toolBar.AddToLeft(btnTodo);

            _toolBar.AddSeparator(_toolBar.LeftGroup);

            // 中间：日期选择区域 + 状态过滤
            _chkShowByMonth.Text = "按月";
            _chkShowByMonth.AutoSize = true;
            _chkShowByMonth.Dock = DockStyle.Left;
            _chkShowByMonth.Margin = new Padding(8, 8, 4, 0);
            _toolBar.AddToCenter(_chkShowByMonth);

            _dayPicker.Width = AppConstants.ListViewDatePickerWidth;
            _dayPicker.Dock = DockStyle.Left;
            _dayPicker.Margin = new Padding(4, 4, 4, 0);
            FluentStyleManager.ApplyFluentStyle(_dayPicker);
            _toolBar.AddToCenter(_dayPicker);

            _monthPicker.Width = AppConstants.ListViewMonthPickerWidth;
            _monthPicker.Dock = DockStyle.Left;
            _monthPicker.Margin = new Padding(4, 4, 4, 0);
            FluentStyleManager.ApplyFluentStyle(_monthPicker);
            _toolBar.AddToCenter(_monthPicker);

            // 状态过滤下拉框
            _statusFilterComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Margin = new Padding(8, 4, 4, 0),
                Dock = DockStyle.Left
            };
            _statusFilterComboBox.Items.Add("全部状态");
            _statusFilterComboBox.Items.Add("仅未完成");
            _statusFilterComboBox.Items.Add("待办");
            _statusFilterComboBox.Items.Add("进行中");
            _statusFilterComboBox.Items.Add("已完成");
            _statusFilterComboBox.Items.Add("阻塞");
            _statusFilterComboBox.Items.Add("已取消");
            _statusFilterComboBox.SelectedIndex = 0;
            _statusFilterComboBox.SelectedIndexChanged += OnStatusFilterChanged;
            FluentStyleManager.ApplyFluentStyle(_statusFilterComboBox);
            _toolBar.AddToCenter(_statusFilterComboBox);

            _toolBar.AddSeparator(_toolBar.CenterGroup);

            var btnDailySummary = _toolBar.CreateGhostButton("每日总结");
            btnDailySummary.Click += OnDailySummaryClick;
            _toolBar.AddToCenter(btnDailySummary);

            // 右侧：工具按钮
            var btnCategory = _toolBar.CreateGhostButton("分类");
            btnCategory.Click += OnCategoryManageClick;
            _toolBar.AddToRight(btnCategory);

            var btnImport = _toolBar.CreateGhostButton("导入");
            btnImport.Click += OnImportWizardClick;
            _toolBar.AddToRight(btnImport);

            var btnRefresh = _toolBar.CreateGhostButton("刷新");
            btnRefresh.Click += OnImportMonthClick;
            _toolBar.AddToRight(btnRefresh);

            _toolBar.AddSeparator(_toolBar.RightGroup);

            var btnSave = _toolBar.CreateSecondaryButton("保存");
            btnSave.Click += OnSaveClick;
            _toolBar.AddToRight(btnSave);

            var btnOpenExcel = _toolBar.CreateGhostButton("Excel");
            btnOpenExcel.Click += OnOpenFileLocationClick;
            _toolBar.AddToRight(btnOpenExcel);

            _toolBar.AddSeparator(_toolBar.RightGroup);

            var btnExport = _toolBar.CreatePrimaryButton("导出");
            btnExport.Click += OnExportClick;
            _toolBar.AddToRight(btnExport);
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var relativePath = ConfigurationManager.AppSettings[AppConstants.DataPathConfigKey] ?? AppConstants.DataDirectoryName;
                var dataDir = Path.Combine(baseDir, relativePath);
                Directory.CreateDirectory(dataDir);

                using (var dialog = new ExportDialog(
                    _importExportService,
                    _pdfExportService,
                    _wordExportService,
                    _allMonthItems,
                    dataDir))
                {
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"导出功能初始化失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCreateItemClick(object sender, EventArgs e)
        {
            var initialDate = _dayPicker.Value;
            using (var form = Program.Container.GetInstance<ItemCreateForm>())
            {
                form.SetInitialDate(initialDate);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
                // 保存后刷新当前月份数据
                RefreshMonthItems();
            }
        }

        private void OnTodoClick(object sender, EventArgs e)
        {
            using (var form = Program.Container.GetInstance<TodoForm>())
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// 刷新月份数据（异步）
        /// </summary>
        private async void OnImportMonthClick(object sender, EventArgs e)
        {
            await RefreshItemsAsync();
        }

        /// <summary>
        /// 保存数据到 Excel（异步）
        /// </summary>
        private async void OnSaveClick(object sender, EventArgs e)
        {
            await SaveDataAsync();
        }

        /// <summary>
        /// 保存数据到 Excel（异步实现）
        /// </summary>
        private async Task SaveDataAsync()
        {
            try
            {
                // 根据当前视图更新 SortOrder
                UpdateSortOrderByCurrentView();

                // 保存到当月 Excel
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var relativePath = ConfigurationManager.AppSettings[AppConstants.DataPathConfigKey] ?? AppConstants.DataDirectoryName;
                var dataDir = Path.Combine(baseDir, relativePath);
                Directory.CreateDirectory(dataDir);

                var selectedDate = _dayPicker.Value;
                var monthRef = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                if (_chkShowByMonth.Checked)
                {
                    monthRef = new DateTime(_monthPicker.Value.Year, _monthPicker.Value.Month, 1);
                }

                // 使用 Task.Run 在后台线程执行耗时操作
                await Task.Run(() => _importExportService.RewriteMonth(monthRef, _allMonthItems, dataDir));

                // 保存后重新绑定，以反映可能的排序变化
                await RefreshItemsAsync();
                MessageBox.Show(this, "已保存排序并更新当月 Excel。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新数据（异步实现）
        /// </summary>
        private async Task RefreshItemsAsync()
        {
            await Task.Run(() => RefreshItems());
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
            using (var form = Program.Container.GetInstance<CategoryManageForm>())
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
            }
        }

        private void OnImportWizardClick(object sender, EventArgs e)
        {
            using (var form = Program.Container.GetInstance<ImportWizardForm>())
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
                RefreshMonthItems();
            }
        }

        private void FixUpCategoryNames(List<WorkLog> logs)
        {
            try
            {
                var categories = _templateService.GetAllCategories();
                if (categories == null) return;
                
                var idToName = categories.ToDictionary(c => c.Id, c => c.Name, StringComparer.OrdinalIgnoreCase);
                
                foreach (var log in logs)
                {
                    if (log.Items == null) continue;
                    foreach (var item in log.Items)
                    {
                        if (!string.IsNullOrEmpty(item.CategoryName) && idToName.ContainsKey(item.CategoryName))
                        {
                            item.CategoryName = idToName[item.CategoryName];
                        }
                    }
                }
            }
            catch { /* Ignore errors during fixup */ }
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
                var relativePath = ConfigurationManager.AppSettings[AppConstants.DataPathConfigKey] ?? AppConstants.DataDirectoryName;
                var dataDir = Path.Combine(baseDir, relativePath);
                Directory.CreateDirectory(dataDir);

                var selectedDate = _dayPicker.Value;
                var monthRef = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                if (_chkShowByMonth.Checked)
                {
                    monthRef = new DateTime(_monthPicker.Value.Year, _monthPicker.Value.Month, 1);
                }

                var monthDays = _importExportService.ImportMonth(monthRef, dataDir) ?? Enumerable.Empty<WorkLog>();
                _allMonthItems = monthDays.ToList();

                // 兼容性处理：如果加载的数据中 CategoryName 是 ID，则转换为名称
                FixUpCategoryNames(_allMonthItems);

                if (!_chkShowByMonth.Checked)
                {
                    // Auto-inherit logic
                    CheckAndInheritItems(_importExportService, dataDir);

                    // 显示当天
                    _allItems = _allMonthItems
                        .Where(d => d.LogDate.Date == selectedDate.Date)
                        .SelectMany(d => d.Items)
                        .OrderBy(i => i.SortOrder)
                        .ToList();
                }
                else
                {
                    // 显示全月
                    _allItems = _allMonthItems
                        .SelectMany(d => d.Items)
                        .OrderBy(i => i.LogDate)
                        .ThenBy(i => i.SortOrder)
                        .ToList();
                }

                // 应用状态过滤
                var displayItems = ApplyStatusFilter(_allItems);
                
                BindListView(displayItems);
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
                lv.SubItems.Add(item.Status.ToChinese());
                lv.SubItems.Add(item.ItemContent ?? "");
                lv.SubItems.Add(item.Tags ?? "");
                lv.SubItems.Add(item.StartTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                lv.SubItems.Add(item.EndTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                
                // 状态色彩编码
                ApplyStatusColor(lv, item.Status);
                
                _listView.Items.Add(lv);
            }
            _listView.EndUpdate();
        }

        /// <summary>
        /// 根据状态应用颜色编码
        /// </summary>
        private void ApplyStatusColor(ListViewItem item, StatusEnum status)
        {
            switch (status)
            {
                case StatusEnum.Todo:
                    // 待办 - 灰色
                    item.ForeColor = Color.Gray;
                    break;
                case StatusEnum.Doing:
                    // 进行中 - 醒目蓝色
                    item.ForeColor = Color.FromArgb(0, 120, 215);
                    break;
                case StatusEnum.Done:
                    // 已完成 - 绿色
                    item.ForeColor = Color.FromArgb(0, 153, 0);
                    break;
                case StatusEnum.Blocked:
                    // 阻塞 - 红色
                    item.ForeColor = Color.FromArgb(200, 0, 0);
                    break;
                case StatusEnum.Cancelled:
                    // 已取消 - 浅灰色
                    item.ForeColor = Color.LightGray;
                    break;
            }
        }

        /// <summary>
        /// 状态过滤改变事件
        /// </summary>
        private void OnStatusFilterChanged(object sender, EventArgs e)
        {
            if (_allItems == null) return;
            var displayItems = ApplyStatusFilter(_allItems);
            BindListView(displayItems);
        }

        /// <summary>
        /// 应用状态过滤到当前列表，返回过滤后的列表
        /// </summary>
        private System.Collections.Generic.List<WorkLogItem> ApplyStatusFilter(System.Collections.Generic.List<WorkLogItem> items)
        {
            if (_statusFilterComboBox == null || items == null) return items;
            
            var selectedIndex = _statusFilterComboBox.SelectedIndex;
            if (selectedIndex == 0)
            {
                // 全部状态 - 不过滤
                return items;
            }

            // 创建一个过滤后的临时列表用于显示
            var filteredItems = new System.Collections.Generic.List<WorkLogItem>();
            
            switch (selectedIndex)
            {
                case 1: // 仅未完成
                    filteredItems.AddRange(items.Where(i => StatusHelper.IsIncomplete(i.Status)));
                    break;
                case 2: // 待办
                    filteredItems.AddRange(items.Where(i => i.Status == StatusEnum.Todo));
                    break;
                case 3: // 进行中
                    filteredItems.AddRange(items.Where(i => i.Status == StatusEnum.Doing));
                    break;
                case 4: // 已完成
                    filteredItems.AddRange(items.Where(i => i.Status == StatusEnum.Done));
                    break;
                case 5: // 阻塞
                    filteredItems.AddRange(items.Where(i => i.Status == StatusEnum.Blocked));
                    break;
                case 6: // 已取消
                    filteredItems.AddRange(items.Where(i => i.Status == StatusEnum.Cancelled));
                    break;
            }
            
            return filteredItems;
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

        /// <summary>
        /// 右键菜单 - 更改状态
        /// </summary>
        private void OnChangeStatusClick(StatusEnum newStatus)
        {
            if (_listView.SelectedItems.Count == 0) return;
            var item = _listView.SelectedItems[0].Tag as WorkLogItem;
            if (item == null) return;

            var result = MessageBox.Show(
                this,
                $"确定要将 \"{item.ItemTitle}\" 的状态更改为 \"{StatusHelper.ToChinese(newStatus)}\" 吗？",
                "确认更改状态",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            item.Status = newStatus;
            
            // 如果设置为已完成，自动填充结束时间
            if (newStatus == StatusEnum.Done && !item.EndTime.HasValue)
            {
                item.EndTime = DateTime.Now;
            }
            
            // 如果设置为进行中，自动填充开始时间
            if (newStatus == StatusEnum.Doing && !item.StartTime.HasValue)
            {
                item.StartTime = DateTime.Now;
            }

            // 保存更改
            OnSaveClick(null, null);
        }

        /// <summary>
        /// 右键菜单 - 追加进展
        /// </summary>
        private void OnAppendProgressClick(object sender, EventArgs e)
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

        /// <summary>
        /// 右键菜单 - 删除项
        /// </summary>
        private void OnDeleteItemClick(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;
            var item = _listView.SelectedItems[0].Tag as WorkLogItem;
            if (item == null) return;

            var result = MessageBox.Show(
                this,
                $"确定要删除 \"{item.ItemTitle}\" 吗？此操作不可恢复。",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            // 从列表中移除
            _allItems.Remove(item);
            
            // 从月份数据中移除
            var workLog = _allMonthItems.FirstOrDefault(w => w.LogDate.Date == item.LogDate.Date);
            if (workLog != null)
            {
                workLog.Items.Remove(item);
            }

            // 保存更改
            OnSaveClick(null, null);
        }

        private void CheckAndInheritItems(IImportExportService svc, string dataDir)
        {
            var targetDate = _dayPicker.Value.Date;

            // 1. 不自动继承到未来日期
            if (targetDate >= DateTime.Today.AddDays(1)) return;

            // 2. 周六周日不自动继承
            //if (targetDate.DayOfWeek == DayOfWeek.Saturday || targetDate.DayOfWeek == DayOfWeek.Sunday) return;

            var targetLog = _allMonthItems.FirstOrDefault(x => x.LogDate.Date == targetDate);
            
            // If today already has items, do nothing
            if (targetLog != null && targetLog.Items.Any()) return;
            
            // 3. 寻找上一工作日（跳过周末）
            var prevDate = targetDate.AddDays(-1);
            while (prevDate.DayOfWeek == DayOfWeek.Saturday || prevDate.DayOfWeek == DayOfWeek.Sunday)
            {
                prevDate = prevDate.AddDays(-1);
            }

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
                // 继承所有未完成状态的任务 (Todo, Doing, Blocked)
                var inherited = prevItems.Where(i => StatusHelper.IsIncomplete(i.Status)).ToList();
                if (inherited.Any())
                {
                    if (targetLog == null)
                    {
                        targetLog = new WorkLog { LogDate = targetDate, Items = new List<WorkLogItem>() };
                        _allMonthItems.Add(targetLog);
                    }

                    foreach (var item in inherited)
                    {
                        // 支持新旧多种格式的清理
                        var cleanContent = System.Text.RegularExpressions.Regex.Replace(
                            item.ItemContent ?? "", 
                            @"(?:<!-- DAILY_PROGRESS .*? -->[\s\S]*?<!-- END_DAILY_PROGRESS -->|—— \d{4}-\d{2}-\d{2} 进展 ——[\s\S]*?—— 结束 ——|——————————\s*\n【\d{4}-\d{2}-\d{2} 进展】[\s\S]*?——————————)\s*", 
                            "");

                        // 继承原条目的追踪ID，若原条目没有则生成新的
                        var trackingId = item.TrackingId;
                        if (string.IsNullOrEmpty(trackingId))
                        {
                            trackingId = Guid.NewGuid().ToString();
                            // 可选：更新原条目的TrackingId（需要持久化）
                            // 暂不实现，因为迁移机制会处理
                        }
                        targetLog.Items.Add(new WorkLogItem
                        {
                            LogDate = targetDate,
                            ItemTitle = item.ItemTitle,
                            ItemContent = cleanContent,
                            CategoryName = item.CategoryName,
                            Status = StatusEnum.Doing,
                            Tags = item.Tags,
                            SortOrder = item.SortOrder,
                            TrackingId = trackingId
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
                var items = _allItems; // 当前显示的列表引用
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
                    // 但需要先更新内存中的 _allItems 顺序
                    // _allItems 只是一个 View，真实数据在 _allMonthItems
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
                if (item is WorkLogItem workLogItem)
                {
                    workLogItem.SortOrder = i;
                }
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

        private void InitToolTips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_btnCreate, "创建新的日志事项");
            toolTip.SetToolTip(_btnTodo, "待办事项 (自动保存)");
            toolTip.SetToolTip(_btnCategoryManage, "管理分类和模板");
            toolTip.SetToolTip(_btnImportWizard, "导入外部日志数据");
            toolTip.SetToolTip(_btnImport, "刷新当前数据显示");
            toolTip.SetToolTip(_btnDailySummary, "编辑每日总结内容");
            toolTip.SetToolTip(_btnSave, "保存所有更改到本地文件");
            toolTip.SetToolTip(_btnOpenFileLocation, "打开数据文件所在目录");
            toolTip.SetToolTip(_chkShowByMonth, "切换显示模式：选中则显示整月数据，否则仅显示选中日期的数据");
        }

        private void OnOpenFileLocationClick(object sender, EventArgs e)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.DataDirectoryName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            catch { }
        }
    }
}
