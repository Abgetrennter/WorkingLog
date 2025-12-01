using System.ComponentModel;
using System.Windows.Forms;
using WorkLogApp.UI.Controls;

namespace WorkLogApp.UI.Forms
{
    partial class ItemCreateForm
    {
        private IContainer components = null;

        private Label lblCategory;
        private CategoryTreeComboBox _categoryCombo;
        private Label lblTitle;
        private TextBox _titleBox;
        private Label lblDate;
        private DateTimePicker _datePicker;
        private Label lblStatus;
        private ComboBox _statusCombo;
        private Label lblStart;
        private DateTimePicker _startPicker;
        private Label lblEnd;
        private DateTimePicker _endPicker;
        private Label lblTags;
        private TextBox _tagsBox;
        private DynamicFormPanel _formPanel;
        private Button _btnGenerateSave;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel headerLayout;
        private System.Windows.Forms.FlowLayoutPanel bottomBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this._titleBox = new System.Windows.Forms.TextBox();
            this._btnGenerateSave = new System.Windows.Forms.Button();
            this._categoryCombo = new WorkLogApp.UI.Controls.CategoryTreeComboBox();
            this._formPanel = new WorkLogApp.UI.Controls.DynamicFormPanel();
            this.lblDate = new System.Windows.Forms.Label();
            this._datePicker = new System.Windows.Forms.DateTimePicker();
            this.lblStatus = new System.Windows.Forms.Label();
            this._statusCombo = new System.Windows.Forms.ComboBox();
            this.lblStart = new System.Windows.Forms.Label();
            this._startPicker = new System.Windows.Forms.DateTimePicker();
            this.lblEnd = new System.Windows.Forms.Label();
            this._endPicker = new System.Windows.Forms.DateTimePicker();
            this.lblTags = new System.Windows.Forms.Label();
            this._tagsBox = new System.Windows.Forms.TextBox();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerLayout = new System.Windows.Forms.TableLayoutPanel();
            this.bottomBar = new System.Windows.Forms.FlowLayoutPanel();
            this.rootLayout.SuspendLayout();
            this.headerLayout.SuspendLayout();
            this.bottomBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 0);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(62, 18);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "分类：";
            this.lblCategory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCategory.Click += new System.EventHandler(this.lblCategory_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(413, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(62, 18);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "标题：";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _titleBox
            // 
            this._titleBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._titleBox.Location = new System.Drawing.Point(517, 3);
            this._titleBox.Name = "_titleBox";
            this._titleBox.Size = new System.Drawing.Size(297, 28);
            this._titleBox.TabIndex = 3;
            // 
            // _btnGenerateSave
            // 
            this._btnGenerateSave.Location = new System.Drawing.Point(621, 3);
            this._btnGenerateSave.Name = "_btnGenerateSave";
            this._btnGenerateSave.Size = new System.Drawing.Size(197, 60);
            this._btnGenerateSave.TabIndex = 5;
            this._btnGenerateSave.Text = "生成并保存";
            this._btnGenerateSave.Click += new System.EventHandler(this.OnGenerateAndSave);
            // 
            // _categoryCombo
            // 
            this._categoryCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._categoryCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._categoryCombo.Location = new System.Drawing.Point(107, 3);
            this._categoryCombo.Name = "_categoryCombo";
            this._categoryCombo.Size = new System.Drawing.Size(200, 26);
            this._categoryCombo.TabIndex = 1;
            this._categoryCombo.TemplateService = null;
            // 
            // _formPanel
            // 
            this._formPanel.AutoScroll = true;
            this._formPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._formPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._formPanel.Location = new System.Drawing.Point(11, 157);
            this._formPanel.Name = "_formPanel";
            this._formPanel.Size = new System.Drawing.Size(821, 419);
            this._formPanel.TabIndex = 4;
            this._formPanel.Paint += new System.Windows.Forms.PaintEventHandler(this._formPanel_Paint);
            // 
            // lblDate
            // 
            this.lblDate.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(3, 34);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(98, 18);
            this.lblDate.TabIndex = 4;
            this.lblDate.Text = "发生日期：";
            this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _datePicker
            // 
            this._datePicker.CustomFormat = "yyyy-MM-dd";
            this._datePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._datePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._datePicker.Location = new System.Drawing.Point(107, 37);
            this._datePicker.Name = "_datePicker";
            this._datePicker.Size = new System.Drawing.Size(300, 28);
            this._datePicker.TabIndex = 5;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(413, 34);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(62, 18);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "状态：";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _statusCombo
            // 
            this._statusCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._statusCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._statusCombo.Location = new System.Drawing.Point(517, 37);
            this._statusCombo.Name = "_statusCombo";
            this._statusCombo.Size = new System.Drawing.Size(200, 26);
            this._statusCombo.TabIndex = 7;
            // 
            // lblStart
            // 
            this.lblStart.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStart.AutoSize = true;
            this.lblStart.Location = new System.Drawing.Point(3, 68);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(98, 18);
            this.lblStart.TabIndex = 8;
            this.lblStart.Text = "开始时间：";
            this.lblStart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _startPicker
            // 
            this._startPicker.Checked = false;
            this._startPicker.CustomFormat = "HH:mm";
            this._startPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._startPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._startPicker.Location = new System.Drawing.Point(107, 71);
            this._startPicker.Name = "_startPicker";
            this._startPicker.ShowCheckBox = true;
            this._startPicker.ShowUpDown = true;
            this._startPicker.Size = new System.Drawing.Size(300, 28);
            this._startPicker.TabIndex = 9;
            // 
            // lblEnd
            // 
            this.lblEnd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEnd.AutoSize = true;
            this.lblEnd.Location = new System.Drawing.Point(413, 68);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(98, 18);
            this.lblEnd.TabIndex = 10;
            this.lblEnd.Text = "结束时间：";
            this.lblEnd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _endPicker
            // 
            this._endPicker.Checked = false;
            this._endPicker.CustomFormat = "HH:mm";
            this._endPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._endPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._endPicker.Location = new System.Drawing.Point(517, 71);
            this._endPicker.Name = "_endPicker";
            this._endPicker.ShowCheckBox = true;
            this._endPicker.ShowUpDown = true;
            this._endPicker.Size = new System.Drawing.Size(301, 28);
            this._endPicker.TabIndex = 11;
            // 
            // lblTags
            // 
            this.lblTags.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(3, 102);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(62, 18);
            this.lblTags.TabIndex = 12;
            this.lblTags.Text = "标签：";
            this.lblTags.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _tagsBox
            // 
            this.headerLayout.SetColumnSpan(this._tagsBox, 3);
            this._tagsBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tagsBox.Location = new System.Drawing.Point(107, 105);
            this._tagsBox.Name = "_tagsBox";
            this._tagsBox.Size = new System.Drawing.Size(711, 28);
            this._tagsBox.TabIndex = 13;
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.headerLayout, 0, 0);
            this.rootLayout.Controls.Add(this._formPanel, 0, 1);
            this.rootLayout.Controls.Add(this.bottomBar, 0, 2);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(8);
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.Size = new System.Drawing.Size(843, 659);
            this.rootLayout.TabIndex = 0;
            // 
            // headerLayout
            // 
            this.headerLayout.AutoSize = true;
            this.headerLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.headerLayout.ColumnCount = 4;
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.headerLayout.Controls.Add(this._categoryCombo, 1, 0);
            this.headerLayout.Controls.Add(this.lblTitle, 2, 0);
            this.headerLayout.Controls.Add(this._titleBox, 3, 0);
            this.headerLayout.Controls.Add(this._datePicker, 1, 1);
            this.headerLayout.Controls.Add(this.lblStatus, 2, 1);
            this.headerLayout.Controls.Add(this._statusCombo, 3, 1);
            this.headerLayout.Controls.Add(this.lblStart, 0, 2);
            this.headerLayout.Controls.Add(this._startPicker, 1, 2);
            this.headerLayout.Controls.Add(this.lblEnd, 2, 2);
            this.headerLayout.Controls.Add(this._endPicker, 3, 2);
            this.headerLayout.Controls.Add(this.lblTags, 0, 3);
            this.headerLayout.Controls.Add(this._tagsBox, 1, 3);
            this.headerLayout.Controls.Add(this.lblCategory, 0, 0);
            this.headerLayout.Controls.Add(this.lblDate, 0, 1);
            this.headerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerLayout.Location = new System.Drawing.Point(11, 11);
            this.headerLayout.Name = "headerLayout";
            this.headerLayout.RowCount = 4;
            this.headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.headerLayout.Size = new System.Drawing.Size(821, 140);
            this.headerLayout.TabIndex = 0;
            // 
            // bottomBar
            // 
            this.bottomBar.AutoSize = true;
            this.bottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bottomBar.Controls.Add(this._btnGenerateSave);
            this.bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.bottomBar.Location = new System.Drawing.Point(11, 582);
            this.bottomBar.Name = "bottomBar";
            this.bottomBar.Size = new System.Drawing.Size(821, 66);
            this.bottomBar.TabIndex = 5;
            // 
            // ItemCreateForm
            // 
            this.ClientSize = new System.Drawing.Size(843, 659);
            this.Controls.Add(this.rootLayout);
            this.Name = "ItemCreateForm";
            this.Text = "创建日志事项";
            this.rootLayout.ResumeLayout(false);
            this.rootLayout.PerformLayout();
            this.headerLayout.ResumeLayout(false);
            this.headerLayout.PerformLayout();
            this.bottomBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}