using System.Windows.Forms;

namespace WorkLogApp.UI.UI
{
    /// <summary>
    /// Fluent Design 主题集成辅助类
    /// 提供快速应用主题的方法
    /// </summary>
    public static class FluentIntegration
    {
        /// <summary>
        /// 为主窗体应用 Fluent 主题
        /// </summary>
        public static void ApplyToMainForm(Form form, params Button[] buttons)
        {
            if (form == null) return;

            // 应用全局主题
            FluentStyleManager.ApplyFluentTheme(form);

            // 为按钮应用特定样式
            if (buttons != null)
            {
                foreach (var btn in buttons)
                {
                    if (btn == null) continue;

                    // 根据按钮名称或Tag判断类型
                    var tag = btn.Tag as string;
                    var name = btn.Name.ToLowerInvariant();

                    if (name.Contains("create") || name.Contains("save") || name.Contains("add") ||
                        tag?.ToLowerInvariant().Contains("primary") == true)
                    {
                        FluentStyleManager.ApplyPrimaryButtonStyle(btn);
                    }
                    else
                    {
                        FluentStyleManager.ApplySecondaryButtonStyle(btn);
                    }
                }
            }
        }

        /// <summary>
        /// 为对话框应用 Fluent 主题
        /// </summary>
        public static void ApplyToDialog(Form form)
        {
            if (form == null) return;

            form.BackColor = FluentColors.Gray10;
            form.Font = FluentTypography.Body;
            form.AutoScaleMode = AutoScaleMode.Dpi;

            // ApplyFluentTheme 已经包含递归处理控件树
        }

        /// <summary>
        /// 为表单应用 Fluent 主题
        /// </summary>
        public static void ApplyToForm(Form form)
        {
            FluentStyleManager.ApplyFluentTheme(form);
        }
    }
}
