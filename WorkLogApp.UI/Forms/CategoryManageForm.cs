using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Services.Implementations;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class CategoryManageForm : Form
    {
        private readonly ITemplateService _templateService;

        private readonly string[] _placeholderTypes = new[] { "text", "textarea", "select", "checkbox", "datetime" };

        // 设计期支持：提供无参构造，便于设计器实例化
        public CategoryManageForm() : this(new TemplateService())
        {
        }

        public CategoryManageForm(ITemplateService templateService)
        {
            _templateService = templateService;
            InitializeComponent();
            IconHelper.ApplyIcon(this);

            // 运行时增强：字体、缩放、抗锯齿
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);

            // 设计时避免调用（仅运行时执行）
            UIStyleManager.SetLineSpacing(_txtFormatTemplate, 1.5f);

            // 为“类型”列填充下拉选项
            var typeCol = _gridPlaceholders.Columns["colType"] as DataGridViewComboBoxColumn;
            if (typeCol != null)
            {
                typeCol.Items.AddRange(_placeholderTypes);
            }

            // 设计期：填充示例分类与占位符行，便于在设计器中预览
            if (UIStyleManager.IsDesignMode)
            {
                try
                {
                    _lstCategories.Items.Clear();
                    _lstCategories.Items.AddRange(new object[] { "通用", "任务", "会议" });
                    if (_lstCategories.Items.Count > 0) _lstCategories.SelectedIndex = 0;

                    _txtFormatTemplate.Text = "【示例模板】{标题} - {日期:yyyy-MM-dd}\\r\\n标签：{标签}\\r\\n内容：{内容}";
                    _gridPlaceholders.Rows.Clear();
                    _gridPlaceholders.Rows.Add("标题", "text", "");
                    _gridPlaceholders.Rows.Add("标签", "checkbox", "研发|测试|部署");
                    _gridPlaceholders.Rows.Add("日期", "datetime", "");
                    _gridPlaceholders.Rows.Add("内容", "textarea", "");
                    RefreshPlaceholderInsertList();
                }
                catch { }
                return;
            }

            // 复杂事件在代码中绑定，保留原有行为
            _gridPlaceholders.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex != _gridPlaceholders.Columns["colName"].Index) return;
                var row = _gridPlaceholders.Rows[e.RowIndex];
                if (row.IsNewRow) return;
                var name = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                var type = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                if (string.IsNullOrEmpty(name)) return;
                InsertPlaceholderToken(name, type);
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

            // 初始数据加载
            LoadCategories();
            RefreshPlaceholderInsertList();
        }

        private void OnCategoriesSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedCategory();
        }

        private void OnInsertPlaceholderClick(object sender, EventArgs e)
        {
            var selected = Convert.ToString(_cmbInsert.SelectedItem)?.Trim();
            if (string.IsNullOrEmpty(selected)) return;
            InsertPlaceholderToken(selected, null);
            _txtFormatTemplate.Focus();
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

        private void OnAddChildCategory(object sender, EventArgs e)
        {
            var parent = _lstCategories.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(parent))
            {
                MessageBox.Show(this, "请先选择父分类。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var suffix = Prompt("请输入子分类名称：", "新增子分类");
            if (string.IsNullOrWhiteSpace(suffix)) return;
            suffix = suffix.Trim();
            var newName = parent + "-" + suffix;
            var exists = _templateService.GetCategoryNames()?.Contains(newName) == true;
            if (exists)
            {
                MessageBox.Show(this, "该子分类已存在。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var parentTpl = _templateService.GetCategoryTemplate(parent);
            CategoryTemplate tpl;
            if (parentTpl != null)
            {
                var ph = parentTpl.Placeholders != null
                    ? new Dictionary<string, string>(parentTpl.Placeholders, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var opt = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                if (parentTpl.Options != null)
                {
                    foreach (var kv in parentTpl.Options)
                    {
                        opt[kv.Key] = kv.Value != null ? new List<string>(kv.Value) : new List<string>();
                    }
                }
                tpl = new CategoryTemplate
                {
                    FormatTemplate = parentTpl.FormatTemplate ?? string.Empty,
                    Placeholders = ph,
                    Options = opt
                };
            }
            else
            {
                tpl = new CategoryTemplate { FormatTemplate = string.Empty, Placeholders = new Dictionary<string, string>(), Options = new Dictionary<string, List<string>>() };
            }
            _templateService.AddOrUpdateCategoryTemplate(newName, tpl);
            _templateService.SaveTemplates();
            LoadCategories();
            _lstCategories.SelectedItem = newName;
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

        private static void InsertTextAtCaret(TextBoxBase textBox, string text)
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

        private void _gridPlaceholders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void _gridPlaceholders_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}