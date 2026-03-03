using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WorkLogApp.UI.UI
{
    /// <summary>
    /// Microsoft Fluent Design System 样式管理器
    /// 提供现代化的 WinUI 风格主题支持
    /// </summary>
    public static class FluentStyleManager
    {
        #region Theme Application - 主题应用

        /// <summary>
        /// 应用 Fluent Design 主题到整个表单
        /// </summary>
        public static void ApplyFluentTheme(Form form)
        {
            if (form == null) return;

            // 设置表单基本样式
            form.BackColor = FluentColors.Gray10;
            form.Font = FluentTypography.Body;
            form.AutoScaleMode = AutoScaleMode.Dpi;

            // 启用双缓冲减少闪烁
            TryEnableDoubleBuffer(form);

            // 递归应用样式到所有控件
            ApplyFluentThemeToControlTree(form);
        }

        /// <summary>
        /// 递归应用 Fluent 主题到控件树
        /// </summary>
        private static void ApplyFluentThemeToControlTree(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                ApplyFluentStyle(ctrl);

                if (ctrl.HasChildren)
                {
                    ApplyFluentThemeToControlTree(ctrl);
                }
            }
        }

        /// <summary>
        /// 应用 Fluent 样式到单个控件
        /// </summary>
        public static void ApplyFluentStyle(Control ctrl)
        {
            try
            {
                if (ctrl is Button btn)
                {
                    ApplyFluentButtonStyle(btn);
                }
                else if (ctrl is RichTextBox rtb)
                {
                    ApplyFluentRichTextBoxStyle(rtb);
                }
                else if (ctrl is TextBoxBase txt)
                {
                    ApplyFluentTextBoxStyle(txt);
                }
                else if (ctrl is ComboBox cb)
                {
                    ApplyFluentComboBoxStyle(cb);
                }
                else if (ctrl is ListView lv)
                {
                    ApplyFluentListViewStyle(lv);
                }
                else if (ctrl is Label lbl)
                {
                    ApplyFluentLabelStyle(lbl);
                }
                else if (ctrl is Panel pnl)
                {
                    ApplyFluentPanelStyle(pnl);
                }
                else if (ctrl is DateTimePicker dtp)
                {
                    ApplyFluentDatePickerStyle(dtp);
                }
                else if (ctrl is CheckBox chk)
                {
                    ApplyFluentCheckBoxStyle(chk);
                }
                else if (ctrl is DataGridView dgv)
                {
                    ApplyFluentDataGridViewStyle(dgv);
                }
                else if (ctrl is TreeView tv)
                {
                    ApplyFluentTreeViewStyle(tv);
                }

                TryEnableDoubleBuffer(ctrl);
            }
            catch { }
        }

        #endregion

        #region Button Styles - 按钮样式

        /// <summary>
        /// 应用 Fluent 按钮样式
        /// </summary>
        /// <param name="btn">按钮控件</param>
        /// <param name="isPrimary">是否为主要按钮</param>
        public static void ApplyFluentButtonStyle(Button btn, bool isPrimary = false)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.Height = 36;
            btn.Padding = new Padding(16, 0, 16, 0);
            btn.Font = FluentTypography.Body;
            btn.Cursor = Cursors.Hand;

            // 检查 Tag 属性确定按钮类型
            var tagStr = btn.Tag as string;
            if (!string.IsNullOrWhiteSpace(tagStr))
            {
                if (tagStr.ToLowerInvariant().Contains("primary"))
                    isPrimary = true;
            }

            if (isPrimary)
            {
                // Primary 按钮样式 (蓝色背景，白色文字)
                btn.BackColor = FluentColors.Primary;
                btn.ForeColor = FluentColors.White;
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = FluentColors.PrimaryLight;
                btn.FlatAppearance.MouseDownBackColor = FluentColors.PrimaryDark;
            }
            else
            {
                // Secondary 按钮样式 (白色背景，灰色边框)
                btn.BackColor = FluentColors.White;
                btn.ForeColor = FluentColors.Gray190;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = FluentColors.Gray80;
                btn.FlatAppearance.MouseOverBackColor = FluentColors.Gray20;
                btn.FlatAppearance.MouseDownBackColor = FluentColors.Gray40;
            }

            // 应用圆角
            ApplyRoundedCorners(btn, 4);
        }

        /// <summary>
        /// 应用主要按钮样式（蓝色强调）
        /// </summary>
        public static void ApplyPrimaryButtonStyle(Button btn)
        {
            ApplyFluentButtonStyle(btn, true);
        }

        /// <summary>
        /// 应用次要按钮样式（白色边框）
        /// </summary>
        public static void ApplySecondaryButtonStyle(Button btn)
        {
            ApplyFluentButtonStyle(btn, false);
        }

        /// <summary>
        /// 应用幽灵按钮样式（透明背景）
        /// </summary>
        public static void ApplyGhostButtonStyle(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.Height = 36;
            btn.Padding = new Padding(12, 0, 12, 0);
            btn.Font = FluentTypography.Body;
            btn.Cursor = Cursors.Hand;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = FluentColors.Gray130;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = FluentColors.Gray20;
            btn.FlatAppearance.MouseDownBackColor = FluentColors.Gray40;
        }

        #endregion

        #region Input Styles - 输入控件样式

        /// <summary>
        /// 应用 Fluent 文本框样式
        /// </summary>
        private static void ApplyFluentTextBoxStyle(TextBoxBase txt)
        {
            if (txt == null) return;

            txt.Font = FluentTypography.Body;
            txt.BackColor = FluentColors.Gray10;
            txt.ForeColor = FluentColors.Gray190;
            txt.BorderStyle = BorderStyle.FixedSingle;

            // 如果是单行文本框，设置高度
            if (txt is TextBox tb && !tb.Multiline)
            {
                tb.Height = 36;
            }
        }

        /// <summary>
        /// 应用 Fluent 下拉框样式
        /// </summary>
        private static void ApplyFluentComboBoxStyle(ComboBox cb)
        {
            if (cb == null) return;

            cb.Font = FluentTypography.Body;
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = FluentColors.White;
            cb.ForeColor = FluentColors.Gray190;
            cb.Height = 36;
        }

        /// <summary>
        /// 应用 Fluent 日期选择器样式
        /// </summary>
        private static void ApplyFluentDatePickerStyle(DateTimePicker dtp)
        {
            if (dtp == null) return;

            dtp.Font = FluentTypography.Body;
            dtp.CalendarForeColor = FluentColors.Gray190;
            dtp.CalendarMonthBackground = FluentColors.White;
            dtp.CalendarTitleBackColor = FluentColors.Primary;
            dtp.CalendarTitleForeColor = FluentColors.White;
            dtp.CalendarTrailingForeColor = FluentColors.Gray100;
        }

        /// <summary>
        /// 应用 Fluent 复选框样式
        /// </summary>
        private static void ApplyFluentCheckBoxStyle(CheckBox chk)
        {
            if (chk == null) return;

            chk.Font = FluentTypography.Body;
            chk.ForeColor = FluentColors.Gray190;
            chk.BackColor = Color.Transparent;
        }

        /// <summary>
        /// 应用 Fluent 富文本框样式
        /// </summary>
        private static void ApplyFluentRichTextBoxStyle(RichTextBox rtb)
        {
            if (rtb == null) return;

            rtb.Font = FluentTypography.Body;
            rtb.BackColor = FluentColors.White;
            rtb.ForeColor = FluentColors.Gray190;
            rtb.BorderStyle = BorderStyle.FixedSingle;
        }

        #endregion

        #region Data Display Styles - 数据显示样式

        /// <summary>
        /// 应用 Fluent 列表视图样式
        /// </summary>
        private static void ApplyFluentListViewStyle(ListView lv)
        {
            if (lv == null) return;

            lv.Font = FluentTypography.Body;
            lv.BackColor = FluentColors.Gray10;
            lv.ForeColor = FluentColors.Gray190;
            lv.BorderStyle = BorderStyle.None;
            lv.GridLines = false;
            lv.FullRowSelect = true;
            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        /// <summary>
        /// 应用 Fluent 数据表格样式
        /// </summary>
        private static void ApplyFluentDataGridViewStyle(DataGridView dgv)
        {
            if (dgv == null) return;

            dgv.Font = FluentTypography.Body;
            dgv.BackgroundColor = FluentColors.Gray10;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = FluentColors.Gray40;

            // 默认单元格样式
            dgv.DefaultCellStyle.BackColor = FluentColors.White;
            dgv.DefaultCellStyle.ForeColor = FluentColors.Gray190;
            dgv.DefaultCellStyle.SelectionBackColor = FluentColors.PrimaryLighter;
            dgv.DefaultCellStyle.SelectionForeColor = FluentColors.Gray190;
            dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            // 列头样式
            dgv.ColumnHeadersDefaultCellStyle.BackColor = FluentColors.Gray20;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = FluentColors.Gray160;
            dgv.ColumnHeadersDefaultCellStyle.Font = FluentTypography.Subtitle;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            dgv.ColumnHeadersHeight = 40;
            dgv.EnableHeadersVisualStyles = false;

            // 行样式
            dgv.RowTemplate.Height = 44;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = FluentColors.Gray10;
        }

        /// <summary>
        /// 应用 Fluent 树视图样式
        /// </summary>
        private static void ApplyFluentTreeViewStyle(TreeView tv)
        {
            if (tv == null) return;

            tv.Font = FluentTypography.Body;
            tv.BackColor = FluentColors.Gray10;
            tv.ForeColor = FluentColors.Gray190;
            tv.BorderStyle = BorderStyle.None;
            tv.FullRowSelect = true;
            tv.HideSelection = false;
            tv.LineColor = FluentColors.Gray60;
        }

        #endregion

        #region Layout Styles - 布局样式

        /// <summary>
        /// 应用 Fluent 标签样式
        /// </summary>
        private static void ApplyFluentLabelStyle(Label lbl)
        {
            if (lbl == null) return;

            lbl.ForeColor = FluentColors.Gray190;
            lbl.BackColor = Color.Transparent;
            lbl.UseCompatibleTextRendering = false;

            // 根据 Tag 应用标题样式
            ApplyFluentHeadingIfTagged(lbl);
        }

        /// <summary>
        /// 根据 Tag 应用标题字体
        /// </summary>
        private static void ApplyFluentHeadingIfTagged(Label lbl)
        {
            var tag = lbl.Tag as string;
            if (string.IsNullOrWhiteSpace(tag)) return;

            switch (tag.Trim().ToLowerInvariant())
            {
                case "h1":
                case "header":
                    lbl.Font = FluentTypography.Header;
                    lbl.ForeColor = FluentColors.Gray190;
                    break;
                case "h2":
                case "title-large":
                    lbl.Font = FluentTypography.TitleLarge;
                    lbl.ForeColor = FluentColors.Gray190;
                    break;
                case "h3":
                case "title":
                    lbl.Font = FluentTypography.Title;
                    lbl.ForeColor = FluentColors.Gray190;
                    break;
                case "h4":
                case "subtitle":
                    lbl.Font = FluentTypography.Subtitle;
                    lbl.ForeColor = FluentColors.Gray160;
                    break;
                case "caption":
                    lbl.Font = FluentTypography.Caption;
                    lbl.ForeColor = FluentColors.Gray130;
                    break;
            }
        }

        /// <summary>
        /// 应用 Fluent 面板样式
        /// </summary>
        private static void ApplyFluentPanelStyle(Panel pnl)
        {
            if (pnl == null) return;

            // 根据 Dock 属性决定背景色
            if (pnl.Dock == DockStyle.Top || pnl.Dock == DockStyle.Bottom)
            {
                pnl.BackColor = FluentColors.White;
            }
            else if (pnl.Dock == DockStyle.Left || pnl.Dock == DockStyle.Right)
            {
                pnl.BackColor = FluentColors.Gray10;
            }
            else
            {
                pnl.BackColor = FluentColors.Gray10;
            }
        }

        /// <summary>
        /// 创建 Fluent 卡片容器
        /// </summary>
        public static Panel CreateFluentCard(int padding = 16)
        {
            var card = new Panel
            {
                BackColor = FluentColors.White,
                Padding = new Padding(padding),
                Margin = new Padding(0, 0, 0, 16)
            };

            // 应用阴影效果
            card.Paint += (s, e) =>
            {
                DrawCardShadow(e.Graphics, card.ClientRectangle);
            };

            return card;
        }

        /// <summary>
        /// 创建 Fluent 分隔线
        /// </summary>
        public static Panel CreateFluentSeparator(bool isVertical = false)
        {
            if (isVertical)
            {
                return new Panel
                {
                    Width = 1,
                    BackColor = FluentColors.Gray40,
                    Dock = DockStyle.Left
                };
            }
            else
            {
                return new Panel
                {
                    Height = 1,
                    BackColor = FluentColors.Gray40,
                    Dock = DockStyle.Top
                };
            }
        }

        #endregion

        #region Helper Methods - 辅助方法

        /// <summary>
        /// 应用圆角到控件
        /// </summary>
        public static void ApplyRoundedCorners(Control ctrl, int radius)
        {
            if (ctrl == null || radius <= 0) return;

            var path = CreateRoundedRectPath(ctrl.ClientRectangle, radius);
            ctrl.Region = new Region(path);

            // 监听大小变化，更新圆角
            ctrl.SizeChanged += (s, e) =>
            {
                var newPath = CreateRoundedRectPath(ctrl.ClientRectangle, radius);
                ctrl.Region = new Region(newPath);
            };
        }

        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        public static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            // 确保半径不超过矩形尺寸的一半
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            radius = d / 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// 绘制卡片阴影
        /// </summary>
        private static void DrawCardShadow(Graphics g, Rectangle rect)
        {
            // Level 1 阴影: 0 2px 4px rgba(0,0,0,0.04)
            using (var path = CreateRoundedRectPath(rect, 4))
            {
                // 这里简化处理，实际可以使用更复杂的阴影绘制
                // 在 WinForms 中完全模拟 Fluent 阴影需要更多工作
            }
        }

        /// <summary>
        /// 启用控件双缓冲
        /// </summary>
        private static void TryEnableDoubleBuffer(Control ctrl)
        {
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                prop?.SetValue(ctrl, true, null);
            }
            catch { }
        }

        #endregion

        #region Spacing Constants - 间距常量

        /// <summary>超小间距 4px</summary>
        public const int SpacingXs = 4;

        /// <summary>小间距 8px</summary>
        public const int SpacingSm = 8;

        /// <summary>中间距 16px</summary>
        public const int SpacingMd = 16;

        /// <summary>大间距 24px</summary>
        public const int SpacingLg = 24;

        /// <summary>超大间距 32px</summary>
        public const int SpacingXl = 32;

        /// <summary>2倍超大 48px</summary>
        public const int Spacing2Xl = 48;

        /// <summary>页面边距</summary>
        public const int PagePadding = 24;

        /// <summary>区块间距</summary>
        public const int SectionGap = 24;

        /// <summary>卡片内边距</summary>
        public const int CardPadding = 16;

        /// <summary>列表项高度</summary>
        public const int ListItemHeight = 48;

        /// <summary>按钮高度</summary>
        public const int ButtonHeight = 36;

        /// <summary>输入框高度</summary>
        public const int InputHeight = 36;

        /// <summary>顶部导航栏高度</summary>
        public const int AppBarHeight = 56;

        /// <summary>工具栏高度</summary>
        public const int ToolBarHeight = 48;

        /// <summary>状态栏高度</summary>
        public const int StatusBarHeight = 40;

        #endregion
    }
}
