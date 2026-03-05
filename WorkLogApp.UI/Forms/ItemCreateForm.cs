using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Constants;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.UI.Helpers;
using WorkLogApp.UI.Controls;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class ItemCreateForm : Form
    {
        private readonly ITemplateService _templateService;
        private WorkTemplate _currentTemplate;
        
        // 设计期支持：提供无参构造，便于设计器实例化
        public ItemCreateForm()
        {
            // 设计时：使用空服务实例
            if (UIStyleManager.IsDesignMode)
            {
                _templateService = null;
            }
            else
            {
                // 运行时：通过 DI 容器获取
                throw new InvalidOperationException("请使用带参数的构造函数进行依赖注入");
            }
        }

        public ItemCreateForm(ITemplateService templateService)
        {
            _templateService = templateService;
            InitializeComponent();

            IconHelper.ApplyIcon(this);
            
            // 应用统一样式（字体、缩放、抗锯齿）
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);

            if (UIStyleManager.IsDesignMode) return;

            // 运行时：将模板服务注入到分类选择控件，并响应选择变化
            _categoryCombo.TemplateService = _templateService;
            _categoryCombo.OnlySelectLeaf = true; // Only allow selecting leaf nodes (which map to templates)
            
            _categoryCombo.SelectedCategoryChanged += (s, category) => LoadTemplateForCategory(category);

            // 初始化状态下拉
            _statusCombo.DisplayMember = "Value";
            _statusCombo.ValueMember = "Key";
            _statusCombo.DataSource = StatusHelper.GetList();
            _statusCombo.SelectedValue = StatusEnum.Done; // Default to "已完成"
            
            InitToolTips();
        }

        /// <summary>
        /// 设置初始日期（在显示前调用）
        /// </summary>
        public void SetInitialDate(DateTime date)
        {
            if (_datePicker != null)
            {
                _datePicker.Value = date;
            }
        }
        
        private void InitToolTips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_btnGenerateSave, "根据模板生成内容并保存日志");
        }

        private void LoadTemplateForCategory(Category category)
        {
            _currentTemplate = null;
            _formPanel.BuildForm(new List<TemplateField>()); // Clear form
            
            if (category == null) return;
            
            var templates = _templateService.GetTemplatesByCategory(category.Id);
            var template = templates.FirstOrDefault();
            
            if (template != null)
            {
                _currentTemplate = template;
                BuildFormForTemplate(template);
                
                // Auto-fill title with template name (only if title is empty)
                if (string.IsNullOrWhiteSpace(_titleBox.Text))
                {
                    _titleBox.Text = template.Name;
                }
                
                // Auto-fill tags
                if (template.Tags != null && template.Tags.Any())
                {
                    var existingTags = _tagsBox.Text.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach(var tag in template.Tags)
                    {
                        if (!existingTags.Contains(tag)) existingTags.Add(tag);
                    }
                    _tagsBox.Text = string.Join(", ", existingTags);
                }
            }
        }

        /// <summary>
        /// 根据模板构建表单
        /// </summary>
        /// <param name="template">工作模板</param>
        private void BuildFormForTemplate(WorkTemplate template)
        {
            if (template == null) return;
            // 使用新的 GetEffectiveFields() 方法，优先使用 Fields，否则转换 Placeholders
            var fields = template.GetEffectiveFields();
            _formPanel.BuildForm(fields);
        }

        private void OnGenerateAndSave(object sender, EventArgs e)
        {
            // Validate
             if (_categoryCombo.SelectedCategory == null)
            {
                MessageBox.Show(this, "请选择分类。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            if (_currentTemplate == null)
            {
                 // If no template selected (or auto-loaded), warn user
                 if (MessageBox.Show("当前分类没有关联模板，确定要继续吗？", "提示", MessageBoxButtons.YesNo) == DialogResult.No) return;
            }

            if (string.IsNullOrWhiteSpace(_titleBox.Text))
            {
                MessageBox.Show(this, "请填写标题", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证表单字段
            var validationResult = _formPanel.ValidateForm();
            if (!validationResult.IsValid)
            {
                var errorMsg = string.Join("\n", validationResult.Errors);
                MessageBox.Show(this, "表单验证失败：\n" + errorMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var values = _formPanel.GetFieldValues();
            
            // Build category path string for display/log if needed
            // We might want to store full path name or just ID.
            // WorkLogItem has CategoryId (int). New system uses GUID string.
            // This is a breaking change for WorkLogItem.CategoryId if it expects int.
            // Let's check WorkLogItem definition.
            
            var status = StatusEnum.Todo;
            if (_statusCombo.SelectedValue is StatusEnum s)
            {
                status = s;
            }

            var item = new WorkLogItem
            {
                LogDate = _datePicker.Value.Date,
                Status = status,
                ItemTitle = _titleBox.Text?.Trim(),
                Tags = _tagsBox.Text?.Trim(),
                StartTime = _startPicker.Checked ? (DateTime?)_startPicker.Value : null,
                EndTime = _endPicker.Checked ? (DateTime?)_endPicker.Value : null,
                CategoryName = _categoryCombo.SelectedCategory.Name
            };

            // Generate Content
            string content = "";
            if (_currentTemplate != null)
            {
                content = _templateService.Render(_currentTemplate.Content, values, item);
            }
            else
            {
                // Fallback content
                content = item.ItemTitle;
            }
            
            item.ItemContent = content;

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, AppConstants.DataDirectoryName);
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

                IImportExportService exportService = ServiceFactory.GetImportExportService();
                var day = new WorkLog { LogDate = item.LogDate.Date, Items = new System.Collections.Generic.List<WorkLogItem> { item } };
                var success = exportService.ExportMonth(item.LogDate, new[] { day }, dataDir);

                if (!success)
                {
                    MessageBox.Show(this, "保存失败：导出未成功", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 文本备份
                var safeTitle = string.IsNullOrWhiteSpace(item.ItemTitle) ? "untitled" : SanitizeFileName(item.ItemTitle);
                var fileName = $"{item.LogDate:yyyy-MM-dd}_{safeTitle}.txt";
                
                // 按照 yyyy/MM/dd 结构存储
                var year = item.LogDate.ToString("yyyy");
                var month = item.LogDate.ToString("MM");
                var dayStr = item.LogDate.ToString("dd");
                var txtDir = Path.Combine(dataDir, year, month, dayStr);
                if (!Directory.Exists(txtDir)) Directory.CreateDirectory(txtDir);

                var filePath = Path.Combine(txtDir, fileName);
                File.WriteAllText(filePath, item.ItemContent);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = new string(Path.GetInvalidFileNameChars());
            var pattern = "[" + Regex.Escape(invalid) + "]";
            return Regex.Replace(name, pattern, "_");
        }

        private void _formPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblCategory_Click(object sender, EventArgs e)
        {

        }
    }
}
