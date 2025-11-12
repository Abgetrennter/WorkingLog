using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    partial class CategoryManageForm
    {
        private IContainer components = null;

        private Panel leftPanel;
        private ListBox _lstCategories;
        private Button _btnAdd;
        private Button _btnRemove;

        private Panel rightPanel;
        private Label lblFormat;
        private RichTextBox _txtFormatTemplate;
        private Label lblQuick;
        private ComboBox _cmbInsert;
        private Button _btnInsert;
        private DataGridView _gridPlaceholders;
        private Button _btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colName;
        private System.Windows.Forms.DataGridViewComboBoxColumn _colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colOptions;

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
            this.leftPanel = new System.Windows.Forms.Panel();
            this._lstCategories = new System.Windows.Forms.ListBox();
            this._btnRemove = new System.Windows.Forms.Button();
            this._btnAdd = new System.Windows.Forms.Button();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.lblFormat = new System.Windows.Forms.Label();
            this._txtFormatTemplate = new System.Windows.Forms.RichTextBox();
            this.lblQuick = new System.Windows.Forms.Label();
            this._cmbInsert = new System.Windows.Forms.ComboBox();
            this._btnInsert = new System.Windows.Forms.Button();
            this._gridPlaceholders = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colOptions = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._btnSave = new System.Windows.Forms.Button();
            this.leftPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridPlaceholders)).BeginInit();
            this.SuspendLayout();
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this._lstCategories);
            this.leftPanel.Controls.Add(this._btnRemove);
            this.leftPanel.Controls.Add(this._btnAdd);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(8);
            this.leftPanel.Size = new System.Drawing.Size(220, 670);
            this.leftPanel.TabIndex = 1;
            // 
            // _lstCategories
            // 
            this._lstCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lstCategories.ItemHeight = 18;
            this._lstCategories.Location = new System.Drawing.Point(8, 72);
            this._lstCategories.Name = "_lstCategories";
            this._lstCategories.Size = new System.Drawing.Size(204, 590);
            this._lstCategories.TabIndex = 0;
            this._lstCategories.SelectedIndexChanged += new System.EventHandler(this.OnCategoriesSelectedIndexChanged);
            // 
            // _btnRemove
            // 
            this._btnRemove.Dock = System.Windows.Forms.DockStyle.Top;
            this._btnRemove.Location = new System.Drawing.Point(8, 40);
            this._btnRemove.Name = "_btnRemove";
            this._btnRemove.Size = new System.Drawing.Size(204, 32);
            this._btnRemove.TabIndex = 1;
            this._btnRemove.Text = "删除分类";
            this._btnRemove.Click += new System.EventHandler(this.OnRemoveCategory);
            // 
            // _btnAdd
            // 
            this._btnAdd.Dock = System.Windows.Forms.DockStyle.Top;
            this._btnAdd.Location = new System.Drawing.Point(8, 8);
            this._btnAdd.Name = "_btnAdd";
            this._btnAdd.Size = new System.Drawing.Size(204, 32);
            this._btnAdd.TabIndex = 2;
            this._btnAdd.Text = "新增分类";
            this._btnAdd.Click += new System.EventHandler(this.OnAddCategory);
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.lblFormat);
            this.rightPanel.Controls.Add(this._txtFormatTemplate);
            this.rightPanel.Controls.Add(this.lblQuick);
            this.rightPanel.Controls.Add(this._cmbInsert);
            this.rightPanel.Controls.Add(this._btnInsert);
            this.rightPanel.Controls.Add(this._gridPlaceholders);
            this.rightPanel.Controls.Add(this._btnSave);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(220, 0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(8);
            this.rightPanel.Size = new System.Drawing.Size(751, 670);
            this.rightPanel.TabIndex = 0;
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(8, 8);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(98, 18);
            this.lblFormat.TabIndex = 0;
            this.lblFormat.Text = "格式模板：";
            // 
            // _txtFormatTemplate
            // 
            this._txtFormatTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtFormatTemplate.Location = new System.Drawing.Point(8, 28);
            this._txtFormatTemplate.Name = "_txtFormatTemplate";
            this._txtFormatTemplate.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this._txtFormatTemplate.Size = new System.Drawing.Size(714, 200);
            this._txtFormatTemplate.TabIndex = 1;
            this._txtFormatTemplate.Text = "";
            // 
            // lblQuick
            // 
            this.lblQuick.AutoSize = true;
            this.lblQuick.Location = new System.Drawing.Point(11, 243);
            this.lblQuick.Name = "lblQuick";
            this.lblQuick.Size = new System.Drawing.Size(152, 18);
            this.lblQuick.TabIndex = 2;
            this.lblQuick.Text = "快速插入占位符：";
            // 
            // _cmbInsert
            // 
            this._cmbInsert.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbInsert.Location = new System.Drawing.Point(225, 240);
            this._cmbInsert.Name = "_cmbInsert";
            this._cmbInsert.Size = new System.Drawing.Size(320, 26);
            this._cmbInsert.TabIndex = 3;
            // 
            // _btnInsert
            // 
            this._btnInsert.Location = new System.Drawing.Point(592, 239);
            this._btnInsert.Name = "_btnInsert";
            this._btnInsert.Size = new System.Drawing.Size(90, 26);
            this._btnInsert.TabIndex = 4;
            this._btnInsert.Text = "插入所选";
            this._btnInsert.Click += new System.EventHandler(this.OnInsertPlaceholderClick);
            // 
            // _gridPlaceholders
            // 
            this._gridPlaceholders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._gridPlaceholders.ColumnHeadersHeight = 34;
            this._gridPlaceholders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colType,
            this.colOptions});
            this._gridPlaceholders.Location = new System.Drawing.Point(8, 289);
            this._gridPlaceholders.Name = "_gridPlaceholders";
            this._gridPlaceholders.RowHeadersWidth = 62;
            this._gridPlaceholders.Size = new System.Drawing.Size(714, 305);
            this._gridPlaceholders.TabIndex = 5;
            // 
            // colName
            // 
            this.colName.HeaderText = "占位符名称";
            this.colName.MinimumWidth = 8;
            this.colName.Name = "colName";
            // 
            // colType
            // 
            this.colType.HeaderText = "类型";
            this.colType.MinimumWidth = 8;
            this.colType.Name = "colType";
            // 
            // colOptions
            // 
            this.colOptions.HeaderText = "选项（|分隔）";
            this.colOptions.MinimumWidth = 8;
            this.colOptions.Name = "colOptions";
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(8, 624);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(120, 34);
            this._btnSave.TabIndex = 6;
            this._btnSave.Text = "保存";
            this._btnSave.Click += new System.EventHandler(this.OnSaveCategory);
            // 
            // CategoryManageForm
            // 
            this.ClientSize = new System.Drawing.Size(971, 670);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
            this.Name = "CategoryManageForm";
            this.Text = "分类与模板管理";
            this.leftPanel.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.rightPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridPlaceholders)).EndInit();
            this.ResumeLayout(false);

        }

        private DataGridViewTextBoxColumn colName;
        private DataGridViewComboBoxColumn colType;
        private DataGridViewTextBoxColumn colOptions;
    }
}