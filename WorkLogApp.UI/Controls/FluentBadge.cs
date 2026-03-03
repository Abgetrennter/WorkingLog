using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Controls
{
    /// <summary>
    /// Fluent Design 状态徽章控件
    /// 用于显示状态标签，如待办、进行中、已完成等
    /// </summary>
    public class FluentBadge : Label
    {
        #region Badge Style Enum - 徽章样式枚举

        /// <summary>
        /// 徽章样式类型
        /// </summary>
        public enum BadgeStyle
        {
            /// <summary>默认样式 - 灰色</summary>
            Default,
            /// <summary>成功样式 - 绿色</summary>
            Success,
            /// <summary>警告样式 - 黄色</summary>
            Warning,
            /// <summary>错误样式 - 红色</summary>
            Error,
            /// <summary>信息样式 - 蓝色</summary>
            Info,
            /// <summary>严重样式 - 深红</summary>
            Severe
        }

        #endregion

        #region Private Fields - 私有字段

        private BadgeStyle _style = BadgeStyle.Default;
        private int _borderRadius = 12;
        private bool _hasBorder = false;

        #endregion

        #region Public Properties - 公共属性

        /// <summary>
        /// 徽章样式
        /// </summary>
        public BadgeStyle Style
        {
            get => _style;
            set
            {
                if (_style != value)
                {
                    _style = value;
                    UpdateAppearance();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// 是否显示边框
        /// </summary>
        public bool HasBorder
        {
            get => _hasBorder;
            set
            {
                _hasBorder = value;
                Invalidate();
            }
        }

        #endregion

        #region Constructor - 构造函数

        /// <summary>
        /// 创建 FluentBadge 实例
        /// </summary>
        public FluentBadge()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建指定样式的 FluentBadge 实例
        /// </summary>
        public FluentBadge(BadgeStyle style)
        {
            _style = style;
            InitializeComponent();
            UpdateAppearance();
        }

        private void InitializeComponent()
        {
            // 基本设置
            AutoSize = false;
            Height = 24;
            Padding = new Padding(8, 2, 8, 2);
            TextAlign = ContentAlignment.MiddleCenter;
            Font = FluentTypography.Caption;
            Cursor = Cursors.Default;

            // 初始外观
            UpdateAppearance();
        }

        #endregion

        #region Appearance Update - 外观更新

        /// <summary>
        /// 根据当前样式更新外观
        /// </summary>
        private void UpdateAppearance()
        {
            switch (_style)
            {
                case BadgeStyle.Success:
                    BackColor = FluentColors.SuccessLight;
                    ForeColor = FluentColors.Success;
                    break;

                case BadgeStyle.Warning:
                    BackColor = FluentColors.WarningLight;
                    ForeColor = Color.FromArgb(138, 87, 0); // Dark yellow for better contrast
                    break;

                case BadgeStyle.Error:
                    BackColor = FluentColors.ErrorLight;
                    ForeColor = FluentColors.Error;
                    break;

                case BadgeStyle.Info:
                    BackColor = FluentColors.PrimaryLighter;
                    ForeColor = FluentColors.Primary;
                    break;

                case BadgeStyle.Severe:
                    BackColor = Color.FromArgb(253, 231, 233);
                    ForeColor = Color.FromArgb(168, 0, 0);
                    break;

                case BadgeStyle.Default:
                default:
                    BackColor = FluentColors.Gray20;
                    ForeColor = FluentColors.Gray130;
                    break;
            }

            Invalidate();
        }

        #endregion

        #region Paint Override - 绘制重写

        /// <summary>
        /// 重写绘制方法
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 绘制背景
            using (var path = GetRoundedRectPath(ClientRectangle, _borderRadius))
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillPath(brush, path);

                // 绘制边框（如果需要）
                if (_hasBorder)
                {
                    using (var pen = new Pen(ForeColor, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // 绘制文字
            if (!string.IsNullOrEmpty(Text))
            {
                var textRect = new Rectangle(
                    Padding.Left,
                    Padding.Top,
                    Width - Padding.Horizontal,
                    Height - Padding.Vertical
                );

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                );
            }
        }

        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            // 确保半径不超过矩形尺寸的一半
            int diameter = radius * 2;
            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;
            radius = diameter / 2;

            // 如果半径为0，绘制普通矩形
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // 绘制圆角矩形
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion

        #region Size Override - 尺寸重写

        /// <summary>
        /// 重写尺寸设置以确保最小高度
        /// </summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            var size = base.GetPreferredSize(proposedSize);
            size.Height = Math.Max(size.Height, 24);

            // 根据文字计算宽度
            if (!string.IsNullOrEmpty(Text))
            {
                using (var g = CreateGraphics())
                {
                    var textSize = TextRenderer.MeasureText(g, Text, Font);
                    size.Width = textSize.Width + Padding.Horizontal + 8; // 额外边距
                }
            }

            return size;
        }

        #endregion

        #region Static Factory Methods - 静态工厂方法

        /// <summary>
        /// 创建成功状态徽章
        /// </summary>
        public static FluentBadge Success(string text)
        {
            return new FluentBadge(BadgeStyle.Success) { Text = text };
        }

        /// <summary>
        /// 创建警告状态徽章
        /// </summary>
        public static FluentBadge Warning(string text)
        {
            return new FluentBadge(BadgeStyle.Warning) { Text = text };
        }

        /// <summary>
        /// 创建错误状态徽章
        /// </summary>
        public static FluentBadge Error(string text)
        {
            return new FluentBadge(BadgeStyle.Error) { Text = text };
        }

        /// <summary>
        /// 创建信息状态徽章
        /// </summary>
        public static FluentBadge Info(string text)
        {
            return new FluentBadge(BadgeStyle.Info) { Text = text };
        }

        /// <summary>
        /// 创建默认状态徽章
        /// </summary>
        public static FluentBadge Default(string text)
        {
            return new FluentBadge(BadgeStyle.Default) { Text = text };
        }

        #endregion

        #region Status Helper - 状态辅助

        /// <summary>
        /// 根据状态字符串创建对应样式的徽章
        /// </summary>
        public static FluentBadge FromStatus(string status)
        {
            var badge = new FluentBadge();

            switch (status?.ToLowerInvariant())
            {
                case "done":
                case "完成":
                case "已完成":
                case "success":
                    badge.Style = BadgeStyle.Success;
                    badge.Text = "✓ " + status;
                    break;

                case "doing":
                case "进行中":
                case "处理中":
                case "warning":
                    badge.Style = BadgeStyle.Warning;
                    badge.Text = "● " + status;
                    break;

                case "todo":
                case "待办":
                case "待处理":
                case "default":
                default:
                    badge.Style = BadgeStyle.Default;
                    badge.Text = "○ " + status;
                    break;

                case "error":
                case "失败":
                case "错误":
                    badge.Style = BadgeStyle.Error;
                    badge.Text = "✗ " + status;
                    break;
            }

            return badge;
        }

        #endregion
    }
}
