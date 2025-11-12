using System;
using System.Drawing;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Services.Implementations;
using WorkLogApp.UI.Controls;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class ItemCreateForm : Form
    {
        private readonly ITemplateService _templateService;
        
        
        // 设计期支持：提供无参构造，便于设计器实例化
        public ItemCreateForm() : this(new TemplateService())
        {
        }

        public ItemCreateForm(ITemplateService templateService)
        {
            _templateService = templateService;
            InitializeComponent();

            // 应用统一样式（字体、缩放、抗锯齿）
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);

            // 设计期：填充示例数据与动态表单，避免依赖外部模板文件
            if (UIStyleManager.IsDesignMode)
            {
                try
                {
                    _titleBox.Text = "示例标题";
                    _categoryCombo.Items.Clear();
                    _categoryCombo.Items.AddRange(new object[] { "通用", "任务", "会议" });
                    if (_categoryCombo.Items.Count > 0) _categoryCombo.SelectedIndex = 0;

                    var placeholders = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["标题"] = "text",
                        ["标签"] = "checkbox",
                        ["日期"] = "datetime",
                        ["内容"] = "textarea"
                    };
                    var options = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["标签"] = new System.Collections.Generic.List<string> { "研发", "测试", "部署" }
                    };
                    _formPanel.BuildForm(placeholders, options);
                }
                catch { }
                return;
            }

            // 运行时：将模板服务注入到分类选择控件，并响应选择变化
            _categoryCombo.TemplateService = _templateService;
            _categoryCombo.SelectedIndexChanged += (s, e) => BuildFormForCategory();

            // 初次构建表单（基于模板服务）
            BuildFormForCategory();
        }

        private void BuildFormForCategory()
        {
            var categoryName = _categoryCombo.SelectedCategoryName;
            var catTpl = _templateService.GetMergedCategoryTemplate(categoryName);
            if (catTpl == null)
            {
                _formPanel.BuildForm(new System.Collections.Generic.Dictionary<string, string>());
                return;
            }
            _formPanel.BuildForm(catTpl.Placeholders, catTpl.Options);
        }

        private void OnGenerateAndSave(object sender, EventArgs e)
        {
            var categoryName = _categoryCombo.SelectedCategoryName;
            var catTpl = _templateService.GetMergedCategoryTemplate(categoryName);
            if (catTpl == null)
            {
                MessageBox.Show(this, "未找到分类模板。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(_titleBox.Text))
            {
                MessageBox.Show(this, "请填写标题", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var values = _formPanel.GetFieldValues();
            values["CategoryPath"] = categoryName ?? string.Empty;
            var item = new WorkLogItem
            {
                LogDate = _datePicker.Value.Date,
                ItemTitle = _titleBox.Text?.Trim(),
                CategoryId = StableIdFromName(categoryName),
                Tags = string.IsNullOrWhiteSpace(categoryName) ? null : categoryName
            };
            var content = _templateService.Render(catTpl.FormatTemplate, values, item);
            // 将初始内容同步到模型，确保编辑窗体与模型一致
            item.ItemContent = content;

            // 打开编辑窗口，允许再次修改并保存纯文本
            using (var editor = new ItemEditForm(item, content))
            {
                editor.StartPosition = FormStartPosition.CenterParent;
                var result = editor.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // 编辑窗口已提示保存成功并关闭，此处直接关闭创建界面
                    Close();
                }
            }
        }

        private void _formPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblCategory_Click(object sender, EventArgs e)
        {

        }

        // 以模板名称生成稳定的正整数 ID（FNV-1a 32-bit）
        private static int StableIdFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return 0;
            unchecked
            {
                const uint fnvOffset = 2166136261;
                const uint fnvPrime = 16777619;
                uint hash = fnvOffset;
                foreach (var ch in name)
                {
                    hash ^= ch;
                    hash *= fnvPrime;
                }
                return (int)(hash & 0x7FFFFFFF);
            }
        }
    }
}