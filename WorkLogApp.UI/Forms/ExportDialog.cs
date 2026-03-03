using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    /// <summary>
    /// 导出对话框 - 选择时间范围和导出格式
    /// </summary>
    public partial class ExportDialog : Form
    {
        private readonly IImportExportService _importExportService;
        private readonly IPdfExportService _pdfExportService;
        private readonly IWordExportService _wordExportService;
        private readonly List<WorkLog> _allLogs;
        private readonly string _dataDir;

        // 导出格式
        public enum ExportFormat
        {
            Excel,
            Pdf,
            Word
        }

        public ExportDialog(
            IImportExportService importExportService,
            IPdfExportService pdfExportService,
            IWordExportService wordExportService,
            List<WorkLog> allLogs,
            string dataDir)
        {
            _importExportService = importExportService;
            _pdfExportService = pdfExportService;
            _wordExportService = wordExportService;
            _allLogs = allLogs;
            _dataDir = dataDir;

            InitializeComponent();
            InitializeFluentStyle();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Text = "导出工作日志";
            this.Size = new Size(520, 420);
            this.MinimumSize = new Size(480, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(24);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // 主布局
            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(16)
            };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 时间范围
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 格式选择
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 说明
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 按钮

            // 时间范围组
            var timeGroup = new GroupBox
            {
                Text = "导出时间范围",
                Dock = DockStyle.Top,
                Height = 120,
                Margin = new Padding(0, 0, 0, 16)
            };

            var timeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(12)
            };
            timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            timeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // 范围类型选择
            var lblRangeType = new Label
            {
                Text = "范围类型：",
                AutoSize = true,
                Margin = new Padding(0, 4, 8, 4)
            };

            var cmbRangeType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Top
            };
            cmbRangeType.Items.AddRange(new object[] { "当前月", "当前周", "自定义范围" });
            cmbRangeType.SelectedIndex = 0;
            cmbRangeType.Name = "cmbRangeType";

            timeLayout.Controls.Add(lblRangeType, 0, 0);
            timeLayout.Controls.Add(cmbRangeType, 1, 0);

            // 开始日期
            var lblStart = new Label
            {
                Text = "开始日期：",
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 4)
            };

            var dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Dock = DockStyle.Top,
                Enabled = false,
                Name = "dtpStart"
            };
            dtpStart.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            timeLayout.Controls.Add(lblStart, 0, 1);
            timeLayout.Controls.Add(dtpStart, 1, 1);

            // 结束日期
            var lblEnd = new Label
            {
                Text = "结束日期：",
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 4)
            };

            var dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Dock = DockStyle.Top,
                Enabled = false,
                Name = "dtpEnd"
            };
            dtpEnd.Value = DateTime.Today;

            timeLayout.Controls.Add(lblEnd, 0, 2);
            timeLayout.Controls.Add(dtpEnd, 1, 2);

            timeGroup.Controls.Add(timeLayout);
            rootLayout.Controls.Add(timeGroup, 0, 0);

            // 格式选择组
            var formatGroup = new GroupBox
            {
                Text = "导出格式",
                Dock = DockStyle.Top,
                Height = 80,
                Margin = new Padding(0, 0, 0, 16)
            };

            var formatLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                FlowDirection = FlowDirection.LeftToRight
            };

            var rbExcel = new RadioButton
            {
                Text = "Excel (.xlsx)",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(0, 0, 24, 0),
                Name = "rbExcel"
            };

            var rbPdf = new RadioButton
            {
                Text = "PDF (.pdf)",
                AutoSize = true,
                Margin = new Padding(0, 0, 24, 0),
                Name = "rbPdf"
            };

            var rbWord = new RadioButton
            {
                Text = "Word (.docx)",
                AutoSize = true,
                Name = "rbWord"
            };

            formatLayout.Controls.Add(rbExcel);
            formatLayout.Controls.Add(rbPdf);
            formatLayout.Controls.Add(rbWord);
            formatGroup.Controls.Add(formatLayout);
            rootLayout.Controls.Add(formatGroup, 0, 1);

            // 说明标签
            var lblInfo = new Label
            {
                Text = "导出将包含选定时间范围内的所有工作日志记录。\n导出的文件将保存到 Data 目录。",
                Dock = DockStyle.Fill,
                ForeColor = FluentColors.Gray130,
                Margin = new Padding(0, 8, 0, 16)
            };
            rootLayout.Controls.Add(lblInfo, 0, 2);

            // 按钮区域
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40,
                Margin = new Padding(0, 8, 0, 0)
            };

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Size = new Size(90, 32),
                Margin = new Padding(8, 0, 0, 0)
            };

            var btnExport = new Button
            {
                Text = "导出",
                DialogResult = DialogResult.OK,
                Size = new Size(90, 32),
                Margin = new Padding(8, 0, 0, 0),
                Name = "btnExport"
            };

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnExport);
            rootLayout.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(rootLayout);

            // 设置 Accept/Cancel 按钮
            this.AcceptButton = btnExport;
            this.CancelButton = btnCancel;
        }

        private void InitializeFluentStyle()
        {
            // 应用 Fluent 主题
            this.BackColor = FluentColors.Gray10;
            this.Font = FluentTypography.Body;

            // 样式化控件
            foreach (Control ctrl in this.GetAllControls())
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    if (btn.Name == "btnExport")
                    {
                        btn.BackColor = FluentColors.Primary;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderSize = 0;
                    }
                    else
                    {
                        btn.BackColor = FluentColors.Gray20;
                        btn.ForeColor = FluentColors.Gray160;
                        btn.FlatAppearance.BorderSize = 0;
                    }
                }
                else if (ctrl is GroupBox gb)
                {
                    gb.BackColor = FluentColors.Gray10;
                    gb.ForeColor = FluentColors.Gray160;
                }
                else if (ctrl is RadioButton rb)
                {
                    rb.BackColor = FluentColors.Gray10;
                    rb.ForeColor = FluentColors.Gray160;
                }
                else if (ctrl is DateTimePicker dtp)
                {
                    dtp.BackColor = FluentColors.White;
                    dtp.ForeColor = FluentColors.Gray160;
                }
                else if (ctrl is ComboBox cb)
                {
                    cb.BackColor = FluentColors.White;
                    cb.ForeColor = FluentColors.Gray160;
                    cb.FlatStyle = FlatStyle.Flat;
                }
            }
        }

        private void SetupEventHandlers()
        {
            // 范围类型改变事件
            var cmbRangeType = this.FindControl<ComboBox>("cmbRangeType");
            var dtpStart = this.FindControl<DateTimePicker>("dtpStart");
            var dtpEnd = this.FindControl<DateTimePicker>("dtpEnd");

            if (cmbRangeType != null)
            {
                cmbRangeType.SelectedIndexChanged += (s, e) =>
                {
                    bool isCustom = cmbRangeType.SelectedIndex == 2;
                    dtpStart.Enabled = isCustom;
                    dtpEnd.Enabled = isCustom;

                    if (!isCustom)
                    {
                        // 自动设置日期范围
                        if (cmbRangeType.SelectedIndex == 0) // 当前月
                        {
                            var now = DateTime.Today;
                            dtpStart.Value = new DateTime(now.Year, now.Month, 1);
                            dtpEnd.Value = dtpStart.Value.AddMonths(1).AddDays(-1);
                        }
                        else if (cmbRangeType.SelectedIndex == 1) // 当前周
                        {
                            var now = DateTime.Today;
                            int diff = (int)now.DayOfWeek - (int)DayOfWeek.Monday;
                            if (diff < 0) diff += 7;
                            dtpStart.Value = now.AddDays(-diff);
                            dtpEnd.Value = dtpStart.Value.AddDays(6);
                        }
                    }
                };

                // 触发一次初始化
                cmbRangeType.SelectedIndexChanged += null;
            }

            // 导出按钮点击事件
            var btnExport = this.FindControl<Button>("btnExport");
            if (btnExport != null)
            {
                btnExport.Click += OnExportClick;
            }
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            var dtpStart = this.FindControl<DateTimePicker>("dtpStart");
            var dtpEnd = this.FindControl<DateTimePicker>("dtpEnd");

            DateTime startDate = dtpStart?.Value ?? DateTime.Today;
            DateTime endDate = dtpEnd?.Value ?? DateTime.Today;

            if (startDate > endDate)
            {
                MessageBox.Show(this, "开始日期不能晚于结束日期。", "输入错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // 获取选中的导出格式
            ExportFormat format = ExportFormat.Excel;
            if (this.FindControl<RadioButton>("rbPdf")?.Checked == true)
                format = ExportFormat.Pdf;
            else if (this.FindControl<RadioButton>("rbWord")?.Checked == true)
                format = ExportFormat.Word;

            // 执行导出
            try
            {
                string filePath = PerformExport(startDate, endDate, format);

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    var result = MessageBox.Show(this, 
                        $"导出成功！\n文件路径：{filePath}\n\n是否打开文件？", 
                        "导出完成", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                }
                else
                {
                    MessageBox.Show(this, "导出完成，但文件未找到。", "警告", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"导出失败：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        private string PerformExport(DateTime startDate, DateTime endDate, ExportFormat format)
        {
            // 筛选指定日期范围内的日志 - 使用 LogDate 而不是 StartTime
            var filteredLogs = _allLogs
                .Where(l => l.LogDate.Date >= startDate.Date && l.LogDate.Date <= endDate.Date)
                .Select(l => new WorkLog
                {
                    LogDate = l.LogDate,
                    Items = l.Items.ToList()
                })
                .Where(l => l.Items.Count > 0)
                .ToList();

            if (filteredLogs.Count == 0)
            {
                throw new Exception("选定时间范围内没有工作日志记录。");
            }

            // 获取导出月份（取第一个日志的月份）
            var exportMonth = new DateTime(filteredLogs.First().LogDate.Year, filteredLogs.First().LogDate.Month, 1);
            string fileName = $"工作日志_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            string filePath;

            switch (format)
            {
                case ExportFormat.Excel:
                    filePath = Path.Combine(_dataDir, fileName + ".xlsx");
                    // 使用现有的导入导出服务 - 按月导出
                    foreach (var log in filteredLogs)
                    {
                        var month = new DateTime(log.LogDate.Year, log.LogDate.Month, 1);
                        _importExportService.RewriteMonth(month, filteredLogs, _dataDir);
                    }
                    break;

                case ExportFormat.Pdf:
                    filePath = Path.Combine(_dataDir, fileName + ".pdf");
                    _pdfExportService.ExportMonthToPdf(exportMonth, filteredLogs, filePath);
                    break;

                case ExportFormat.Word:
                    filePath = Path.Combine(_dataDir, fileName + ".docx");
                    _wordExportService.ExportMonthToWord(exportMonth, filteredLogs, filePath);
                    break;

                default:
                    throw new ArgumentException("不支持的导出格式。");
            }

            return filePath;
        }

        // 辅助方法：查找控件
        private T FindControl<T>(string name) where T : Control
        {
            foreach (Control ctrl in this.GetAllControls())
            {
                if (ctrl.Name == name && ctrl is T)
                    return (T)ctrl;
            }
            return null;
        }

        // 辅助方法：获取所有控件
        private IEnumerable<Control> GetAllControls()
        {
            var queue = new Queue<Control>();
            foreach (Control ctrl in this.Controls)
                queue.Enqueue(ctrl);

            while (queue.Count > 0)
            {
                var ctrl = queue.Dequeue();
                yield return ctrl;
                foreach (Control child in ctrl.Controls)
                    queue.Enqueue(child);
            }
        }
    }
}
