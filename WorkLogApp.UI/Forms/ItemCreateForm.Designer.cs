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
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this._titleBox = new System.Windows.Forms.TextBox();
            this._btnGenerateSave = new System.Windows.Forms.Button();
            this._categoryCombo = new WorkLogApp.UI.Controls.CategoryTreeComboBox();
            this._formPanel = new WorkLogApp.UI.Controls.DynamicFormPanel();
            this.SuspendLayout();
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(12, 13);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(62, 18);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "分类：";
            this.lblCategory.Click += new System.EventHandler(this.lblCategory_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(367, 13);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(62, 18);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "标题：";
            // 
            // _titleBox
            // 
            this._titleBox.Location = new System.Drawing.Point(450, 10);
            this._titleBox.Name = "_titleBox";
            this._titleBox.Size = new System.Drawing.Size(300, 28);
            this._titleBox.TabIndex = 3;
            // 
            // _btnGenerateSave
            // 
            this._btnGenerateSave.Location = new System.Drawing.Point(303, 528);
            this._btnGenerateSave.Name = "_btnGenerateSave";
            this._btnGenerateSave.Size = new System.Drawing.Size(197, 60);
            this._btnGenerateSave.TabIndex = 5;
            this._btnGenerateSave.Text = "生成并保存";
            this._btnGenerateSave.Click += new System.EventHandler(this.OnGenerateAndSave);
            // 
            // _categoryCombo
            // 
            this._categoryCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._categoryCombo.Location = new System.Drawing.Point(112, 10);
            this._categoryCombo.Name = "_categoryCombo";
            this._categoryCombo.Size = new System.Drawing.Size(200, 26);
            this._categoryCombo.TabIndex = 1;
            this._categoryCombo.TemplateService = null;
            // 
            // _formPanel
            // 
            this._formPanel.AutoScroll = true;
            this._formPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._formPanel.Location = new System.Drawing.Point(15, 50);
            this._formPanel.Name = "_formPanel";
            this._formPanel.Size = new System.Drawing.Size(760, 472);
            this._formPanel.TabIndex = 4;
            this._formPanel.Paint += new System.Windows.Forms.PaintEventHandler(this._formPanel_Paint);
            // 
            // ItemCreateForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this._categoryCombo);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this._titleBox);
            this.Controls.Add(this._formPanel);
            this.Controls.Add(this._btnGenerateSave);
            this.Name = "ItemCreateForm";
            this.Text = "创建日志事项";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}