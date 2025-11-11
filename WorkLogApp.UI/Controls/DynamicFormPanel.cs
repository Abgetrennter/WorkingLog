using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WorkLogApp.UI.Controls
{
    public class DynamicFormPanel : Panel
    {
        private readonly Dictionary<string, Control> _controls = new Dictionary<string, Control>();

        public void BuildForm(Dictionary<string, string> placeholders)
        {
            Controls.Clear();
            _controls.Clear();

            var y = 10;
            foreach (var kv in placeholders)
            {
                var name = kv.Key;
                var type = kv.Value?.ToLowerInvariant() ?? "text";

                var label = new Label
                {
                    Text = name + "：",
                    Location = new Point(10, y + 4),
                    AutoSize = true
                };
                Controls.Add(label);

                Control input;
                switch (type)
                {
                    case "textarea":
                        input = new TextBox { Multiline = true, Width = 500, Height = 80 };
                        break;
                    case "datetime":
                        input = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };
                        break;
                    default:
                        input = new TextBox { Width = 500 };
                        break;
                }
                input.Location = new Point(120, y);
                Controls.Add(input);
                _controls[name] = input;

                y += input.Height + 15;
            }
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
    }
}