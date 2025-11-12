using System;
using System.Drawing;
using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    public class DailySummaryForm : Form
    {
        private TextBox _textBox;
        private FlowLayoutPanel _bottomBar;
        private Button _btnCopy;
        private Button _btnClose;

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
            _btnCopy = new Button { Text = "复制", Width = 100, Height = 32 };
            _btnCopy.Click += (s, e) =>
            {
                try { Clipboard.SetText(_textBox.Text ?? string.Empty); }
                catch { }
            };

            _bottomBar.Controls.Add(_btnClose);
            _bottomBar.Controls.Add(_btnCopy);

            Controls.Add(_textBox);
            Controls.Add(_bottomBar);

            _textBox.Text = initialText ?? string.Empty;
        }

        public string SummaryText => _textBox.Text;
    }
}