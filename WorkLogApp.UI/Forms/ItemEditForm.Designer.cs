using System.ComponentModel;
using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    partial class ItemEditForm
    {
        private IContainer components = null;

        private Label lblTitle;
        private TextBox _titleBox;
        private Label lblSummary;
        private RichTextBox _summaryBox;
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
            this.components = new Container();

            this.lblTitle = new Label();
            this._titleBox = new TextBox();
            this.lblSummary = new Label();
            this._summaryBox = new RichTextBox();
            this.lblContent = new Label();
            this._contentBox = new RichTextBox();
            this._btnSave = new Button();
            this._btnCancel = new Button();

            this.SuspendLayout();
            this.Text = "编辑日志事项";
            this.ClientSize = new System.Drawing.Size(900, 650);
            // rootLayout
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.rootLayout.ColumnCount = 2;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Padding = new System.Windows.Forms.Padding(8);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);

            // lblTitle
            this.lblTitle.Text = "标题：";
            this.lblTitle.AutoSize = true;
            // _titleBox
            this._titleBox.Dock = System.Windows.Forms.DockStyle.Fill;

            // lblSummary
            this.lblSummary.Text = "当日总结（仅第一条记录写入）：";
            this.lblSummary.AutoSize = true;
            // _summaryBox
            this._summaryBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._summaryBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            this._summaryBox.BorderStyle = BorderStyle.FixedSingle;

            // lblContent
            this.lblContent.Text = "内容：";
            this.lblContent.AutoSize = true;
            // _contentBox
            this._contentBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentBox.ScrollBars = RichTextBoxScrollBars.Both;
            this._contentBox.BorderStyle = BorderStyle.FixedSingle;

            // buttons bar
            this.bottomBar = new System.Windows.Forms.FlowLayoutPanel();
            this.bottomBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomBar.AutoSize = true;
            this.bottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnCancel.Text = "取消";
            this._btnCancel.Click += new System.EventHandler(this.OnCancelClick);
            this._btnSave.Text = "保存";
            this._btnSave.Click += new System.EventHandler(this.OnSaveClick);
            this.bottomBar.Controls.Add(this._btnSave);
            this.bottomBar.Controls.Add(this._btnCancel);

            // Accept/Cancel
            this.AcceptButton = this._btnSave;
            this.CancelButton = this._btnCancel;
            // add controls (TableLayout)
            this.rootLayout.Controls.Add(this.lblTitle, 0, 0);
            this.rootLayout.Controls.Add(this._titleBox, 1, 0);
            this.rootLayout.Controls.Add(this.lblSummary, 0, 1);
            this.rootLayout.Controls.Add(this._summaryBox, 1, 1);
            this.rootLayout.Controls.Add(this.lblContent, 0, 2);
            this.rootLayout.Controls.Add(this._contentBox, 1, 2);
            this.rootLayout.Controls.Add(this.bottomBar, 1, 3);
            this.rootLayout.SetColumnSpan(this.bottomBar, 2);
            this.Controls.Add(this.rootLayout);

            this.ResumeLayout(false);
        }
    }
}