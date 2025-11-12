using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private ListView _listView;
        private DateTimePicker _monthPicker;
        private Button _btnCreate;
        private Button _btnImport;
        private Button _btnCategoryManage;
        private Button _btnImportWizard;
        private FlowLayoutPanel topPanel;
        private ColumnHeader _colDate;
        private ColumnHeader _colTitle;
        private ColumnHeader _colContent;
        private ColumnHeader _colTags;
        private ColumnHeader _colStart;
        private ColumnHeader _colEnd;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private CheckBox _chkShowByMonth;
        private DateTimePicker _dayPicker;
        private Button _btnDailySummary;
        private Button _btnSave;
        private Button _btnOpenFileLocation;
        private Panel _topSeparator;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeComponent()
        {
            this.topPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnCreate = new System.Windows.Forms.Button();
            this._dayPicker = new System.Windows.Forms.DateTimePicker();
            this._chkShowByMonth = new System.Windows.Forms.CheckBox();
            this._monthPicker = new System.Windows.Forms.DateTimePicker();
            this._btnCategoryManage = new System.Windows.Forms.Button();
            this._btnImportWizard = new System.Windows.Forms.Button();
            this._btnImport = new System.Windows.Forms.Button();
            this._btnDailySummary = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this._btnOpenFileLocation = new System.Windows.Forms.Button();
            this._listView = new System.Windows.Forms.ListView();
            this._topSeparator = new System.Windows.Forms.Panel();
            this._colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colContent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTags = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colStart = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colEnd = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.topPanel.SuspendLayout();
            this.rootLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.AutoScroll = true;
            this.topPanel.AutoSize = false;
            this.topPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.topPanel.Controls.Add(this._btnCreate);
            this.topPanel.Controls.Add(this._chkShowByMonth);
            this.topPanel.Controls.Add(this._dayPicker);
            this.topPanel.Controls.Add(this._monthPicker);
            this.topPanel.Controls.Add(this._btnCategoryManage);
            this.topPanel.Controls.Add(this._btnImportWizard);
            this.topPanel.Controls.Add(this._btnDailySummary);
            this.topPanel.Controls.Add(this._btnImport);
            this.topPanel.Controls.Add(this._btnSave);
            this.topPanel.Controls.Add(this._btnOpenFileLocation);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topPanel.Location = new System.Drawing.Point(3, 3);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(6);
            this.topPanel.Size = new System.Drawing.Size(994, 68);
            this.topPanel.TabIndex = 1;
            this.topPanel.WrapContents = false;
            // 
            // _btnCreate
            // 
            this._btnCreate.Location = new System.Drawing.Point(10, 12);
            this._btnCreate.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnCreate.Name = "_btnCreate";
            this._btnCreate.Size = new System.Drawing.Size(110, 36);
            this._btnCreate.TabIndex = 0;
            this._btnCreate.Tag = "compact";
            this._btnCreate.Text = "创建事项";
            this._btnCreate.Click += new System.EventHandler(this.OnCreateItemClick);
            // 
            // _dayPicker
            // 
            this._dayPicker.CustomFormat = "yyyy-MM-dd";
            this._dayPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._dayPicker.Location = new System.Drawing.Point(286, 12);
            this._dayPicker.Margin = new System.Windows.Forms.Padding(8, 6, 4, 6);
            this._dayPicker.Name = "_dayPicker";
            this._dayPicker.Size = new System.Drawing.Size(154, 28);
            this._dayPicker.TabIndex = 6;
            this._dayPicker.Tag = "compact";
            // 
            // _chkShowByMonth
            // 
            this._chkShowByMonth.AutoSize = true;
            this._chkShowByMonth.Location = new System.Drawing.Point(168, 16);
            this._chkShowByMonth.Margin = new System.Windows.Forms.Padding(4, 10, 4, 6);
            this._chkShowByMonth.Name = "_chkShowByMonth";
            this._chkShowByMonth.Size = new System.Drawing.Size(106, 22);
            this._chkShowByMonth.TabIndex = 7;
            this._chkShowByMonth.Tag = "compact";
            this._chkShowByMonth.Text = "按月显示";
            // 
            // _monthPicker
            // 
            this._monthPicker.CustomFormat = "yyyy-MM";
            this._monthPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._monthPicker.Location = new System.Drawing.Point(452, 12);
            this._monthPicker.Margin = new System.Windows.Forms.Padding(8, 6, 4, 6);
            this._monthPicker.Name = "_monthPicker";
            this._monthPicker.ShowUpDown = true;
            this._monthPicker.Size = new System.Drawing.Size(154, 28);
            this._monthPicker.TabIndex = 1;
            this._monthPicker.Tag = "compact";
            this._monthPicker.Value = new System.DateTime(2025, 11, 11, 0, 0, 0, 0);
            this._monthPicker.ValueChanged += new System.EventHandler(this._monthPicker_ValueChanged);
            // 
            // _btnCategoryManage
            // 
            this._btnCategoryManage.Location = new System.Drawing.Point(614, 12);
            this._btnCategoryManage.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnCategoryManage.Name = "_btnCategoryManage";
            this._btnCategoryManage.Size = new System.Drawing.Size(110, 36);
            this._btnCategoryManage.TabIndex = 4;
            this._btnCategoryManage.Tag = "compact";
            this._btnCategoryManage.Text = "分类管理";
            this._btnCategoryManage.Click += new System.EventHandler(this.OnCategoryManageClick);
            // 
            // _btnImportWizard
            // 
            this._btnImportWizard.Location = new System.Drawing.Point(772, 12);
            this._btnImportWizard.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnImportWizard.Name = "_btnImportWizard";
            this._btnImportWizard.Size = new System.Drawing.Size(110, 36);
            this._btnImportWizard.TabIndex = 5;
            this._btnImportWizard.Tag = "compact";
            this._btnImportWizard.Text = "导入向导";
            this._btnImportWizard.Click += new System.EventHandler(this.OnImportWizardClick);
            // 
            // _btnImport
            // 
            this._btnImport.Location = new System.Drawing.Point(1088, 12);
            this._btnImport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(110, 36);
            this._btnImport.TabIndex = 2;
            this._btnImport.Tag = "compact";
            this._btnImport.Text = "刷新";
            this._btnImport.Click += new System.EventHandler(this.OnImportMonthClick);
            // 
            // _btnDailySummary
            // 
            this._btnDailySummary.Location = new System.Drawing.Point(930, 12);
            this._btnDailySummary.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnDailySummary.Name = "_btnDailySummary";
            this._btnDailySummary.Size = new System.Drawing.Size(110, 36);
            this._btnDailySummary.TabIndex = 8;
            this._btnDailySummary.Tag = "compact";
            this._btnDailySummary.Text = "每日总结";
            this._btnDailySummary.Click += new System.EventHandler(this.OnDailySummaryClick);
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(1246, 12);
            this._btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(110, 36);
            this._btnSave.TabIndex = 9;
            this._btnSave.Tag = "compact";
            this._btnSave.Text = "保存";
            this._btnSave.Click += new System.EventHandler(this.OnSaveClick);
            // 
            // _btnOpenFileLocation
            // 
            this._btnOpenFileLocation.Location = new System.Drawing.Point(1404, 12);
            this._btnOpenFileLocation.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this._btnOpenFileLocation.Name = "_btnOpenFileLocation";
            this._btnOpenFileLocation.Size = new System.Drawing.Size(130, 36);
            this._btnOpenFileLocation.TabIndex = 10;
            this._btnOpenFileLocation.Tag = "compact";
            this._btnOpenFileLocation.Text = "打开EXCEL";
            this._btnOpenFileLocation.Click += new System.EventHandler(this.OnOpenFileLocationClick);
            // 
            // _listView
            // 
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colDate,
            this._colTitle,
            this._colContent,
            this._colTags,
            this._colStart,
            this._colEnd});
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.FullRowSelect = true;
            this._listView.GridLines = true;
            this._listView.HideSelection = false;
            this._listView.Location = new System.Drawing.Point(3, 77);
            this._listView.Name = "_listView";
            this._listView.Size = new System.Drawing.Size(994, 620);
            this._listView.TabIndex = 0;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View = System.Windows.Forms.View.Details;
            // 
            // _topSeparator
            // 
            this._topSeparator.Dock = System.Windows.Forms.DockStyle.Fill;
            this._topSeparator.Margin = new System.Windows.Forms.Padding(0);
            this._topSeparator.Name = "_topSeparator";
            this._topSeparator.BackColor = System.Drawing.Color.FromArgb(235, 240, 246);
            // 
            // _colDate
            // 
            this._colDate.Text = "日期";
            this._colDate.Width = 120;
            // 
            // _colTitle
            // 
            this._colTitle.Text = "标题";
            this._colTitle.Width = 200;
            // 
            // _colContent
            // 
            this._colContent.Text = "内容";
            this._colContent.Width = 400;
            // 
            // _colTags
            // 
            this._colTags.Text = "标签";
            this._colTags.Width = 120;
            // 
            // _colStart
            // 
            this._colStart.Text = "开始";
            this._colStart.Width = 120;
            // 
            // _colEnd
            // 
            this._colEnd.Text = "结束";
            this._colEnd.Width = 120;
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.topPanel, 0, 0);
            this.rootLayout.Controls.Add(this._topSeparator, 0, 1);
            this.rootLayout.Controls.Add(this._listView, 0, 2);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(1000, 700);
            this.rootLayout.TabIndex = 0;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.rootLayout);
            this.Name = "MainForm";
            this.Text = "工作日志 - 主界面";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.rootLayout.ResumeLayout(false);
            this.rootLayout.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}