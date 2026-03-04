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
            MinimumSize = new Size(500, 350);
            Width = 875;
            Height = 625;
            IconHelper.ApplyIcon(this);

            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(12)
            };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F)
            };

            _bottomBar = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            _btnClose = new Button { Text = "关闭", DialogResult = DialogResult.OK, Width = 100, Height = 32 };

            _bottomBar.Controls.Add(_btnClose);

            rootLayout.Controls.Add(_textBox, 0, 0);
            rootLayout.Controls.Add(_bottomBar, 0, 1);
            Controls.Add(rootLayout);

            _textBox.Text = initialText ?? string.Empty;

            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
            InitToolTips();
        }

        public DailySummaryForm(System.Collections.Generic.IEnumerable<WorkLogItem> items, string existingText)
        {
            Text = "每日总结";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(500, 350);
            Width = 875;
            Height = 625;

            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(12)
            };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F)
            };

            _bottomBar = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            _btnClose = new Button { Text = "关闭", DialogResult = DialogResult.OK, Width = 100, Height = 32 };
            _btnReset = new Button { Text = "重置", Width = 100, Height = 32, Margin = new Padding(0, 0, 8, 0) };
            _btnReset.Click += (s, e) =>
            {
                _textBox.Text = BuildChain(_items);
            };

            _bottomBar.Controls.Add(_btnClose);
            _bottomBar.Controls.Add(_btnReset);

            rootLayout.Controls.Add(_textBox, 0, 0);
            rootLayout.Controls.Add(_bottomBar, 0, 1);
            Controls.Add(rootLayout);

            _items = items ?? Enumerable.Empty<WorkLogItem>();
            _textBox.Text = string.IsNullOrWhiteSpace(existingText) ? BuildChain(_items) : existingText;

            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);
            InitToolTips();
        }

        private void InitToolTips()
        {
            var toolTip = new ToolTip();
            if (_btnClose != null) toolTip.SetToolTip(_btnClose, "关闭窗口并保存更改");
            if (_btnReset != null) toolTip.SetToolTip(_btnReset, "根据今日事项自动重新生成总结");
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