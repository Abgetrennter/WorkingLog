using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Controls
{
    public class DynamicFormPanel : Panel
    {
        private readonly Dictionary<string, Control> _controls = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);
        private List<TemplateField> _currentFields = new List<TemplateField>();
        private TableLayoutPanel _layout;

        #region 旧版本兼容方法

        public void BuildForm(Dictionary<string, string> placeholders)
        {
            BuildForm(placeholders, null);
        }

        public void BuildForm(Dictionary<string, string> placeholders, Dictionary<string, System.Collections.Generic.List<string>> options)
        {
            // 转换为新的字段格式
            var fields = new List<TemplateField>();
            int order = 0;
            foreach (var kv in placeholders ?? new Dictionary<string, string>())
            {
                var field = new TemplateField
                {
                    Key = kv.Key,
                    Name = kv.Key,
                    Type = kv.Value?.ToLowerInvariant() ?? "text",
                    Order = order++
                };

                if (options != null && options.TryGetValue(kv.Key, out var opts))
                {
                    field.Options = opts;
                }

                fields.Add(field);
            }

            BuildForm(fields);
        }

        #endregion

        /// <summary>
        /// 使用结构化字段定义构建表单
        /// </summary>
        public void BuildForm(List<TemplateField> fields)
        {
            _currentFields = fields ?? new List<TemplateField>();
            _controls.Clear();

            // 按 Order 排序
            var sortedFields = _currentFields.OrderBy(f => f.Order).ToList();

            // 清理旧布局
            if (_layout != null)
            {
                Controls.Remove(_layout);
                _layout.Dispose();
            }

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = sortedFields.Count,
                Padding = new Padding(10),
            };

            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int rowIndex = 0;
            foreach (var field in sortedFields)
            {
                // 创建标签
                var label = CreateLabel(field);
                _layout.Controls.Add(label, 0, rowIndex);

                // 创建输入控件
                var input = CreateInputControl(field);
                _layout.Controls.Add(input, 1, rowIndex);
                _controls[field.Key] = input;

                rowIndex++;
            }

            Controls.Add(_layout);
            AutoScroll = true;
        }

        private Label CreateLabel(TemplateField field)
        {
            var labelText = field.Name + "：";
            if (field.IsRequired)
            {
                labelText = "* " + labelText;
            }

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Margin = new Padding(0, 8, 5, 0),
                MaximumSize = new Size(115, 0),
                TextAlign = ContentAlignment.TopRight,
                Font = UIStyleManager.BodyFont,
                UseCompatibleTextRendering = false,
                Tag = field // 存储字段信息
            };

            // 必填项标签用红色
            if (field.IsRequired)
            {
                label.ForeColor = Color.Red;
            }

            // 如果有帮助文本，添加工具提示
            if (!string.IsNullOrEmpty(field.HelpText))
            {
                var tooltip = new ToolTip();
                tooltip.SetToolTip(label, field.HelpText);
            }

            return label;
        }

        private Control CreateInputControl(TemplateField field)
        {
            Control input;
            var type = field.Type?.ToLowerInvariant() ?? "text";

            switch (type)
            {
                case "textarea":
                    var rtb = new RichTextBox
                    {
                        Height = 80,
                        ScrollBars = RichTextBoxScrollBars.Vertical,
                        BorderStyle = BorderStyle.FixedSingle,
                        Text = field.DefaultValue ?? ""
                    };
                    UIStyleManager.SetLineSpacing(rtb, 1.5f);
                    if (!string.IsNullOrEmpty(field.Placeholder))
                    {
                        // RichTextBox 不支持 Placeholder，可考虑自定义绘制
                    }
                    input = rtb;
                    break;

                case "datetime":
                    input = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "yyyy-MM-dd HH:mm"
                    };
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        if (DateTime.TryParse(field.DefaultValue, out var dt))
                        {
                            ((DateTimePicker)input).Value = dt;
                        }
                    }
                    break;

                case "date":
                    input = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "yyyy-MM-dd"
                    };
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        if (DateTime.TryParse(field.DefaultValue, out var dt))
                        {
                            ((DateTimePicker)input).Value = dt;
                        }
                    }
                    break;

                case "time":
                    input = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "HH:mm",
                        ShowUpDown = true
                    };
                    break;

                case "select":
                    var combo = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    if (field.Options != null)
                    {
                        foreach (var opt in field.Options) combo.Items.Add(opt);
                        if (combo.Items.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(field.DefaultValue) && combo.Items.Contains(field.DefaultValue))
                            {
                                combo.SelectedItem = field.DefaultValue;
                            }
                            else
                            {
                                combo.SelectedIndex = 0;
                            }
                        }
                    }
                    input = combo;
                    break;

                case "multiselect":
                    var multiCombo = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDown,
                        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource = AutoCompleteSource.ListItems
                    };
                    if (field.Options != null)
                    {
                        foreach (var opt in field.Options) multiCombo.Items.Add(opt);
                    }
                    input = multiCombo;
                    break;

                case "checkbox":
                    var clb = new CheckedListBox
                    {
                        Height = 80,
                        CheckOnClick = true
                    };
                    if (field.Options != null)
                    {
                        foreach (var opt in field.Options) clb.Items.Add(opt, false);
                    }
                    input = clb;
                    break;

                case "radio":
                    var panel = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = true
                    };
                    if (field.Options != null)
                    {
                        bool first = true;
                        foreach (var opt in field.Options)
                        {
                            var rb = new RadioButton
                            {
                                Text = opt,
                                AutoSize = true,
                                Checked = first || opt == field.DefaultValue
                            };
                            panel.Controls.Add(rb);
                            first = false;
                        }
                    }
                    input = panel;
                    break;

                case "number":
                    var numBox = new NumericUpDown
                    {
                        Minimum = 0,
                        Maximum = 999999,
                        DecimalPlaces = 0
                    };
                    if (!string.IsNullOrEmpty(field.DefaultValue) && int.TryParse(field.DefaultValue, out var num))
                    {
                        numBox.Value = num;
                    }
                    input = numBox;
                    break;

                case "autocomplete":
                    var autoBox = new TextBox();
                    // 自动补全功能需要配合历史数据，这里先作为普通文本框
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        autoBox.Text = field.DefaultValue;
                    }
                    if (!string.IsNullOrEmpty(field.Placeholder))
                    {
                        // 文本框不直接支持 Placeholder，可考虑自定义
                    }
                    input = autoBox;
                    break;

                case "duration":
                    // 时长选择器：小时 + 分钟
                    var durationPanel = new Panel { Height = 30 };
                    var hourBox = new NumericUpDown
                    {
                        Location = new Point(0, 0),
                        Width = 60,
                        Minimum = 0,
                        Maximum = 999
                    };
                    var hourLabel = new Label
                    {
                        Text = "小时",
                        Location = new Point(65, 3),
                        AutoSize = true
                    };
                    var minBox = new NumericUpDown
                    {
                        Location = new Point(110, 0),
                        Width = 60,
                        Minimum = 0,
                        Maximum = 59
                    };
                    var minLabel = new Label
                    {
                        Text = "分钟",
                        Location = new Point(175, 3),
                        AutoSize = true
                    };
                    durationPanel.Controls.AddRange(new Control[] { hourBox, hourLabel, minBox, minLabel });
                    input = durationPanel;
                    break;

                case "rangedatetime":
                    // 时间范围选择器
                    var rangePanel = new Panel { Height = 60 };
                    var startPicker = new DateTimePicker
                    {
                        Location = new Point(0, 0),
                        Width = 200,
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "yyyy-MM-dd HH:mm"
                    };
                    var endPicker = new DateTimePicker
                    {
                        Location = new Point(0, 30),
                        Width = 200,
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "yyyy-MM-dd HH:mm"
                    };
                    var toLabel = new Label
                    {
                        Text = "至",
                        Location = new Point(210, 15),
                        AutoSize = true
                    };
                    rangePanel.Controls.AddRange(new Control[] { startPicker, endPicker, toLabel });
                    input = rangePanel;
                    break;

                default: // text
                    var tb = new TextBox();
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        tb.Text = field.DefaultValue;
                    }
                    input = tb;
                    break;
            }

            input.Dock = DockStyle.Fill;
            input.Font = UIStyleManager.BodyFont;
            input.Margin = new Padding(0, 0, 0, 15);
            input.Tag = field; // 存储字段信息用于验证

            return input;
        }

        /// <summary>
        /// 获取字段值字典
        /// </summary>
        /// <param name="fillEmptyWithNone">是否将非必填项的空值填充为"无"</param>
        public Dictionary<string, object> GetFieldValues(bool fillEmptyWithNone = false)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _controls)
            {
                var ctrl = kv.Value;
                var field = ctrl.Tag as TemplateField;
                object val = GetControlValue(ctrl);
                
                // 处理空值：将非必填项的空值填充为"无"
                if (fillEmptyWithNone && val != null)
                {
                    var strVal = val.ToString();
                    if (string.IsNullOrWhiteSpace(strVal) && (field == null || !field.IsRequired))
                    {
                        val = "无";
                    }
                }
                
                dict[kv.Key] = val;
            }
            return dict;
        }

        private object GetControlValue(Control ctrl)
        {
            switch (ctrl)
            {
                case TextBox tb:
                    return tb.Text;
                case RichTextBox rtb:
                    return rtb.Text;
                case DateTimePicker dt:
                    return dt.Value;
                case ComboBox cb:
                    return cb.SelectedItem?.ToString() ?? cb.Text;
                case CheckedListBox clb:
                    var list = new List<string>();
                    foreach (var item in clb.CheckedItems) list.Add(item.ToString());
                    return string.Join("、", list);
                case NumericUpDown nud:
                    return (int)nud.Value;
                case FlowLayoutPanel flp when flp.Controls.Count > 0:
                    // RadioButton 组
                    foreach (RadioButton rb in flp.Controls.OfType<RadioButton>())
                    {
                        if (rb.Checked) return rb.Text;
                    }
                    return null;
                case Panel panel:
                    // 处理复合控件（duration, rangeDateTime）
                    return GetCompositeControlValue(panel);
                default:
                    return null;
            }
        }

        private object GetCompositeControlValue(Panel panel)
        {
            // 尝试识别控件类型并获取值
            var numericControls = panel.Controls.OfType<NumericUpDown>().ToList();
            var dateControls = panel.Controls.OfType<DateTimePicker>().ToList();

            if (numericControls.Count == 2)
            {
                // Duration 控件
                var hours = (int)numericControls[0].Value;
                var minutes = (int)numericControls[1].Value;
                return $"{hours}小时{minutes}分钟";
            }

            if (dateControls.Count == 2)
            {
                // RangeDateTime 控件
                var start = dateControls[0].Value;
                var end = dateControls[1].Value;
                return new { Start = start, End = end, Duration = end - start };
            }

            return null;
        }

        /// <summary>
        /// 验证表单字段
        /// </summary>
        public ValidationResult ValidateForm()
        {
            var errors = new List<string>();

            foreach (var kv in _controls)
            {
                var field = kv.Value.Tag as TemplateField;
                if (field == null) continue;

                var value = GetControlValue(kv.Value);
                var stringValue = value?.ToString() ?? "";

                // 必填验证
                if (field.IsRequired && string.IsNullOrWhiteSpace(stringValue))
                {
                    errors.Add($"[{field.Name}] 为必填项");
                    continue;
                }

                // 正则验证
                if (!string.IsNullOrEmpty(field.ValidationRule) && !string.IsNullOrEmpty(stringValue))
                {
                    if (!Regex.IsMatch(stringValue, field.ValidationRule))
                    {
                        errors.Add($"[{field.Name}] 格式不正确");
                    }
                }
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            base.OnPaint(e);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            try
            {
                if (UIStyleManager.IsDesignMode && Controls.Count == 0)
                {
                    if (UIStyleManager.BodyFont == null) UIStyleManager.Initialize();
                    
                    // 使用新的字段格式显示设计时示例
                    var fields = new List<TemplateField>
                    {
                        new TemplateField { Key = "title", Name = "标题", Type = "text", Order = 0 },
                        new TemplateField { Key = "category", Name = "类型", Type = "select", Order = 1, Options = new List<string> { "研发", "测试", "部署" } },
                        new TemplateField { Key = "date", Name = "日期", Type = "date", Order = 2 },
                        new TemplateField { Key = "content", Name = "内容", Type = "textarea", Order = 3, IsRequired = true }
                    };
                    
                    BuildForm(fields);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
