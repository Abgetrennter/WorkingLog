using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Forms
{
    public class CategoryManageForm : Form
    {
        private readonly ITemplateService _templateService;
        private readonly ListBox _lstCategories;
        private readonly TextBox _txtFormatTemplate;
        private readonly DataGridView _gridPlaceholders;
        private readonly ComboBox _cmbInsert;
        private readonly Button _btnInsert;
        private readonly Button _btnAdd;
        private readonly Button _btnRemove;
        private readonly Button _btnSave;

        private readonly string[] _placeholderTypes = new[] { "text", "textarea", "select", "checkbox", "datetime" };

        public CategoryManageForm(ITemplateService templateService)
        {
            _templateService = templateService;
            Text = "分类与模板管理";
            Width = 900;
            Height = 600;

            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 220, Padding = new Padding(8) };
            var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

            _lstCategories = new ListBox { Dock = DockStyle.Fill };
            _lstCategories.SelectedIndexChanged += (s, e) => LoadSelectedCategory();

            _btnAdd = new Button { Text = "新增分类", Dock = DockStyle.Top, Height = 32 };
            _btnAdd.Click += OnAddCategory;

            _btnRemove = new Button { Text = "删除分类", Dock = DockStyle.Top, Height = 32 };
            _btnRemove.Click += OnRemoveCategory;

            leftPanel.Controls.Add(_lstCategories);
            leftPanel.Controls.Add(_btnRemove);
            leftPanel.Controls.Add(_btnAdd);

            var lblFormat = new Label { Text = "格式模板：", AutoSize = true, Location = new Point(8, 8) };
            _txtFormatTemplate = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Location = new Point(8, 28), Width = 620, Height = 200, Font = new Font(FontFamily.GenericMonospace, 9f) };

            var lblQuick = new Label { Text = "快速插入占位符：", AutoSize = true, Location = new Point(8, 232) };
            _cmbInsert = new ComboBox { Location = new Point(120, 228), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            _btnInsert = new Button { Text = "插入所选", Location = new Point(450, 226), Width = 90, Height = 26 };
            _btnInsert.Click += (s, e) => InsertSelectedPlaceholder();

            _gridPlaceholders = new DataGridView { Location = new Point(8, 260), Width = 620, Height = 230, AllowUserToAddRows = true, AllowUserToDeleteRows = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            _gridPlaceholders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "占位符名称", Name = "colName" });
            var typeCol = new DataGridViewComboBoxColumn { HeaderText = "类型", Name = "colType" };
            typeCol.Items.AddRange(_placeholderTypes);
            _gridPlaceholders.Columns.Add(typeCol);
            _gridPlaceholders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "选项（|分隔）", Name = "colOptions" });
            _gridPlaceholders.CellDoubleClick += (s, e) =>
            {
                // 仅当双击的是“占位符名称”列的有效行时才插入
                if (e.RowIndex < 0) return; // 跳过表头
                if (e.ColumnIndex != _gridPlaceholders.Columns["colName"].Index) return; // 仅名称列生效
                var row = _gridPlaceholders.Rows[e.RowIndex];
                if (row.IsNewRow) return; // 跳过新建行
                var name = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                var type = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                if (string.IsNullOrEmpty(name)) return;
                InsertPlaceholderToken(name, type);
                // 双击后将焦点切换到模板编辑器，便于继续编辑
                _txtFormatTemplate.Focus();
            };
            _gridPlaceholders.RowsAdded += (s, e) => RefreshPlaceholderInsertList();
            _gridPlaceholders.RowsRemoved += (s, e) => RefreshPlaceholderInsertList();
            _gridPlaceholders.CellValueChanged += (s, e) => RefreshPlaceholderInsertList();
            _gridPlaceholders.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_gridPlaceholders.IsCurrentCellDirty)
                {
                    _gridPlaceholders.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            _btnSave = new Button { Text = "保存", Location = new Point(8, 500), Width = 120, Height = 34 };
            _btnSave.Click += OnSaveCategory;

            rightPanel.Controls.Add(lblFormat);
            rightPanel.Controls.Add(_txtFormatTemplate);
            rightPanel.Controls.Add(lblQuick);
            rightPanel.Controls.Add(_cmbInsert);
            rightPanel.Controls.Add(_btnInsert);
            rightPanel.Controls.Add(_gridPlaceholders);
            rightPanel.Controls.Add(_btnSave);

            Controls.Add(rightPanel);
            Controls.Add(leftPanel);

            LoadCategories();
            RefreshPlaceholderInsertList();
        }

        private void LoadCategories()
        {
            _lstCategories.Items.Clear();
            var names = _templateService.GetCategoryNames() ?? Enumerable.Empty<string>();
            foreach (var n in names) _lstCategories.Items.Add(n);
            if (_lstCategories.Items.Count > 0) _lstCategories.SelectedIndex = 0;
        }

        private void LoadSelectedCategory()
        {
            var name = _lstCategories.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name)) return;
            var tpl = _templateService.GetCategoryTemplate(name) ?? new CategoryTemplate { FormatTemplate = string.Empty, Placeholders = new Dictionary<string, string>(), Options = new Dictionary<string, List<string>>() };
            _txtFormatTemplate.Text = tpl.FormatTemplate ?? string.Empty;
            _gridPlaceholders.Rows.Clear();
            if (tpl.Placeholders != null)
            {
                foreach (var kv in tpl.Placeholders)
                {
                    string opt = null;
                    if (tpl.Options != null && tpl.Options.TryGetValue(kv.Key, out var list) && list != null)
                    {
                        opt = string.Join("|", list);
                    }
                    _gridPlaceholders.Rows.Add(kv.Key, kv.Value, opt);
                }
            }
            RefreshPlaceholderInsertList();
        }

        private void OnAddCategory(object sender, EventArgs e)
        {
            var name = Prompt("请输入新分类名称：", "新增分类");
            if (string.IsNullOrWhiteSpace(name)) return;
            var exists = _templateService.GetCategoryNames()?.Contains(name) == true;
            if (exists)
            {
                MessageBox.Show(this, "该分类已存在。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _templateService.AddOrUpdateCategoryTemplate(name, new CategoryTemplate { FormatTemplate = string.Empty, Placeholders = new Dictionary<string, string>() });
            _templateService.SaveTemplates();
            LoadCategories();
            _lstCategories.SelectedItem = name;
        }

        private void RefreshPlaceholderInsertList()
        {
            var names = new List<string>();
            foreach (DataGridViewRow row in _gridPlaceholders.Rows)
            {
                if (row.IsNewRow) continue;
                var key = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                if (!string.IsNullOrEmpty(key)) names.Add(key);
            }
            _cmbInsert.BeginUpdate();
            _cmbInsert.Items.Clear();
            foreach (var n in names.Distinct()) _cmbInsert.Items.Add(n);
            if (_cmbInsert.Items.Count > 0 && _cmbInsert.SelectedIndex == -1) _cmbInsert.SelectedIndex = 0;
            _cmbInsert.EndUpdate();
        }

        private void InsertSelectedPlaceholder()
        {
            var name = _cmbInsert.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name)) return;
            // 查找类型以便为 datetime 添加默认格式
            string type = null;
            foreach (DataGridViewRow row in _gridPlaceholders.Rows)
            {
                if (row.IsNewRow) continue;
                var key = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                {
                    type = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                    break;
                }
            }
            InsertPlaceholderToken(name, type);
        }

        private void InsertPlaceholderToken(string name, string type)
        {
            var token = "{" + name;
            if (string.Equals(type, "datetime", StringComparison.OrdinalIgnoreCase))
            {
                token += ":yyyy-MM-dd HH:mm";
            }
            token += "}";
            InsertTextAtCaret(_txtFormatTemplate, token);
        }

        private static void InsertTextAtCaret(TextBox textBox, string text)
        {
            if (textBox == null) return;
            var selStart = textBox.SelectionStart;
            var selLength = textBox.SelectionLength;
            var current = textBox.Text ?? string.Empty;
            string next;
            if (selLength > 0)
            {
                next = current.Substring(0, selStart) + text + current.Substring(selStart + selLength);
            }
            else
            {
                next = current.Substring(0, selStart) + text + current.Substring(selStart);
            }
            textBox.Text = next;
            textBox.SelectionStart = selStart + text.Length;
            textBox.SelectionLength = 0;
            textBox.Focus();
        }

        private void OnRemoveCategory(object sender, EventArgs e)
        {
            var name = _lstCategories.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name)) return;
            if (MessageBox.Show(this, $"确定删除分类[{name}]吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _templateService.RemoveCategory(name);
            _templateService.SaveTemplates();
            LoadCategories();
        }

        private void OnSaveCategory(object sender, EventArgs e)
        {
            var name = _lstCategories.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(this, "请先选择分类。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var options = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in _gridPlaceholders.Rows)
            {
                if (row.IsNewRow) continue;
                var key = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                var type = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                var optStr = Convert.ToString(row.Cells["colOptions"].Value)?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(type) || !_placeholderTypes.Contains(type)) type = "text";
                placeholders[key] = type;
                if (!string.IsNullOrEmpty(optStr))
                {
                    var list = optStr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .Where(s => s.Length > 0)
                                     .ToList();
                    if (list.Count > 0) options[key] = list;
                }
            }

            var tpl = new CategoryTemplate
            {
                FormatTemplate = _txtFormatTemplate.Text ?? string.Empty,
                Placeholders = placeholders,
                Options = options
            };
            _templateService.AddOrUpdateCategoryTemplate(name, tpl);
            if (_templateService.SaveTemplates())
            {
                MessageBox.Show(this, "已保存模板。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "保存失败，请检查模板文件路径是否有效。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Prompt(string text, string caption)
        {
            var form = new Form { Width = 360, Height = 140, Text = caption, StartPosition = FormStartPosition.CenterParent };
            var lbl = new Label { Left = 10, Top = 10, Text = text, AutoSize = true };
            var txt = new TextBox { Left = 10, Top = 35, Width = 320 };
            var ok = new Button { Text = "确定", Left = 170, Width = 75, Top = 70, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "取消", Left = 255, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };
            form.Controls.Add(lbl);
            form.Controls.Add(txt);
            form.Controls.Add(ok);
            form.Controls.Add(cancel);
            form.AcceptButton = ok;
            form.CancelButton = cancel;
            return form.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }
    }
}