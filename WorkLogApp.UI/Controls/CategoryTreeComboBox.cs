using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Controls
{
    public class CategoryTreeComboBox : ComboBox
    {
        private ITemplateService _templateService;
        private ToolStripDropDown _dropDown;
        private TreeView _treeView;
        private string _selectedCategoryName;

        public CategoryTreeComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            _treeView = new TreeView
            {
                BorderStyle = BorderStyle.FixedSingle,
                HideSelection = false,
                PathSeparator = "-",
                Font = Font
            };
            _treeView.NodeMouseClick += OnNodeClicked;
            var host = new ToolStripControlHost(_treeView) { AutoSize = false, Margin = Padding.Empty, Padding = Padding.Empty, Size = new Size(240, 260) };
            _dropDown = new ToolStripDropDown { Padding = Padding.Empty };
            _dropDown.Items.Add(host);
        }

        public CategoryTreeComboBox(ITemplateService templateService) : this()
        {
            TemplateService = templateService;
        }

        public ITemplateService TemplateService
        {
            get => _templateService;
            set
            {
                _templateService = value;
                if (_templateService != null)
                {
                    ReloadCategories();
                }
            }
        }

        public void ReloadCategories()
        {
            Items.Clear();
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();
            if (_templateService == null)
            {
                _treeView.EndUpdate();
                return;
            }
            var map = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            var names = _templateService.GetCategoryNames() ?? Enumerable.Empty<string>();
            foreach (var name in names.OrderBy(n => n))
            {
                var parts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                string path = string.Empty;
                TreeNode parent = null;
                for (int i = 0; i < parts.Length; i++)
                {
                    path = i == 0 ? parts[0] : path + "-" + parts[i];
                    if (!map.TryGetValue(path, out var node))
                    {
                        node = new TreeNode(parts[i]) { Name = path };
                        if (parent == null) _treeView.Nodes.Add(node);
                        else parent.Nodes.Add(node);
                        map[path] = node;
                    }
                    parent = node;
                }
            }
            _treeView.ExpandAll();
            _treeView.EndUpdate();
            var first = names.FirstOrDefault();
            if (first != null) SetSelectedCategory(first);
        }

        public string SelectedCategoryName => _selectedCategoryName;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            ShowTree();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.F4) ShowTree();
        }

        private void ShowTree()
        {
            if (_dropDown == null) return;
            _treeView.Font = Font;
            var host = _dropDown.Items[0] as ToolStripControlHost;
            if (host != null) host.Size = new Size(Width, 260);
            _dropDown.Show(this, new Point(0, Height));
            _treeView.Focus();
        }

        private void OnNodeClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            var path = e.Node.FullPath;
            SetSelectedCategory(path);
            _dropDown.Close();
        }

        private void SetSelectedCategory(string path)
        {
            _selectedCategoryName = path;
            Items.Clear();
            Items.Add(path);
            SelectedIndex = 0;
        }
    }
}