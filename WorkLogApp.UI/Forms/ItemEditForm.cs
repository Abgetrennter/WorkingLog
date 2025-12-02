using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Enums;
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
        }

        private void InitializeFields()
        {
            // 状态
            var statusOptions = new System.Collections.Generic.List<StatusOption>
            {
                new StatusOption { Text = "待办", Value = StatusEnum.Todo },
                new StatusOption { Text = "进行中", Value = StatusEnum.Doing },
                new StatusOption { Text = "已完成", Value = StatusEnum.Done },
                new StatusOption { Text = "阻塞", Value = StatusEnum.Blocked },
                new StatusOption { Text = "已取消", Value = StatusEnum.Cancelled }
            };
            _statusComboBox.DisplayMember = "Text";
            _statusComboBox.ValueMember = "Value";
            _statusComboBox.DataSource = statusOptions;
            _statusComboBox.SelectedValue = _item.Status;

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

        private void OnSaveClickNew(object sender, EventArgs e)
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

                // 持久化到 Data\worklog_yyyyMM.xlsx
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);

                IImportExportService exportService = new ImportExportService();
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
                    $"已保存到 Excel 与文本备份:\n{Path.Combine(dataDir, "worklog_" + _item.LogDate.ToString("yyyyMM") + ".xlsx")}\n{filePath}",
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

        private void OnSaveClick(object sender, EventArgs e)
        {
            try
            {
                _item.ItemTitle = _titleBox.Text?.Trim();
                _item.ItemContent = _contentBox.Text ?? string.Empty;

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

                // Excel 导出（按月文件、按日 Sheet）
                IImportExportService exportService = new ImportExportService();
                var day = new WorkLog { LogDate = _item.LogDate.Date, Items = new System.Collections.Generic.List<WorkLogItem> { _item } };
                exportService.ExportMonth(_item.LogDate, new[] { day }, dataDir);

                // 可选：文本备份，便于直接查看
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

                MessageBox.Show(this, $"已保存到 Excel 与文本备份:\n{Path.Combine(dataDir, "worklog_" + _item.LogDate.ToString("yyyyMM") + ".xlsx")}\n{filePath}", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private class StatusOption
        {
            public string Text { get; set; }
            public StatusEnum Value { get; set; }
        }
    }
}