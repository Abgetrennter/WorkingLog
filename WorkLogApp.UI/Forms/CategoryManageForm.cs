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
        // Map internal type (key) to display name (value)
        private readonly Dictionary<string, string> _typeMap = new Dictionary<string, string>
        {
            { "text", "文本框" },
            { "textarea", "多行文本" },
            { "select", "下拉选择" },
            { "checkbox", "复选框" },
            { "datetime", "日期时间" }
        };
        private Category _selectedCategory;
        private WorkTemplate _selectedTemplate;

        // TreeView Controls
        private TreeView _treeView;
        private ContextMenuStrip _treeMenu;
        
        // Template List Controls (Removed)
        // private ListBox _lstTemplates;
        // private ContextMenuStrip _templateMenu;

        public CategoryManageForm() : this(new TemplateService())
        {
        }

        public CategoryManageForm(ITemplateService templateService)
        {
            _templateService = templateService;
            InitializeComponent();
            
            // Initialize placeholder types
            if (colType != null)
            {
                colType.Items.AddRange(_typeMap.Values.ToArray());
            }

            IconHelper.ApplyIcon(this);
            
            // Rebuild UI manually since we are drastically changing it and the designer might be confused by the switch
            SetupNewLayout();
            
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
            
            // Bind events
            BindEvents();

            // Initial Load
            LoadCategoryTree();
            RefreshPlaceholderInsertList();
        }

        private void SetupNewLayout()
        {
            // Clear existing controls in Left Panel
            if (this.leftPanel != null)
            {
                this.leftPanel.Controls.Clear();
                _treeView = new TreeView
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None,
                    HideSelection = false,
                    FullRowSelect = true
                };
                this.leftPanel.Controls.Add(_treeView);

                // Context Menu for Tree
                _treeMenu = new ContextMenuStrip();
                _treeMenu.Items.Add("新建分类(文件夹)", null, OnAddChildCategory);
                _treeMenu.Items.Add("新建模板", null, OnAddTemplateNode);
                _treeMenu.Items.Add(new ToolStripSeparator());
                _treeMenu.Items.Add("重命名", null, OnRenameCategory);
                _treeMenu.Items.Add("删除", null, OnDeleteCategory);
                _treeView.ContextMenuStrip = _treeMenu;
            }

            // Right Panel Layout
            if (this.rightPanel != null)
            {
                // Save existing controls (specifically the layout panel) to move them
                var editorLayout = this._layoutRight;
                this.rightPanel.Controls.Clear();
                
                // No more template list, just the editor
                if (editorLayout != null)
                {
                    this.rightPanel.Controls.Add(editorLayout);
                }
            }
        }

        // Legacy event handlers to satisfy Designer
        private void OnCategoriesSelectedIndexChanged(object sender, EventArgs e) { }
        private void OnRemoveCategory(object sender, EventArgs e) { }
        private void OnAddCategory(object sender, EventArgs e) { }
        private void _gridPlaceholders_CellContentClick_1(object sender, DataGridViewCellEventArgs e) { }
        private void OnSaveCategory(object sender, EventArgs e) { OnSave(sender, e); }

        private void BindEvents()
        {
            _treeView.AfterSelect += OnTreeAfterSelect;
            _treeView.ItemDrag += OnTreeItemDrag;
            _treeView.DragEnter += OnTreeDragEnter;
            _treeView.DragDrop += OnTreeDragDrop;

            // Editor events
             _gridPlaceholders.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex != _gridPlaceholders.Columns["colName"].Index) return;
                var row = _gridPlaceholders.Rows[e.RowIndex];
                if (row.IsNewRow) return;
                var name = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                var displayType = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                if (string.IsNullOrEmpty(name)) return;
                // Convert display name to internal type
                var typeKey = _typeMap.FirstOrDefault(x => x.Value == displayType).Key ?? "text";
                InsertPlaceholderToken(name, typeKey);
                _txtFormatTemplate.Focus();
            };
            _gridPlaceholders.RowsAdded += (s, e) => RefreshPlaceholderInsertList();
            _gridPlaceholders.RowsRemoved += (s, e) => RefreshPlaceholderInsertList();
            _gridPlaceholders.CellValueChanged += (s, e) => RefreshPlaceholderInsertList();
            
            // Save Button
            _btnSave.Click -= OnSave; // Remove old if any
            _btnSave.Click += OnSave;
            
            // Add root category button (if exists in UI, or add one)
            // Assuming there is a button to add category in the original UI, likely handled by context menu now.
        }

        #region Tree Logic

        private void LoadCategoryTree()
        {
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();
            
            var categories = _templateService.GetAllCategories();
            var nodeMap = new Dictionary<string, TreeNode>();
            var rootNodes = new List<TreeNode>();

            foreach (var cat in categories)
            {
                var node = new TreeNode(cat.Name) { Tag = cat };
                nodeMap[cat.Id] = node;
            }

            foreach (var cat in categories)
            {
                if (!string.IsNullOrEmpty(cat.ParentId) && nodeMap.ContainsKey(cat.ParentId))
                {
                    nodeMap[cat.ParentId].Nodes.Add(nodeMap[cat.Id]);
                }
                else
                {
                    rootNodes.Add(nodeMap[cat.Id]);
                }
            }

            foreach (var node in rootNodes) _treeView.Nodes.Add(node);

            _treeView.ExpandAll();
            _treeView.EndUpdate();
        }

        private void OnTreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedCategory = e.Node.Tag as Category;
            if (_selectedCategory != null)
            {
                var templates = _templateService.GetTemplatesByCategory(_selectedCategory.Id);
                _selectedTemplate = templates.FirstOrDefault();
                if (_selectedTemplate != null)
                {
                    LoadTemplateToEditor(_selectedTemplate);
                    _layoutRight.Enabled = true;
                }
                else
                {
                    ClearEditor();
                    _layoutRight.Enabled = false; // Disable editor for folders
                }
            }
        }

        private void OnAddChildCategory(object sender, EventArgs e)
        {
            var parent = _treeView.SelectedNode?.Tag as Category;
            var name = Prompt("请输入分类名称：", "新建分类");
            if (string.IsNullOrWhiteSpace(name)) return;

            var newCat = _templateService.CreateCategory(name, parent?.Id);
            
            var node = new TreeNode(newCat.Name) { Tag = newCat };
            if (_treeView.SelectedNode != null)
            {
                _treeView.SelectedNode.Nodes.Add(node);
                _treeView.SelectedNode.Expand();
            }
            else
            {
                _treeView.Nodes.Add(node);
            }
        }

        private void OnAddTemplateNode(object sender, EventArgs e)
        {
            var parent = _treeView.SelectedNode?.Tag as Category;
            var name = Prompt("请输入模板名称：", "新建模板");
            if (string.IsNullOrWhiteSpace(name)) return;

            // 1. Create Category (Leaf)
            var newCat = _templateService.CreateCategory(name, parent?.Id);
            
            // 2. Create Template
            var newTpl = new WorkTemplate
            {
                Name = name,
                CategoryId = newCat.Id,
                Content = "",
                Tags = new List<string>(),
                Placeholders = new Dictionary<string, string>(),
                Options = new Dictionary<string, List<string>>()
            };
            _templateService.CreateTemplate(newTpl);

            // 3. Add Node
            var node = new TreeNode(newCat.Name) { Tag = newCat };
            if (_treeView.SelectedNode != null)
            {
                _treeView.SelectedNode.Nodes.Add(node);
                _treeView.SelectedNode.Expand();
            }
            else
            {
                _treeView.Nodes.Add(node);
            }
            
            // 4. Select it to show editor
            _treeView.SelectedNode = node;
        }

        private void OnRenameCategory(object sender, EventArgs e)
        {
            var cat = _treeView.SelectedNode?.Tag as Category;
            if (cat == null) return;

            var newName = Prompt("请输入新名称：", "重命名", cat.Name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            cat.Name = newName;
            if (_templateService.UpdateCategory(cat))
            {
                _treeView.SelectedNode.Text = newName;
            }
        }

        private void OnDeleteCategory(object sender, EventArgs e)
        {
            var cat = _treeView.SelectedNode?.Tag as Category;
            if (cat == null) return;

            if (MessageBox.Show($"确定要删除 '{cat.Name}' 吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_templateService.DeleteCategory(cat.Id))
                {
                    _treeView.SelectedNode.Remove();
                    _selectedCategory = null;
                    _selectedTemplate = null;
                    ClearEditor();
                }
            }
        }

        private void OnTreeItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void OnTreeDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void OnTreeDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                Point pt = _treeView.PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = _treeView.GetNodeAt(pt);
                TreeNode draggedNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");

                if (draggedNode != null && destinationNode != draggedNode)
                {
                    // Move logic
                    var draggedCat = draggedNode.Tag as Category;
                    var destCat = destinationNode?.Tag as Category; // null if root

                    if (draggedCat != null)
                    {
                         // Check for circular reference is handled in service, but let's avoid self-drop
                        if (destinationNode != null && IsDescendantNode(draggedNode, destinationNode)) return;

                        if (_templateService.MoveCategory(draggedCat.Id, destCat?.Id))
                        {
                            draggedNode.Remove();
                            if (destinationNode != null)
                            {
                                destinationNode.Nodes.Add(draggedNode);
                                destinationNode.Expand();
                            }
                            else
                            {
                                _treeView.Nodes.Add(draggedNode);
                            }
                        }
                    }
                }
            }
        }

        private bool IsDescendantNode(TreeNode potentialAncestor, TreeNode potentialDescendant)
        {
            if (potentialDescendant == potentialAncestor) return true;
            var p = potentialDescendant.Parent;
            while (p != null)
            {
                if (p == potentialAncestor) return true;
                p = p.Parent;
            }
            return false;
        }

        #endregion

        #region Template List Logic
        // Removed
        #endregion

        #region Editor Logic

        private void LoadTemplateToEditor(WorkTemplate t)
        {
            if (t == null)
            {
                ClearEditor();
                return;
            }

            _txtFormatTemplate.Text = t.Content;
            // Tags? Need a textbox for tags.
            // I'll assume there isn't one in the designer, so I might need to add one or prepend to content for now?
            // Ideally I should add a txtTags control.
            // Let's use the Title label to show name for now, and maybe I can't edit tags easily without adding control.
            // Wait, I have control over the form code. I can add a Tag textbox.
            
            EnsureTagControlExists();
            _txtTags.Text = string.Join(", ", t.Tags);
            
            _gridPlaceholders.Rows.Clear();
            if (t.Placeholders != null)
            {
                foreach (var kv in t.Placeholders)
                {
                    string opts = "";
                    if (t.Options != null && t.Options.ContainsKey(kv.Key))
                    {
                        opts = string.Join("|", t.Options[kv.Key]);
                    }
                    // Convert internal type to display name
                    string displayType = _typeMap.ContainsKey(kv.Value) ? _typeMap[kv.Value] : kv.Value;
                    _gridPlaceholders.Rows.Add(kv.Key, displayType, opts);
                }
            }
            RefreshPlaceholderInsertList();
        }

        private TextBox _txtTags;
        private void EnsureTagControlExists()
        {
            if (_txtTags == null)
            {
                // Find where to put it. Maybe above _txtFormatTemplate?
                // The layout is dynamic now.
                var panel = _txtFormatTemplate.Parent;
                _txtTags = new TextBox { Location = new Point(120, _txtFormatTemplate.Top - 25), Width = 200 };
                var lblTags = new Label { Text = "标签(逗号分隔):", AutoSize = true };
                
                panel.Controls.Add(lblTags);
                panel.Controls.Add(_txtTags);

                // Align label tightly to the left of the textbox
                // Adding 3 to Top for better vertical alignment with the textbox text
                lblTags.Location = new Point(_txtTags.Left - lblTags.PreferredWidth - 2, _txtTags.Top + 3);
            }
        }

        private void ClearEditor()
        {
            _txtFormatTemplate.Text = "";
            _gridPlaceholders.Rows.Clear();
            if (_txtTags != null) _txtTags.Text = "";
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (_selectedTemplate == null) return;

            _selectedTemplate.Content = _txtFormatTemplate.Text;
            
            if (_txtTags != null)
            {
                _selectedTemplate.Tags = _txtTags.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct().ToList();
            }

            // Parse placeholders from grid
            var placeholders = new Dictionary<string, string>();
            var options = new Dictionary<string, List<string>>();

            foreach (DataGridViewRow row in _gridPlaceholders.Rows)
            {
                if (row.IsNewRow) continue;
                var name = Convert.ToString(row.Cells["colName"].Value)?.Trim();
                var displayType = Convert.ToString(row.Cells["colType"].Value)?.Trim();
                var opts = Convert.ToString(row.Cells["colOptions"].Value)?.Trim();

                if (!string.IsNullOrEmpty(name))
                {
                    // Convert display name back to internal type
                    var typeKey = _typeMap.FirstOrDefault(x => x.Value == displayType).Key ?? "text";
                    placeholders[name] = typeKey;
                    if (!string.IsNullOrEmpty(opts))
                    {
                        options[name] = opts.Split('|').Select(s => s.Trim()).ToList();
                    }
                }
            }
            
            _selectedTemplate.Placeholders = placeholders;
            _selectedTemplate.Options = options;

            if (_templateService.UpdateTemplate(_selectedTemplate))
            {
                MessageBox.Show("保存成功。");
            }
        }

        private void InsertPlaceholderToken(string name, string type)
        {
             int selectionIndex = _txtFormatTemplate.SelectionStart;
             string token = "{" + name + (type == "datetime" ? ":yyyy-MM-dd HH:mm" : "") + "}";
             _txtFormatTemplate.Text = _txtFormatTemplate.Text.Insert(selectionIndex, token);
             _txtFormatTemplate.SelectionStart = selectionIndex + token.Length;
        }

        private void RefreshPlaceholderInsertList()
        {
            _cmbInsert.Items.Clear();
            foreach (DataGridViewRow row in _gridPlaceholders.Rows)
            {
                if (row.IsNewRow) continue;
                var name = Convert.ToString(row.Cells["colName"].Value);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _cmbInsert.Items.Add(name);
                }
            }
            if (_cmbInsert.Items.Count > 0) _cmbInsert.SelectedIndex = 0;
        }
        
        private void OnInsertPlaceholderClick(object sender, EventArgs e)
        {
             var selected = Convert.ToString(_cmbInsert.SelectedItem)?.Trim();
            if (string.IsNullOrEmpty(selected)) return;
            InsertPlaceholderToken(selected, null);
            _txtFormatTemplate.Focus();
        }

        #endregion

        #region Helpers

        private string Prompt(string text, string caption, string defaultVal = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultVal };
            Button confirmation = new Button() { Text = "确定", Left = 250, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : "";
        }

        #endregion
    }
}
