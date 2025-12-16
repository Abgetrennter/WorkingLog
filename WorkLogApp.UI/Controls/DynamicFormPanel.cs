using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Controls
{
    public class DynamicFormPanel : Panel
    {
        private readonly Dictionary<string, Control> _controls = new Dictionary<string, Control>();

        public void BuildForm(Dictionary<string, string> placeholders)
        {
            BuildForm(placeholders, null);
        }

        public void BuildForm(Dictionary<string, string> placeholders, Dictionary<string, System.Collections.Generic.List<string>> options)
        {
            Controls.Clear();
            _controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = placeholders.Count,
                Padding = new Padding(10),
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            int rowIndex = 0;
            foreach (var kv in placeholders)
            {
                var name = kv.Key;
                var type = kv.Value?.ToLowerInvariant() ?? "text";

                var label = new Label
                {
                    Text = name + "：",
                    AutoSize = true,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Margin = new Padding(0, 8, 5, 0), // Align with input text
                    MaximumSize = new Size(115, 0),
                    TextAlign = ContentAlignment.TopRight
                };
                label.UseCompatibleTextRendering = true;
                label.Font = UIStyleManager.BodyFont;
                layout.Controls.Add(label, 0, rowIndex);

                Control input;
                switch (type)
                {
                    case "textarea":
                        var rtb = new RichTextBox { Height = 80, ScrollBars = RichTextBoxScrollBars.Vertical, BorderStyle = BorderStyle.FixedSingle };
                        UIStyleManager.SetLineSpacing(rtb, 1.5f);
                        input = rtb;
                        break;
                    case "datetime":
                        input = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };
                        break;
                    case "select":
                        var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                        if (options != null && options.TryGetValue(name, out var list) && list != null)
                        {
                            foreach (var opt in list) combo.Items.Add(opt);
                            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
                        }
                        input = combo;
                        break;
                    case "checkbox":
                        var clb = new CheckedListBox { Height = 80, CheckOnClick = true };
                        if (options != null && options.TryGetValue(name, out var olist) && olist != null)
                        {
                            foreach (var opt in olist) clb.Items.Add(opt, false);
                        }
                        input = clb;
                        break;
                    default:
                        input = new TextBox { };
                        break;
                }
                
                input.Dock = DockStyle.Fill;
                input.Font = UIStyleManager.BodyFont;
                input.Margin = new Padding(0, 0, 0, 15); // Spacing between rows

                layout.Controls.Add(input, 1, rowIndex);
                _controls[name] = input;

                rowIndex++;
            }
            
            Controls.Add(layout);
            AutoScroll = true;
        }

        public Dictionary<string, object> GetFieldValues()
        {
            var dict = new Dictionary<string, object>();
            foreach (var kv in _controls)
            {
                var ctrl = kv.Value;
                object val = null;
                if (ctrl is TextBox tb) val = tb.Text;
                else if (ctrl is RichTextBox rtb) val = rtb.Text;
                else if (ctrl is DateTimePicker dt) val = dt.Value;
                else if (ctrl is ComboBox cb) val = cb.SelectedItem?.ToString();
                else if (ctrl is CheckedListBox clb)
                {
                    var list = new List<string>();
                    foreach (var item in clb.CheckedItems) list.Add(item.ToString());
                    val = string.Join("、", list);
                }
                dict[kv.Key] = val;
            }
            return dict;
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
                    var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["标题"] = "text",
                        ["标签"] = "checkbox",
                        ["日期"] = "datetime",
                        ["内容"] = "textarea"
                    };
                    var options = new Dictionary<string, System.Collections.Generic.List<string>>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["标签"] = new System.Collections.Generic.List<string> { "研发", "测试", "部署" }
                    };
                    BuildForm(placeholders, options);
                }
            }
            catch { }
        }
    }
}