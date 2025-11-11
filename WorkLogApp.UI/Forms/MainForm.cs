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
        private readonly Button _btnMerge;
        private readonly Button _btnCategoryManage;
        private readonly Button _btnImportWizard;

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

            _btnImport = new Button { Text = "刷新当月", Width = 100, Height = 32, Location = new Point(230, 8) };
            _btnImport.Click += OnImportMonthClick;

            _btnMerge = new Button { Text = "合并其他日志", Width = 120, Height = 32, Location = new Point(340, 8) };
            _btnMerge.Click += OnMergeOtherClick;

            _btnCategoryManage = new Button { Text = "分类管理", Width = 100, Height = 32, Location = new Point(470, 8) };
            _btnCategoryManage.Click += OnCategoryManageClick;

            _btnImportWizard = new Button { Text = "导入向导", Width = 100, Height = 32, Location = new Point(580, 8) };
            _btnImportWizard.Click += OnImportWizardClick;

            topPanel.Controls.Add(_btnCreate);
            topPanel.Controls.Add(_monthPicker);
            topPanel.Controls.Add(_btnImport);
            topPanel.Controls.Add(_btnMerge);
            topPanel.Controls.Add(_btnCategoryManage);
            topPanel.Controls.Add(_btnImportWizard);

            _listView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            _listView.Columns.Add("日期", 100);
            _listView.Columns.Add("标题", 200);
            _listView.Columns.Add("内容", 400);
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
    }
}