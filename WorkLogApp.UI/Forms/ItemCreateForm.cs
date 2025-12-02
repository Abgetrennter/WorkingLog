using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
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
        private WorkTemplate _currentTemplate;
        
        // 设计期支持：提供无参构造，便于设计器实例化
        public ItemCreateForm() : this(new TemplateService())
        {
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
            
            _categoryCombo.SelectedCategoryChanged += (s, cat) => LoadTemplateForCategory(cat);

            // 初始化状态下拉
            _statusCombo.DisplayMember = "Value";
            _statusCombo.ValueMember = "Key";
            _statusCombo.DataSource = StatusHelper.GetList();
            _statusCombo.SelectedValue = StatusEnum.Done; // Default to "已完成"
            
            InitToolTips();
        }
        
        private void InitToolTips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_btnGenerateSave, "根据模板生成内容并保存日志");
        }

        private void LoadTemplateForCategory(Category category)
        {
            _currentTemplate = null;
            _formPanel.BuildForm(new Dictionary<string, string>()); // Clear form
            
            if (category == null) return;
            
            var templates = _templateService.GetTemplatesByCategory(category.Id);
            var tpl = templates.FirstOrDefault();
            
            if (tpl != null)
            {
                _currentTemplate = tpl;
                BuildFormForTemplate(tpl);
                
                // Auto-fill tags
                if (tpl.Tags != null && tpl.Tags.Any())
                {
                    var existingTags = _tagsBox.Text.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach(var tag in tpl.Tags)
                    {
                        if (!existingTags.Contains(tag)) existingTags.Add(tag);
                    }
                    _tagsBox.Text = string.Join(", ", existingTags);
                }
            }
        }

        private void BuildFormForTemplate(WorkTemplate tpl)
        {
            if (tpl == null) return;
            _formPanel.BuildForm(tpl.Placeholders, tpl.Options);
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
                CategoryId = _categoryCombo.SelectedCategory.Id
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
                var dataDir = Path.Combine(baseDir, "Data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

                IImportExportService exportService = new ImportExportService();
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
