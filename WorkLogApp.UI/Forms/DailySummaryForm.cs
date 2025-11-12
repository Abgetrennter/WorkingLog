using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Core.Models;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public class DailySummaryForm : Form
    {
        private TextBox _textBox;
        private FlowLayoutPanel _bottomBar;
        private Button _btnReset;
        private Button _btnClose;
        private System.Collections.Generic.IEnumerable<WorkLogItem> _items;

        public DailySummaryForm(string initialText)
        {
            Text = "每日总结";
            StartPosition = FormStartPosition.CenterParent;
            Width = 700;
            Height = 500;

            _textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F)
            };

            _bottomBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };

            _btnClose = new Button { Text = "关闭", DialogResult = DialogResult.OK, Width = 100, Height = 32 };

            _bottomBar.Controls.Add(_btnClose);

            Controls.Add(_textBox);
            Controls.Add(_bottomBar);

            _textBox.Text = initialText ?? string.Empty;

            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
        }

        public DailySummaryForm(System.Collections.Generic.IEnumerable<WorkLogItem> items, string existingText)
        {
            Text = "每日总结";
            StartPosition = FormStartPosition.CenterParent;
            Width = 700;
            Height = 500;

            _textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F)
            };

            _bottomBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };

            _btnClose = new Button { Text = "关闭", DialogResult = DialogResult.OK, Width = 100, Height = 32 };
            _btnReset = new Button { Text = "重置", Width = 100, Height = 32 };
            _btnReset.Click += (s, e) =>
            {
                _textBox.Text = BuildChain(_items);
            };

            _bottomBar.Controls.Add(_btnClose);
            _bottomBar.Controls.Add(_btnReset);

            Controls.Add(_textBox);
            Controls.Add(_bottomBar);

            _items = items ?? Enumerable.Empty<WorkLogItem>();
            _textBox.Text = string.IsNullOrWhiteSpace(existingText) ? BuildChain(_items) : existingText;

            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
        }

        public string SummaryText => _textBox.Text;

        private static string BuildChain(System.Collections.Generic.IEnumerable<WorkLogItem> items)
        {
            return string.Join(Environment.NewLine,
                (items ?? Enumerable.Empty<WorkLogItem>())
                    .OrderBy(it => it.StartTime ?? DateTime.MinValue)
                    .ThenBy(it => it.ItemTitle ?? string.Empty)
                    .Select(it => (it.StartTime.HasValue ? it.StartTime.Value.ToString("HH:mm") : "——") + " " + (it.ItemTitle ?? string.Empty))
            );
        }
    }
}