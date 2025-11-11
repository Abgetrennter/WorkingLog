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

            // lblTitle
            this.lblTitle.Text = "标题：";
            this.lblTitle.Left = 10;
            this.lblTitle.Top = 15;
            this.lblTitle.AutoSize = true;

            // _titleBox
            this._titleBox.Left = 60;
            this._titleBox.Top = 10;
            this._titleBox.Width = 800;

            // lblSummary
            this.lblSummary.Text = "当日总结（仅第一条记录写入）：";
            this.lblSummary.Left = 10;
            this.lblSummary.Top = 45;
            this.lblSummary.AutoSize = true;

            // _summaryBox
            this._summaryBox.Left = 10;
            this._summaryBox.Top = 70;
            this._summaryBox.Width = 850;
            this._summaryBox.Height = 120;
            this._summaryBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            this._summaryBox.BorderStyle = BorderStyle.FixedSingle;

            // lblContent
            this.lblContent.Text = "内容：";
            this.lblContent.Left = 10;
            this.lblContent.Top = 200;
            this.lblContent.AutoSize = true;

            // _contentBox
            this._contentBox.Left = 10;
            this._contentBox.Top = 225;
            this._contentBox.Width = 850;
            this._contentBox.Height = 325;
            this._contentBox.ScrollBars = RichTextBoxScrollBars.Both;
            this._contentBox.BorderStyle = BorderStyle.FixedSingle;

            // _btnSave
            this._btnSave.Text = "保存";
            this._btnSave.Left = 10;
            this._btnSave.Top = 565;
            this._btnSave.Width = 100;
            this._btnSave.Height = 35;
            this._btnSave.Click += new System.EventHandler(this.OnSaveClick);

            // _btnCancel
            this._btnCancel.Text = "取消";
            this._btnCancel.Left = 120;
            this._btnCancel.Top = 565;
            this._btnCancel.Width = 100;
            this._btnCancel.Height = 35;
            this._btnCancel.Click += new System.EventHandler(this.OnCancelClick);

            // Accept/Cancel
            this.AcceptButton = this._btnSave;
            this.CancelButton = this._btnCancel;

            // add controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this._titleBox);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this._summaryBox);
            this.Controls.Add(this.lblContent);
            this.Controls.Add(this._contentBox);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this._btnCancel);

            this.ResumeLayout(false);
        }
    }
}