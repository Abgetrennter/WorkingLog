using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
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
            IconHelper.ApplyIcon(this);
            InitializeFields();
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
            InitToolTips();
            
            // 绑定事件
            // 先解绑以防重复（虽然构造函数只调一次，但为了保险）
            _btnAddProgress.Click -= OnAddProgressClick;
            _btnAddProgress.Click += OnAddProgressClick;
            
            _btnComplete.Click -= OnCompleteClick;
            _btnComplete.Click += OnCompleteClick;

            _statusComboBox.SelectedIndexChanged -= OnStatusChanged;
            _statusComboBox.SelectedIndexChanged += OnStatusChanged;

            // 确保窗口加载时刷新布局可见性
            this.Load += (s, e) => UpdateVisibility(_item.Status);
        }

        private void OnAddProgressClick(object sender, EventArgs e)
        {
            var text = _txtDailyProgress.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                // 如果为空，视为仅执行保存逻辑（如果有其他修改）
                OnSaveClick(sender, e);
                return;
            }

            var dateStr = _item.LogDate.ToString("yyyy-MM-dd");
            var formatted = $"\n\n——————————\n【{dateStr} 进展】\n{text}\n——————————\n";
            _contentBox.AppendText(formatted);
            _txtDailyProgress.Clear();
            
            // 自动触发保存
            OnSaveClick(sender, e);
        }

        private void OnCompleteClick(object sender, EventArgs e)
        {
            // 1. 追加当日进展（如果有）
            var text = _txtDailyProgress.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var dateStr = _item.LogDate.ToString("yyyy-MM-dd");
                var formatted = $"\n\n——————————\n【{dateStr} 进展】\n{text}\n——————————\n";
                _contentBox.AppendText(formatted);
                _txtDailyProgress.Clear();
            }

            // 2. 更改状态为已完成
            _item.Status = StatusEnum.Done;
            // 同步 UI
            _statusComboBox.SelectedValue = StatusEnum.Done;
            // 更新可见性（此时会显示时间选择器，但我们将立即保存关闭，所以用户可能看不到变化，这没关系）
            // UpdateVisibility(StatusEnum.Done); 

            // 3. 执行追溯汇总逻辑 (Unique to this button)
            // 需要构造服务
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            IImportExportService exportService = Program.Container?.GetInstance<IImportExportService>();
            if (exportService == null)
            {
                // 如果容器不可用，尝试创建（设计时支持）
                var pdfService = new PdfExportService();
                var wordService = new WordExportService();
                exportService = new ImportExportService(pdfService, wordService);
            }

            TraceBackAndMergeProgress(exportService, dataDir);
            
            // 更新内容框以反映汇总结果
            _contentBox.Text = _item.ItemContent;

            // 4. 保存
            OnSaveClick(sender, e);
        }

        private void OnStatusChanged(object sender, EventArgs e)
        {
            if (_statusComboBox.SelectedValue is StatusEnum s)
            {
                UpdateVisibility(s);
            }
        }

        private void UpdateVisibility(StatusEnum status)
        {
            var isDone = status == StatusEnum.Done;
            // 如果不是 Done，隐藏时间选择器
            // 需求：未完成（Todo, Doing）显示“当日进展”，隐藏“开始/结束时间”
            // 已完成（Done）显示“开始/结束时间”，隐藏“当日进展”
            // 其他（Blocked, Cancelled）暂按已完成处理（显示时间，隐藏进展）或按需调整

            var isUnfinished = status == StatusEnum.Todo || status == StatusEnum.Doing;

            var showTime = !isUnfinished; // Done, Blocked, Cancelled 显示时间
            var showDailyProgress = isUnfinished; // Todo, Doing 显示当日进展

            _startPicker.Visible = showTime;
            _endPicker.Visible = showTime;
            lblStart.Visible = showTime;
            lblEnd.Visible = showTime;

            lblDailyProgress.Visible = showDailyProgress;
            _txtDailyProgress.Visible = showDailyProgress;
            // 隐藏整个按钮面板
            progressButtonPanel.Visible = showDailyProgress;

            // 确保底部按钮栏始终可见
            // 需求调整：进行中/未完成状态（显示当日进展UI时）不显示底部的保存/取消按钮
            // 只有已完成/其他状态（显示经典UI时）才显示底部按钮
            var showBottomBar = !isUnfinished;

            bottomBar.Visible = showBottomBar;
            if (showBottomBar)
            {
                bottomBar.BringToFront();
            }
            _btnSave.Visible = showBottomBar;
            _btnCancel.Visible = showBottomBar;
            
            // 强制重新布局
            rootLayout.PerformLayout();
            this.PerformLayout();
        }

        
        public ItemEditForm(WorkLogItem item, string initialContent)
        {
            _item = item ?? new WorkLogItem { LogDate = DateTime.Now.Date };
            InitializeComponent();
            IconHelper.ApplyIcon(this);

            _titleBox.Text = _item.ItemTitle ?? string.Empty;
            _contentBox.Text = initialContent ?? _item.ItemContent ?? string.Empty;

            InitializeFields();

            // 应用统一样式并设置 1.5 倍行距
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
            InitToolTips();
            
            // 绑定事件
            // 先解绑以防重复
            _btnAddProgress.Click -= OnAddProgressClick;
            _btnAddProgress.Click += OnAddProgressClick;
            
            _btnComplete.Click -= OnCompleteClick;
            _btnComplete.Click += OnCompleteClick;

            _statusComboBox.SelectedIndexChanged -= OnStatusChanged;
            _statusComboBox.SelectedIndexChanged += OnStatusChanged;

            // 确保窗口加载时刷新布局可见性
            this.Load += (s, e) => UpdateVisibility(_item.Status);
        }

        private void InitToolTips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_btnSave, "保存修改并关闭窗口");
            toolTip.SetToolTip(_btnCancel, "放弃修改并关闭窗口");
            toolTip.SetToolTip(_sortUpDown, "设置排序权重，数字越小越靠前");
        }

        private void InitializeFields()
        {
            // 状态
            _statusComboBox.DisplayMember = "Value";
            _statusComboBox.ValueMember = "Key";
            _statusComboBox.DataSource = StatusHelper.GetList();
            _statusComboBox.SelectedValue = _item.Status;
            
            UpdateVisibility(_item.Status);

            // 日期
            _datePicker.Value = _item.LogDate == default(DateTime) ? DateTime.Now.Date : _item.LogDate;
            // 标签
            _tagsBox.Text = _item.Tags ?? string.Empty;
            // 开始时间
            if (_item.StartTime.HasValue)
            {
                _startPicker.Checked = true;
                _startPicker.Value = _item.StartTime.Value;
            }
            else
            {
                _startPicker.Checked = false;
            }
            // 结束时间
            if (_item.EndTime.HasValue)
            {
                _endPicker.Checked = true;
                _endPicker.Value = _item.EndTime.Value;
            }
            else
            {
                _endPicker.Checked = false;
            }
            // 排序
            _sortUpDown.Value = _item.SortOrder.HasValue ? _item.SortOrder.Value : 0;
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                // 基本校验
                var title = _titleBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(title))
                {
                    MessageBox.Show(this, "标题不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _titleBox.Focus();
                    return;
                }

                // （已移除状态选择校验）

                if (_startPicker.Checked && _endPicker.Checked && _endPicker.Value < _startPicker.Value)
                {
                    MessageBox.Show(this, "结束时间不能早于开始时间", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _endPicker.Focus();
                    return;
                }

                // 写入模型
                _item.ItemTitle = title;
                _item.ItemContent = _contentBox.Text ?? string.Empty;
                _item.LogDate = _datePicker.Value.Date;
                
                if (_statusComboBox.SelectedValue is StatusEnum s)
                {
                    _item.Status = s;
                }

                _item.Tags = _tagsBox.Text?.Trim();
                _item.StartTime = _startPicker.Checked ? (DateTime?)_startPicker.Value : null;
                _item.EndTime = _endPicker.Checked ? (DateTime?)_endPicker.Value : null;
                _item.SortOrder = (int)_sortUpDown.Value;

                // 持久化到 Data\工作日志_yyyyMM.xlsx
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);

                IImportExportService exportService = Program.Container?.GetInstance<IImportExportService>();
                if (exportService == null)
                {
                    // 如果容器不可用，尝试创建（设计时支持）
                    var pdfService = new PdfExportService();
                    var wordService = new WordExportService();
                    exportService = new ImportExportService(pdfService, wordService);
                }

                // (移除) 如果状态为已完成，执行追溯汇总逻辑
                // if (_item.Status == StatusEnum.Done) ...
                // 现已移至 OnCompleteClick 单独触发

                var day = new WorkLog { LogDate = _item.LogDate.Date, Items = new System.Collections.Generic.List<WorkLogItem> { _item } };
                var ok = exportService.ExportMonth(_item.LogDate, new[] { day }, dataDir);
                if (!ok)
                {
                    MessageBox.Show(this, "保存失败：导出未成功", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // 保持窗口打开
                }

                // 文本备份（可选）
                var safeTitle = string.IsNullOrWhiteSpace(_item.ItemTitle) ? "untitled" : SanitizeFileName(_item.ItemTitle);
                var fileName = $"{_item.LogDate:yyyy-MM-dd}_{safeTitle}.txt";
                
                // 按照 yyyy/MM/dd 结构存储
                var year = _item.LogDate.ToString("yyyy");
                var month = _item.LogDate.ToString("MM");
                var dayStr = _item.LogDate.ToString("dd");
                var txtDir = Path.Combine(dataDir, year, month, dayStr);
                if (!Directory.Exists(txtDir)) Directory.CreateDirectory(txtDir);

                var filePath = Path.Combine(txtDir, fileName);
                File.WriteAllText(filePath, _item.ItemContent);

                MessageBox.Show(this,
                    $"已保存到 Excel 与文本备份:\n{Path.Combine(dataDir, ImportExportService.FilePrefix + _item.LogDate.ToString("yyyyMM") + ".xlsx")}\n{filePath}",
                    "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 不关闭窗口，以便用户修正问题
            }
        }

        private static string SanitizeFileName(string name)
        {
            // 移除 Windows 非法文件名字符
            var invalid = new string(Path.GetInvalidFileNameChars());
            var pattern = "[" + Regex.Escape(invalid) + "]";
            return Regex.Replace(name, pattern, "_");
        }

        private void TraceBackAndMergeProgress(IImportExportService svc, string dataDir)
        {
            var currentTitle = _item.ItemTitle;
            var collectedProgress = new System.Collections.Generic.List<string>();
            
            // 检查当日是否也有进展（尚未保存到 Content 中，但如果在 txtDailyProgress 中有内容，也应该包含？）
            // 用户逻辑是：点击“确认添加”后进入 Content。所以此时 Content 应该已经包含了当日进展。
            // 我们还需要提取当日 Content 中的进展吗？
            // 需求：汇总所有的追溯的日志的每日进度。
            // 包括当日的吗？通常包括。
            // 提取当日内容中的进展
            var todayProgress = ExtractProgress(_item.ItemContent);
            if (!string.IsNullOrWhiteSpace(todayProgress))
            {
                collectedProgress.Add(todayProgress);
            }

            var checkDate = _item.LogDate.AddDays(-1);
            var notFoundCount = 0;
            
            // 简单的内存缓存，避免重复加载同一月文件
            var loadedMonths = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<WorkLog>>();

            while (notFoundCount <= 3)
            {
                var monthKey = checkDate.ToString("yyyyMM");
                System.Collections.Generic.List<WorkLog> monthLogs = null;

                if (loadedMonths.ContainsKey(monthKey))
                {
                    monthLogs = loadedMonths[monthKey];
                }
                else
                {
                    var monthRef = new DateTime(checkDate.Year, checkDate.Month, 1);
                    var logs = svc.ImportMonth(monthRef, dataDir);
                    monthLogs = logs != null ? new System.Collections.Generic.List<WorkLog>(logs) : new System.Collections.Generic.List<WorkLog>();
                    loadedMonths[monthKey] = monthLogs;
                }

                var dayLog = monthLogs.FirstOrDefault(x => x.LogDate.Date == checkDate.Date);
                var foundItem = dayLog?.Items?.FirstOrDefault(x => x.TrackingId == _item.TrackingId);

                if (foundItem != null)
                {
                    notFoundCount = 0;
                    var p = ExtractProgress(foundItem.ItemContent);
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        collectedProgress.Add(p);
                    }
                }
                else
                {
                    notFoundCount++;
                }

                checkDate = checkDate.AddDays(-1);
                if ((_item.LogDate - checkDate).TotalDays > 365) break; // 防死循环
            }

            collectedProgress.Reverse(); // 时间正序

            if (collectedProgress.Count > 0)
            {
                // 移除原有的 Daily Progress 标记块，生成纯净的汇总？
                // 或者保留？
                // 需求：汇总...合并到已完成的日志条目中
                
                var summary = string.Join("\n", collectedProgress);
                
                // 清理掉原有的分散的进展块（如果希望最终结果整洁）
                // _item.ItemContent = RemoveAllProgressBlocks(_item.ItemContent);
                // 加上汇总
                
                if (!_item.ItemContent.Contains("【项目全周期进展汇总】"))
                {
                    _item.ItemContent += "\n\n【项目全周期进展汇总】\n" + summary;
                }
            }
        }

        private string ExtractProgress(string content)
        {
            if (string.IsNullOrEmpty(content)) return null;
            
            // 新格式：——————————\n【yyyy-MM-dd 进展】\n...\n——————————
            var regex = new Regex(@"——————————\s*\n【\d{4}-\d{2}-\d{2} 进展】\s*\n([\s\S]*?)\n——————————");
            var match = regex.Match(content);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            
            // 兼容旧格式1：—— yyyy-MM-dd 进展 —— ... —— 结束 ——
            var regexV2 = new Regex(@"—— \d{4}-\d{2}-\d{2} 进展 ——([\s\S]*?)—— 结束 ——");
            var matchV2 = regexV2.Match(content);
            if (matchV2.Success)
            {
                return matchV2.Groups[1].Value.Trim();
            }

            // 兼容旧格式2
            var oldRegex = new Regex(@"<!-- DAILY_PROGRESS .*? -->([\s\S]*?)<!-- END_DAILY_PROGRESS -->");
            var oldMatch = oldRegex.Match(content);
            if (oldMatch.Success)
            {
                return oldMatch.Groups[1].Value.Trim();
            }

            return null;
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}