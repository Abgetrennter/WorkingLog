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

        private WorkLogApp.UI.Controls.FluentListView _listView;
        private DateTimePicker _monthPicker;
        private Button _btnCreate;
        private Button _btnTodo;
        private Button _btnImport;
        private Button _btnCategoryManage;
        private Button _btnImportWizard;
        private WorkLogApp.UI.Controls.FluentToolBar _toolBar;
        private ColumnHeader _colDate;
        private ColumnHeader _colTitle;
        private ColumnHeader _colStatus;
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
            this._toolBar = new WorkLogApp.UI.Controls.FluentToolBar();
            this._btnCreate = new System.Windows.Forms.Button();
            this._btnTodo = new System.Windows.Forms.Button();
            this._dayPicker = new System.Windows.Forms.DateTimePicker();
            this._chkShowByMonth = new System.Windows.Forms.CheckBox();
            this._monthPicker = new System.Windows.Forms.DateTimePicker();
            this._btnCategoryManage = new System.Windows.Forms.Button();
            this._btnImportWizard = new System.Windows.Forms.Button();
            this._btnImport = new System.Windows.Forms.Button();
            this._btnDailySummary = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this._btnOpenFileLocation = new System.Windows.Forms.Button();
            this._listView = new WorkLogApp.UI.Controls.FluentListView();
            this._topSeparator = new System.Windows.Forms.Panel();
            this._colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colContent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTags = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colStart = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colEnd = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.rootLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnCreate
            // 
            this._btnCreate.Location = new System.Drawing.Point(0, 0);
            this._btnCreate.Name = "_btnCreate";
            this._btnCreate.Size = new System.Drawing.Size(75, 23);
            this._btnCreate.TabIndex = 0;
            this._btnCreate.Text = "创建事项";
            this._btnCreate.UseVisualStyleBackColor = true;
            this._btnCreate.Click += new System.EventHandler(this.OnCreateItemClick);
            // 
            // _btnTodo
            // 
            this._btnTodo.Location = new System.Drawing.Point(0, 0);
            this._btnTodo.Name = "_btnTodo";
            this._btnTodo.Size = new System.Drawing.Size(75, 23);
            this._btnTodo.TabIndex = 0;
            this._btnTodo.Text = "待办事项";
            this._btnTodo.UseVisualStyleBackColor = true;
            this._btnTodo.Click += new System.EventHandler(this.OnTodoClick);
            // 
            // _dayPicker
            // 
            this._dayPicker.CustomFormat = "yyyy-MM-dd";
            this._dayPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._dayPicker.Location = new System.Drawing.Point(0, 0);
            this._dayPicker.Name = "_dayPicker";
            this._dayPicker.Size = new System.Drawing.Size(154, 28);
            this._dayPicker.TabIndex = 0;
            // 
            // _chkShowByMonth
            // 
            this._chkShowByMonth.AutoSize = true;
            this._chkShowByMonth.Location = new System.Drawing.Point(0, 0);
            this._chkShowByMonth.Name = "_chkShowByMonth";
            this._chkShowByMonth.Size = new System.Drawing.Size(106, 22);
            this._chkShowByMonth.TabIndex = 0;
            this._chkShowByMonth.Text = "按月显示";
            this._chkShowByMonth.UseVisualStyleBackColor = true;
            // 
            // _monthPicker
            // 
            this._monthPicker.CustomFormat = "yyyy-MM";
            this._monthPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._monthPicker.Location = new System.Drawing.Point(0, 0);
            this._monthPicker.Name = "_monthPicker";
            this._monthPicker.ShowUpDown = true;
            this._monthPicker.Size = new System.Drawing.Size(154, 28);
            this._monthPicker.TabIndex = 0;
            this._monthPicker.Value = new System.DateTime(2025, 11, 11, 0, 0, 0, 0);
            // 
            // _btnCategoryManage
            // 
            this._btnCategoryManage.Location = new System.Drawing.Point(0, 0);
            this._btnCategoryManage.Name = "_btnCategoryManage";
            this._btnCategoryManage.Size = new System.Drawing.Size(75, 23);
            this._btnCategoryManage.TabIndex = 0;
            this._btnCategoryManage.Text = "分类管理";
            this._btnCategoryManage.UseVisualStyleBackColor = true;
            this._btnCategoryManage.Click += new System.EventHandler(this.OnCategoryManageClick);
            // 
            // _btnImportWizard
            // 
            this._btnImportWizard.Location = new System.Drawing.Point(0, 0);
            this._btnImportWizard.Name = "_btnImportWizard";
            this._btnImportWizard.Size = new System.Drawing.Size(75, 23);
            this._btnImportWizard.TabIndex = 0;
            this._btnImportWizard.Text = "导入日志";
            this._btnImportWizard.UseVisualStyleBackColor = true;
            this._btnImportWizard.Click += new System.EventHandler(this.OnImportWizardClick);
            // 
            // _btnDailySummary
            // 
            this._btnDailySummary.Location = new System.Drawing.Point(0, 0);
            this._btnDailySummary.Name = "_btnDailySummary";
            this._btnDailySummary.Size = new System.Drawing.Size(75, 23);
            this._btnDailySummary.TabIndex = 0;
            this._btnDailySummary.Text = "每日总结";
            this._btnDailySummary.UseVisualStyleBackColor = true;
            this._btnDailySummary.Click += new System.EventHandler(this.OnDailySummaryClick);
            // 
            // _btnImport
            // 
            this._btnImport.Location = new System.Drawing.Point(0, 0);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(75, 23);
            this._btnImport.TabIndex = 0;
            this._btnImport.Text = "刷新";
            this._btnImport.UseVisualStyleBackColor = true;
            this._btnImport.Click += new System.EventHandler(this.OnImportMonthClick);
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(0, 0);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(75, 23);
            this._btnSave.TabIndex = 0;
            this._btnSave.Text = "保存";
            this._btnSave.UseVisualStyleBackColor = true;
            this._btnSave.Click += new System.EventHandler(this.OnSaveClick);
            // 
            // _btnOpenFileLocation
            // 
            this._btnOpenFileLocation.Location = new System.Drawing.Point(0, 0);
            this._btnOpenFileLocation.Name = "_btnOpenFileLocation";
            this._btnOpenFileLocation.Size = new System.Drawing.Size(75, 23);
            this._btnOpenFileLocation.TabIndex = 0;
            this._btnOpenFileLocation.Text = "打开EXCEL";
            this._btnOpenFileLocation.UseVisualStyleBackColor = true;
            this._btnOpenFileLocation.Click += new System.EventHandler(this.OnOpenFileLocationClick);
            // 
            // _listView
            // 
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colDate,
            this._colTitle,
            this._colStatus,
            this._colContent,
            this._colTags,
            this._colStart,
            this._colEnd});
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.FullRowSelect = true;
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
            this._topSeparator.Height = 1;
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
            // _colStatus
            // 
            this._colStatus.Text = "状态";
            this._colStatus.Width = 100;
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
            this.rootLayout.Controls.Add(this._toolBar, 0, 0);
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
            // _toolBar
            // 
            this._toolBar.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolBar.Location = new System.Drawing.Point(0, 0);
            this._toolBar.Name = "_toolBar";
            this._toolBar.Size = new System.Drawing.Size(1000, 56);
            this._toolBar.TabIndex = 0;
            //
            // MainForm
            //
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.rootLayout);
            this.Name = "MainForm";
            this.Text = "工作日志 - 主界面";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.rootLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
