using System;
using System.Windows.Forms;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Forms
{
    public class MainForm : Form
    {
        private readonly ITemplateService _templateService;

        public MainForm(ITemplateService templateService)
        {
            _templateService = templateService;
            Text = "工作日志 - 主界面";
            Width = 900;
            Height = 600;

            var btnCreate = new Button
            {
                Text = "创建事项",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnCreate.Click += OnCreateItemClick;

            var info = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = "请选择‘创建事项’以基于模板生成日志。"
            };

            Controls.Add(info);
            Controls.Add(btnCreate);
        }

        private void OnCreateItemClick(object sender, EventArgs e)
        {
            using (var form = new ItemCreateForm(_templateService))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
            }
        }
    }
}