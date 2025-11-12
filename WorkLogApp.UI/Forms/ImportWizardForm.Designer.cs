using System.ComponentModel;
using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    partial class ImportWizardForm
    {
        private IContainer components = null;

        private Panel topPanel;
        private Label _lblFile;
        private Button _btnChoose;
        private ListView _previewList;
        private Panel bottomPanel;
        private Button _btnImport;
        private ColumnHeader _colDate;
        private ColumnHeader _colTitle;
        private ColumnHeader _colStatus;
        private ColumnHeader _colTags;
        private System.Windows.Forms.TableLayoutPanel rootLayout;

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
            this.topPanel = new System.Windows.Forms.Panel();
            this._lblFile = new System.Windows.Forms.Label();
            this._btnChoose = new System.Windows.Forms.Button();
            this._previewList = new System.Windows.Forms.ListView();
            this._colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTags = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bottomPanel = new System.Windows.Forms.Panel();
            this._btnImport = new System.Windows.Forms.Button();
            this.topPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this._lblFile);
            this.topPanel.Controls.Add(this._btnChoose);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(8);
            this.topPanel.Size = new System.Drawing.Size(800, 48);
            this.topPanel.TabIndex = 2;
            this.topPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.topPanel_Paint);
            // 
            // _lblFile
            // 
            this._lblFile.AutoSize = true;
            this._lblFile.Location = new System.Drawing.Point(8, 16);
            this._lblFile.Name = "_lblFile";
            this._lblFile.Size = new System.Drawing.Size(98, 18);
            this._lblFile.TabIndex = 0;
            this._lblFile.Text = "未选择文件";
            // 
            // _btnChoose
            // 
            this._btnChoose.Location = new System.Drawing.Point(620, 10);
            this._btnChoose.Name = "_btnChoose";
            this._btnChoose.Size = new System.Drawing.Size(100, 30);
            this._btnChoose.TabIndex = 1;
            this._btnChoose.Text = "选择文件";
            this._btnChoose.Click += new System.EventHandler(this.OnChooseFile);
            // 
            // _previewList
            // 
            this._previewList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colDate,
            this._colTitle,
            this._colStatus,
            this._colTags});
            this._previewList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._previewList.FullRowSelect = true;
            this._previewList.GridLines = true;
            this._previewList.HideSelection = false;
            this._previewList.Location = new System.Drawing.Point(0, 48);
            this._previewList.Name = "_previewList";
            this._previewList.Size = new System.Drawing.Size(800, 500);
            this._previewList.TabIndex = 0;
            this._previewList.UseCompatibleStateImageBehavior = false;
            this._previewList.View = System.Windows.Forms.View.Details;
            // 
            // _colDate
            // 
            this._colDate.Text = "日期";
            this._colDate.Width = 120;
            // 
            // _colTitle
            // 
            this._colTitle.Text = "标题";
            this._colTitle.Width = 260;
            // 
            // _colStatus
            // 
            this._colStatus.Text = "状态";
            this._colStatus.Width = 120;
            // 
            // _colTags
            // 
            this._colTags.Text = "标签";
            this._colTags.Width = 200;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this._btnImport);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.Location = new System.Drawing.Point(0, 548);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(8);
            this.bottomPanel.Size = new System.Drawing.Size(800, 52);
            this.bottomPanel.TabIndex = 1;
            // 
            // _btnImport
            // 
            this._btnImport.Enabled = false;
            this._btnImport.Location = new System.Drawing.Point(0, 0);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(120, 34);
            this._btnImport.TabIndex = 0;
            this._btnImport.Text = "开始导入";
            this._btnImport.Click += new System.EventHandler(this.OnImport);
            // 
            // ImportWizardForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
            this.rootLayout.Padding = new System.Windows.Forms.Padding(0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Controls.Add(this.topPanel, 0, 0);
            this._previewList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Controls.Add(this._previewList, 0, 1);
            this.rootLayout.Controls.Add(this.bottomPanel, 0, 2);
            this.Controls.Add(this.rootLayout);
            this.Name = "ImportWizardForm";
            this.Text = "导入向导（基础版）";
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}