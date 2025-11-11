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

            this.Text = "分类与模板管理";
            this.ClientSize = new Size(900, 600);

            // left
            this.leftPanel = new Panel { Dock = DockStyle.Left, Width = 220, Padding = new Padding(8) };
            this._lstCategories = new ListBox { Dock = DockStyle.Fill };
            this._lstCategories.SelectedIndexChanged += new System.EventHandler(this.OnCategoriesSelectedIndexChanged);
            this._btnAdd = new Button { Text = "新增分类", Dock = DockStyle.Top, Height = 32 };
            this._btnAdd.Click += new System.EventHandler(this.OnAddCategory);
            this._btnRemove = new Button { Text = "删除分类", Dock = DockStyle.Top, Height = 32 };
            this._btnRemove.Click += new System.EventHandler(this.OnRemoveCategory);
            this.leftPanel.Controls.Add(this._lstCategories);
            this.leftPanel.Controls.Add(this._btnRemove);
            this.leftPanel.Controls.Add(this._btnAdd);

            // right
            this.rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            this.lblFormat = new Label { Text = "格式模板：", AutoSize = true, Location = new Point(8, 8) };
            this._txtFormatTemplate = new RichTextBox { ScrollBars = RichTextBoxScrollBars.Vertical, Location = new Point(8, 28), Width = 620, Height = 200, BorderStyle = BorderStyle.FixedSingle };
            this.lblQuick = new Label { Text = "快速插入占位符：", AutoSize = true, Location = new Point(8, 232) };
            this._cmbInsert = new ComboBox { Location = new Point(120, 228), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            this._btnInsert = new Button { Text = "插入所选", Location = new Point(450, 226), Width = 90, Height = 26 };
            this._btnInsert.Click += new System.EventHandler(this.OnInsertPlaceholderClick);

            this._gridPlaceholders = new DataGridView { Location = new Point(8, 260), Width = 620, Height = 230, AllowUserToAddRows = true, AllowUserToDeleteRows = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            var colName = new DataGridViewTextBoxColumn { HeaderText = "占位符名称", Name = "colName" };
            var colType = new DataGridViewComboBoxColumn { HeaderText = "类型", Name = "colType" };
            var colOptions = new DataGridViewTextBoxColumn { HeaderText = "选项（|分隔）", Name = "colOptions" };
            this._gridPlaceholders.Columns.Add(colName);
            this._gridPlaceholders.Columns.Add(colType);
            this._gridPlaceholders.Columns.Add(colOptions);

            this._btnSave = new Button { Text = "保存", Location = new Point(8, 500), Width = 120, Height = 34 };
            this._btnSave.Click += new System.EventHandler(this.OnSaveCategory);

            this.rightPanel.Controls.Add(this.lblFormat);
            this.rightPanel.Controls.Add(this._txtFormatTemplate);
            this.rightPanel.Controls.Add(this.lblQuick);
            this.rightPanel.Controls.Add(this._cmbInsert);
            this.rightPanel.Controls.Add(this._btnInsert);
            this.rightPanel.Controls.Add(this._gridPlaceholders);
            this.rightPanel.Controls.Add(this._btnSave);

            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
        }
    }
}