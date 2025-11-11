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
        private DynamicFormPanel _formPanel;
        private Button _btnGenerateSave;

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

            this.lblCategory = new Label();
            this._categoryCombo = new CategoryTreeComboBox();
            this.lblTitle = new Label();
            this._titleBox = new TextBox();
            this._formPanel = new DynamicFormPanel();
            this._btnGenerateSave = new Button();

            this.SuspendLayout();
            this.Text = "创建日志事项";
            this.ClientSize = new System.Drawing.Size(800, 600);

            // lblCategory
            this.lblCategory.Text = "分类：";
            this.lblCategory.Location = new System.Drawing.Point(10, 15);
            this.lblCategory.AutoSize = true;

            // _categoryCombo
            this._categoryCombo.Location = new System.Drawing.Point(60, 10);
            this._categoryCombo.Width = 200;

            // lblTitle
            this.lblTitle.Text = "标题：";
            this.lblTitle.Location = new System.Drawing.Point(300, 15);
            this.lblTitle.AutoSize = true;

            // _titleBox
            this._titleBox.Location = new System.Drawing.Point(350, 10);
            this._titleBox.Width = 300;

            // _formPanel
            this._formPanel.Location = new System.Drawing.Point(10, 50);
            this._formPanel.Width = 740;
            this._formPanel.Height = 440;
            this._formPanel.BorderStyle = BorderStyle.FixedSingle;

            // _btnGenerateSave
            this._btnGenerateSave.Text = "生成并保存";
            this._btnGenerateSave.Location = new System.Drawing.Point(10, 510);
            this._btnGenerateSave.Width = 120;
            this._btnGenerateSave.Height = 35;
            this._btnGenerateSave.Click += new System.EventHandler(this.OnGenerateAndSave);

            // add controls
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this._categoryCombo);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this._titleBox);
            this.Controls.Add(this._formPanel);
            this.Controls.Add(this._btnGenerateSave);

            this.ResumeLayout(false);
        }
    }
}