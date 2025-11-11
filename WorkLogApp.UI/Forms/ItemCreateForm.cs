using System;
using System.Drawing;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.Controls;

namespace WorkLogApp.UI.Forms
{
    public class ItemCreateForm : Form
    {
        private readonly ITemplateService _templateService;
        private readonly CategoryTreeComboBox _categoryCombo;
        private readonly DynamicFormPanel _formPanel;
        private readonly TextBox _titleBox;
        private readonly Button _btnGenerateSave;

        public ItemCreateForm(ITemplateService templateService)
        {
            _templateService = templateService;
            Text = "创建日志事项";
            Width = 800;
            Height = 600;

            var lblCategory = new Label { Text = "分类：", Location = new Point(10, 15), AutoSize = true };
            _categoryCombo = new CategoryTreeComboBox(_templateService) { Location = new Point(60, 10), Width = 200 };
            _categoryCombo.SelectedIndexChanged += (s, e) => BuildFormForCategory();

            var lblTitle = new Label { Text = "标题：", Location = new Point(300, 15), AutoSize = true };
            _titleBox = new TextBox { Location = new Point(350, 10), Width = 300 };

            _formPanel = new DynamicFormPanel { Location = new Point(10, 50), Width = 740, Height = 440, BorderStyle = BorderStyle.FixedSingle };

            _btnGenerateSave = new Button { Text = "生成并保存", Location = new Point(10, 510), Width = 120, Height = 35 };
            _btnGenerateSave.Click += OnGenerateAndSave;

            Controls.Add(lblCategory);
            Controls.Add(_categoryCombo);
            Controls.Add(lblTitle);
            Controls.Add(_titleBox);
            Controls.Add(_formPanel);
            Controls.Add(_btnGenerateSave);

            BuildFormForCategory();
        }

        private void BuildFormForCategory()
        {
            var categoryName = _categoryCombo.SelectedCategoryName;
            var catTpl = _templateService.GetCategoryTemplate(categoryName);
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
            var catTpl = _templateService.GetCategoryTemplate(categoryName);
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
            var item = new WorkLogItem
            {
                LogDate = DateTime.Now.Date,
                ItemTitle = _titleBox.Text?.Trim(),
                CategoryId = 0
            };
            var content = _templateService.Render(catTpl.FormatTemplate, values, item);

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
    }
}