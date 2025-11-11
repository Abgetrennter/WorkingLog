using System.Windows.Forms;

namespace WorkLogApp.UI.Forms
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "工作日志 - 主界面";
            Width = 900;
            Height = 600;

            var label = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = "项目已初始化。请后续完善功能模块。"
            };
            Controls.Add(label);
        }
    }
}