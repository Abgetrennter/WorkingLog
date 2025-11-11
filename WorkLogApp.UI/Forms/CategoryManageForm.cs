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

            _gridPlaceholders = new DataGridView { Location = new Point(8, 240), Width = 620, Height = 250, AllowUserToAddRows = true, AllowUserToDeleteRows = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            _gridPlaceholders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "占位符名称", Name = "colName" });
            var typeCol = new DataGridViewComboBoxColumn { HeaderText = "类型", Name = "colType" };
            typeCol.Items.AddRange(_placeholderTypes);
            _gridPlaceholders.Columns.Add(typeCol);

            _btnSave = new Button { Text = "保存", Location = new Point(8, 500), Width = 120, Height = 34 };
            _btnSave.Click += OnSaveCategory;

            rightPanel.Controls.Add(lblFormat);
            rightPanel.Controls.Add(_txtFormatTemplate);
            rightPanel.Controls.Add(_gridPlaceholders);
            rightPanel.Controls.Add(_btnSave);

            Controls.Add(rightPanel);
            Controls.Add(leftPanel);

            LoadCategories();
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
            var tpl = _templateService.GetCategoryTemplate(name) ?? new CategoryTemplate { FormatTemplate = string.Empty, Placeholders = new Dictionary<string, string>() };
            _txtFormatTemplate.Text = tpl.FormatTemplate ?? string.Empty;
            _gridPlaceholders.Rows.Clear();
            if (tpl.Placeholders != null)
            {
                foreach (var kv in tpl.Placeholders)
                {
                    _gridPlaceholders.Rows.Add(kv.Key, kv.Value);
                }
            }
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
            foreach (DataGridViewRow row in _gridPlaceholders.Rows)
            {
                if (row.IsNewRow) continue;
                var key = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                var type = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(type) || !_placeholderTypes.Contains(type)) type = "text";
                placeholders[key] = type;
            }

            var tpl = new CategoryTemplate
            {
                FormatTemplate = _txtFormatTemplate.Text ?? string.Empty,
                Placeholders = placeholders
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