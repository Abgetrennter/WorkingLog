using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Controls
{
    public class CategoryTreeComboBox : ComboBox
    {
        private ITemplateService _templateService;
        private ToolStripDropDown _dropDown;
        private TreeView _treeView;
        private Category _selectedCategory;

        public event EventHandler<Category> SelectedCategoryChanged;

        public CategoryTreeComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            _treeView = new TreeView
            {
                BorderStyle = BorderStyle.FixedSingle,
                HideSelection = false,
                Font = Font,
                FullRowSelect = true
            };
            _treeView.NodeMouseClick += OnNodeClicked;
            
            var host = new ToolStripControlHost(_treeView) 
            { 
                AutoSize = false, 
                Margin = Padding.Empty, 
                Padding = Padding.Empty, 
                Size = new Size(240, 260) 
            };
            
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

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    Items.Clear();
                    if (_selectedCategory != null)
                    {
                        Items.Add(_selectedCategory.Name);
                        SelectedIndex = 0;
                    }
                    SelectedCategoryChanged?.Invoke(this, _selectedCategory);
                }
            }
        }

        public void ReloadCategories()
        {
            // Preserve selection if possible
            var currentId = _selectedCategory?.Id;

            Items.Clear();
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();
            
            if (_templateService == null)
            {
                _treeView.EndUpdate();
                return;
            }

            var categories = _templateService.GetAllCategories();
            var nodes = BuildTree(categories);
            foreach (var node in nodes)
            {
                _treeView.Nodes.Add(node);
            }

            _treeView.ExpandAll();
            _treeView.EndUpdate();

            // Restore selection
            if (currentId != null)
            {
                var category = _templateService.GetCategory(currentId);
                if (category != null)
                {
                    SetSelectedCategory(category);
                }
                else
                {
                    SelectedCategory = null;
                }
            }
        }

        /// <summary>
        /// 根据分类列表构建树形节点
        /// </summary>
        /// <param name="categories">分类列表</param>
        /// <returns>根节点列表</returns>
        private List<TreeNode> BuildTree(IReadOnlyList<Category> categories)
        {
            var nodes = new Dictionary<string, TreeNode>();
            var rootNodes = new List<TreeNode>();

            // First pass: create all nodes
            foreach (var category in categories)
            {
                var node = new TreeNode(category.Name) { Tag = category };
                nodes[category.Id] = node;
            }

            // Second pass: build hierarchy
            foreach (var category in categories)
            {
                if (!string.IsNullOrEmpty(category.ParentId) && nodes.TryGetValue(category.ParentId, out var parentNode))
                {
                    parentNode.Nodes.Add(nodes[category.Id]);
                }
                else
                {
                    rootNodes.Add(nodes[category.Id]);
                }
            }

            return rootNodes;
        }

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
            if (host != null) host.Size = new Size(Math.Max(Width, 240), 260);
            _dropDown.Show(this, new Point(0, Height));
            _treeView.Focus();
        }

        public bool OnlySelectLeaf { get; set; } = false;

        /// <summary>
        /// 处理节点点击事件
        /// </summary>
        private void OnNodeClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is Category category)
            {
                if (OnlySelectLeaf && e.Node.Nodes.Count > 0)
                {
                    // If we only allow leaf nodes, and this node has children, do nothing (expand/collapse happens automatically by TreeView)
                    return;
                }
                
                SetSelectedCategory(category);
                _dropDown.Close();
            }
        }

        /// <summary>
        /// 设置选中的分类
        /// </summary>
        /// <param name="category">分类对象</param>
        private void SetSelectedCategory(Category category)
        {
            SelectedCategory = category;
        }
    }
}
