using System.ComponentModel;
using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    partial class ItemEditForm
    {
        private IContainer components = null;

        private Label lblTitle;
        private TextBox _titleBox;
        private Label lblDate;
        private DateTimePicker _datePicker;
        private Label lblStatus;
        private ComboBox _statusCombo;
        private Label lblTags;
        private TextBox _tagsBox;
        private Label lblStart;
        private DateTimePicker _startPicker;
        private Label lblEnd;
        private DateTimePicker _endPicker;
        private Label lblSort;
        private NumericUpDown _sortUpDown;
        private Label lblContent;
        private RichTextBox _contentBox;
        private Button _btnSave;
        private Button _btnCancel;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
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
            this.lblTitle = new System.Windows.Forms.Label();
            this._titleBox = new System.Windows.Forms.TextBox();
            this.lblDate = new System.Windows.Forms.Label();
            this._datePicker = new System.Windows.Forms.DateTimePicker();
            this.lblStatus = new System.Windows.Forms.Label();
            this._statusCombo = new System.Windows.Forms.ComboBox();
            this.lblTags = new System.Windows.Forms.Label();
            this._tagsBox = new System.Windows.Forms.TextBox();
            this.lblStart = new System.Windows.Forms.Label();
            this._startPicker = new System.Windows.Forms.DateTimePicker();
            this.lblEnd = new System.Windows.Forms.Label();
            this._endPicker = new System.Windows.Forms.DateTimePicker();
            this.lblSort = new System.Windows.Forms.Label();
            this._sortUpDown = new System.Windows.Forms.NumericUpDown();
            this.lblContent = new System.Windows.Forms.Label();
            this._contentBox = new System.Windows.Forms.RichTextBox();
            this._btnSave = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblProgress = new System.Windows.Forms.Label();
            this._progressUpDown = new System.Windows.Forms.NumericUpDown();
            this.bottomBar = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this._sortUpDown)).BeginInit();
            this.rootLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._progressUpDown)).BeginInit();
            this.bottomBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(11, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(62, 18);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "标题：";
            // 
            // _titleBox
            // 
            this._titleBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._titleBox.Location = new System.Drawing.Point(295, 11);
            this._titleBox.Name = "_titleBox";
            this._titleBox.Size = new System.Drawing.Size(594, 28);
            this._titleBox.TabIndex = 1;
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(11, 42);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(62, 18);
            this.lblDate.TabIndex = 2;
            this.lblDate.Text = "日期：";
            // 
            // _datePicker
            // 
            this._datePicker.CustomFormat = "yyyy-MM-dd";
            this._datePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._datePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._datePicker.Location = new System.Drawing.Point(295, 45);
            this._datePicker.Name = "_datePicker";
            this._datePicker.Size = new System.Drawing.Size(594, 28);
            this._datePicker.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(11, 76);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(62, 18);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "状态：";
            // 
            // _statusCombo
            // 
            this._statusCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._statusCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._statusCombo.Location = new System.Drawing.Point(295, 79);
            this._statusCombo.Name = "_statusCombo";
            this._statusCombo.Size = new System.Drawing.Size(594, 26);
            this._statusCombo.TabIndex = 5;
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(11, 142);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(152, 18);
            this.lblTags.TabIndex = 8;
            this.lblTags.Text = "标签(分号分隔)：";
            // 
            // _tagsBox
            // 
            this._tagsBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tagsBox.Location = new System.Drawing.Point(295, 145);
            this._tagsBox.Name = "_tagsBox";
            this._tagsBox.Size = new System.Drawing.Size(594, 28);
            this._tagsBox.TabIndex = 9;
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Location = new System.Drawing.Point(11, 176);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(98, 18);
            this.lblStart.TabIndex = 10;
            this.lblStart.Text = "开始时间：";
            // 
            // _startPicker
            // 
            this._startPicker.CustomFormat = "yyyy-MM-dd HH:mm";
            this._startPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._startPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._startPicker.Location = new System.Drawing.Point(295, 179);
            this._startPicker.Name = "_startPicker";
            this._startPicker.ShowCheckBox = true;
            this._startPicker.Size = new System.Drawing.Size(594, 28);
            this._startPicker.TabIndex = 11;
            // 
            // lblEnd
            // 
            this.lblEnd.AutoSize = true;
            this.lblEnd.Location = new System.Drawing.Point(11, 210);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(98, 18);
            this.lblEnd.TabIndex = 12;
            this.lblEnd.Text = "结束时间：";
            // 
            // _endPicker
            // 
            this._endPicker.CustomFormat = "yyyy-MM-dd HH:mm";
            this._endPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._endPicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._endPicker.Location = new System.Drawing.Point(295, 213);
            this._endPicker.Name = "_endPicker";
            this._endPicker.ShowCheckBox = true;
            this._endPicker.Size = new System.Drawing.Size(594, 28);
            this._endPicker.TabIndex = 13;
            // 
            // lblSort
            // 
            this.lblSort.AutoSize = true;
            this.lblSort.Location = new System.Drawing.Point(0, 0);
            this.lblSort.Name = "lblSort";
            this.lblSort.Size = new System.Drawing.Size(100, 23);
            this.lblSort.TabIndex = 0;
            this.lblSort.Text = "排序：";
            // 
            // _sortUpDown
            // 
            this._sortUpDown.Dock = System.Windows.Forms.DockStyle.Left;
            this._sortUpDown.Location = new System.Drawing.Point(0, 0);
            this._sortUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._sortUpDown.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this._sortUpDown.Name = "_sortUpDown";
            this._sortUpDown.Size = new System.Drawing.Size(120, 28);
            this._sortUpDown.TabIndex = 0;
            // 
            // lblContent
            // 
            this.lblContent.AutoSize = true;
            this.lblContent.Location = new System.Drawing.Point(11, 244);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(62, 18);
            this.lblContent.TabIndex = 14;
            this.lblContent.Text = "内容：";
            // 
            // _contentBox
            // 
            this._contentBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._contentBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentBox.Location = new System.Drawing.Point(295, 247);
            this._contentBox.Name = "_contentBox";
            this._contentBox.Size = new System.Drawing.Size(594, 339);
            this._contentBox.TabIndex = 15;
            this._contentBox.Text = "";
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(505, 3);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(182, 42);
            this._btnSave.TabIndex = 0;
            this._btnSave.Text = "保存";
            this._btnSave.Click += new System.EventHandler(this.OnSaveClickNew);
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(693, 3);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(182, 42);
            this._btnCancel.TabIndex = 1;
            this._btnCancel.Text = "取消";
            this._btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 2;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.lblTitle, 0, 0);
            this.rootLayout.Controls.Add(this._titleBox, 1, 0);
            this.rootLayout.Controls.Add(this.lblDate, 0, 1);
            this.rootLayout.Controls.Add(this._datePicker, 1, 1);
            this.rootLayout.Controls.Add(this.lblStatus, 0, 2);
            this.rootLayout.Controls.Add(this._statusCombo, 1, 2);
            this.rootLayout.Controls.Add(this.lblProgress, 0, 3);
            this.rootLayout.Controls.Add(this._progressUpDown, 1, 3);
            this.rootLayout.Controls.Add(this.lblTags, 0, 4);
            this.rootLayout.Controls.Add(this._tagsBox, 1, 4);
            this.rootLayout.Controls.Add(this.lblStart, 0, 5);
            this.rootLayout.Controls.Add(this._startPicker, 1, 5);
            this.rootLayout.Controls.Add(this.lblEnd, 0, 6);
            this.rootLayout.Controls.Add(this._endPicker, 1, 6);
            this.rootLayout.Controls.Add(this.lblContent, 0, 7);
            this.rootLayout.Controls.Add(this._contentBox, 1, 7);
            this.rootLayout.Controls.Add(this.bottomBar, 1, 8);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(8);
            this.rootLayout.RowCount = 9;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.rootLayout.Size = new System.Drawing.Size(900, 650);
            this.rootLayout.TabIndex = 0;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(11, 108);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(125, 18);
            this.lblProgress.TabIndex = 6;
            this.lblProgress.Text = "进度(0-100)：";
            // 
            // _progressUpDown
            // 
            this._progressUpDown.Dock = System.Windows.Forms.DockStyle.Left;
            this._progressUpDown.Location = new System.Drawing.Point(295, 111);
            this._progressUpDown.Name = "_progressUpDown";
            this._progressUpDown.Size = new System.Drawing.Size(120, 28);
            this._progressUpDown.TabIndex = 7;
            // 
            // bottomBar
            // 
            this.bottomBar.AutoSize = true;
            this.bottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rootLayout.SetColumnSpan(this.bottomBar, 2);
            this.bottomBar.Controls.Add(this._btnCancel);
            this.bottomBar.Controls.Add(this._btnSave);
            this.bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.bottomBar.Location = new System.Drawing.Point(11, 592);
            this.bottomBar.Name = "bottomBar";
            this.bottomBar.Size = new System.Drawing.Size(878, 47);
            this.bottomBar.TabIndex = 18;
            // 
            // ItemEditForm
            // 
            this.AcceptButton = this._btnSave;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.Controls.Add(this.rootLayout);
            this.Name = "ItemEditForm";
            this.Text = "编辑日志事项";
            ((System.ComponentModel.ISupportInitialize)(this._sortUpDown)).EndInit();
            this.rootLayout.ResumeLayout(false);
            this.rootLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._progressUpDown)).EndInit();
            this.bottomBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Label lblProgress;
        private NumericUpDown _progressUpDown;
    }
}