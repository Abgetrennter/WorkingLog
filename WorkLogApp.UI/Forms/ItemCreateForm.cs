using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Enums;
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
            IconHelper.ApplyIcon(this);

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

            // 初始化状态下拉
            _statusCombo.Items.Clear();
            _statusCombo.Items.AddRange(new object[] { "待办", "进行中", "已完成", "阻塞", "已取消" });
            _statusCombo.SelectedIndex = 0;

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

            var status = StatusEnum.Todo;
            switch (_statusCombo.SelectedItem?.ToString())
            {
                case "进行中": status = StatusEnum.Doing; break;
                case "已完成": status = StatusEnum.Done; break;
                case "阻塞": status = StatusEnum.Blocked; break;
                case "已取消": status = StatusEnum.Cancelled; break;
            }

            var item = new WorkLogItem
            {
                LogDate = _datePicker.Value.Date,
                Status = status,
                ItemTitle = _titleBox.Text?.Trim(),
                Tags = _tagsBox.Text?.Trim(),
                StartTime = _startPicker.Checked ? (DateTime?)_startPicker.Value : null,
                EndTime = _endPicker.Checked ? (DateTime?)_endPicker.Value : null,
                CategoryId = StableIdFromName(categoryName)
            };

            // 如果分类模板名不为空，但用户未填写标签，可选择自动补充分类名到标签（可选）
            if (string.IsNullOrWhiteSpace(item.Tags) && !string.IsNullOrWhiteSpace(categoryName))
            {
                item.Tags = categoryName;
            }
            var content = _templateService.Render(catTpl.FormatTemplate, values, item);
            // 将初始内容同步到模型
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
                var filePath = Path.Combine(dataDir, fileName);
                File.WriteAllText(filePath, item.ItemContent);

                // MessageBox.Show(this, "保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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