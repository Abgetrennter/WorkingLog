using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Services.Implementations;

namespace WorkLogApp.UI.Forms
{
    public class MainForm : Form
    {
        private readonly ITemplateService _templateService;
        private readonly ListView _listView;
        private readonly DateTimePicker _monthPicker;
        private readonly Button _btnCreate;
        private readonly Button _btnImport;

        public MainForm(ITemplateService templateService)
        {
            _templateService = templateService;
            Text = "工作日志 - 主界面";
            Width = 1000;
            Height = 700;

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8, 8, 8, 4) };

            _btnCreate = new Button { Text = "创建事项", Width = 100, Height = 32, Location = new Point(8, 8) };
            _btnCreate.Click += OnCreateItemClick;

            _monthPicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM", ShowUpDown = true, Width = 100, Height = 28, Location = new Point(120, 10), Value = DateTime.Today };

            _btnImport = new Button { Text = "导入当月 Excel", Width = 140, Height = 32, Location = new Point(230, 8) };
            _btnImport.Click += OnImportMonthClick;

            topPanel.Controls.Add(_btnCreate);
            topPanel.Controls.Add(_monthPicker);
            topPanel.Controls.Add(_btnImport);

            _listView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            _listView.Columns.Add("日期", 100);
            _listView.Columns.Add("标题", 250);
            _listView.Columns.Add("状态", 80);
            _listView.Columns.Add("标签", 120);
            _listView.Columns.Add("开始", 120);
            _listView.Columns.Add("结束", 120);

            Controls.Add(_listView);
            Controls.Add(topPanel);
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
                var lv = new ListViewItem(new[]
                {
                    it.LogDate.ToString("yyyy-MM-dd"),
                    it.ItemTitle ?? string.Empty,
                    it.Status.ToString(),
                    it.Tags ?? string.Empty,
                    it.StartTime.HasValue ? it.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                    it.EndTime.HasValue ? it.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty
                });
                _listView.Items.Add(lv);
            }
            _listView.EndUpdate();
        }
    }
}