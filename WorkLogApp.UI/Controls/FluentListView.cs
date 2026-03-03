using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Controls
{
    /// <summary>
    /// Fluent Design 风格的现代化 ListView
    /// 48px 行高，悬停效果，选中状态，圆角选择框
    /// </summary>
    public class FluentListView : ListView
    {
        private int _rowHeight = 48;
        private Color _hoverBackColor = FluentColors.Gray20;
        private Color _selectedBackColor = FluentColors.PrimaryLighter;
        private Color _selectedBorderColor = FluentColors.Primary;
        private int _hoveredItemIndex = -1;
        private bool _useFluentRendering = true;

        /// <summary>
        /// 行高度（默认 48px）
        /// </summary>
        public int RowHeight
        {
            get => _rowHeight;
            set
            {
                _rowHeight = value;
                SetupImageList();
                Invalidate();
            }
        }

        /// <summary>
        /// 是否使用 Fluent 渲染样式
        /// </summary>
        public bool UseFluentRendering
        {
            get => _useFluentRendering;
            set
            {
                _useFluentRendering = value;
                Invalidate();
            }
        }

        public FluentListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 基础样式
            this.View = View.Details;
            this.FullRowSelect = true;
            this.GridLines = false; // Fluent 风格不使用网格线
            this.HideSelection = false;
            this.DoubleBuffered = true;
            this.BackColor = FluentColors.Gray10;
            this.ForeColor = FluentColors.Gray160;
            this.Font = FluentTypography.Body;
            
            // 设置 OwnerDraw 以实现自定义渲染
            this.OwnerDraw = true;
            
            // 初始化 ImageList 来控制行高
            SetupImageList();
            
            // 绑定事件
            this.DrawColumnHeader += OnDrawColumnHeader;
            this.DrawItem += OnDrawItem;
            this.DrawSubItem += OnDrawSubItem;
            this.MouseMove += OnMouseMove;
            this.MouseLeave += OnMouseLeave;
        }

        private void SetupImageList()
        {
            // 使用 ImageList 来设置行高
            var imgList = new ImageList
            {
                ImageSize = new Size(1, _rowHeight),
                ColorDepth = ColorDepth.Depth32Bit
            };
            this.SmallImageList = imgList;
        }

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (!_useFluentRendering)
            {
                e.DrawDefault = true;
                return;
            }

            // 绘制表头背景
            using (var brush = new SolidBrush(FluentColors.Gray10))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // 绘制表头文字
            var textRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y, e.Bounds.Width - 24, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, FluentTypography.Caption, 
                textRect, FluentColors.Gray130, 
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // 绘制底部边框
            using (var pen = new Pen(FluentColors.Gray40))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (!_useFluentRendering)
            {
                e.DrawDefault = true;
                return;
            }

            // 绘制项背景
            DrawItemBackground(e.Graphics, e.Bounds, e.Item.Index);
        }

        private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (!_useFluentRendering)
            {
                e.DrawDefault = true;
                return;
            }

            // 如果正在绘制第一列，背景已经在 DrawItem 中绘制
            // 如果是状态列，使用徽章样式
            if (e.ColumnIndex == 2) // 假设第3列是状态列
            {
                DrawStatusBadge(e.Graphics, e.Bounds, e.SubItem.Text);
            }
            else
            {
                // 普通文本列
                var textColor = e.Item.Selected ? FluentColors.Gray160 : FluentColors.Gray160;
                var padding = e.ColumnIndex == 0 ? 12 : 8;
                var textRect = new Rectangle(e.Bounds.X + padding, e.Bounds.Y, e.Bounds.Width - padding * 2, e.Bounds.Height);
                
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, this.Font,
                    textRect, textColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawItemBackground(Graphics g, Rectangle bounds, int itemIndex)
        {
            Color backColor;
            
            if (this.SelectedIndices.Contains(itemIndex))
            {
                backColor = _selectedBackColor;
            }
            else if (itemIndex == _hoveredItemIndex)
            {
                backColor = _hoverBackColor;
            }
            else
            {
                backColor = itemIndex % 2 == 0 ? FluentColors.Gray10 : FluentColors.Gray20;
            }

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 绘制选中边框
            if (this.SelectedIndices.Contains(itemIndex))
            {
                using (var pen = new Pen(_selectedBorderColor, 2))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }

                // 绘制左侧选中指示条
                using (var pen = new Pen(_selectedBorderColor, 3))
                {
                    g.DrawLine(pen, bounds.X, bounds.Y + 4, bounds.X, bounds.Bottom - 4);
                }
            }
        }

        private void DrawStatusBadge(Graphics g, Rectangle bounds, string status)
        {
            var badgeStyle = GetBadgeStyleFromStatus(status);
            var badgeColors = GetBadgeColors(badgeStyle);
            
            var badgeWidth = Math.Min(80, bounds.Width - 16);
            var badgeRect = new Rectangle(bounds.X + 8, bounds.Y + (bounds.Height - 20) / 2, badgeWidth, 20);

            // 绘制圆角背景
            using (var brush = new SolidBrush(badgeColors.Background))
            using (var path = CreateRoundedRectPath(badgeRect, 10))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(brush, path);
            }

            // 绘制状态文字
            TextRenderer.DrawText(g, status, FluentTypography.Caption,
                badgeRect, badgeColors.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private BadgeStyle GetBadgeStyleFromStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return BadgeStyle.Default;
            
            var s = status.ToLowerInvariant();
            if (s.Contains("完成") || s.Contains("done")) return BadgeStyle.Success;
            if (s.Contains("进行") || s.Contains("doing")) return BadgeStyle.Warning;
            if (s.Contains("待办") || s.Contains("todo")) return BadgeStyle.Default;
            if (s.Contains("错误") || s.Contains("error")) return BadgeStyle.Error;
            
            return BadgeStyle.Default;
        }

        private (Color Background, Color Text) GetBadgeColors(BadgeStyle style)
        {
            switch (style)
            {
                case BadgeStyle.Success:
                    return (FluentColors.SuccessLight, FluentColors.Success);
                case BadgeStyle.Warning:
                    return (FluentColors.WarningLight, FluentColors.Warning);
                case BadgeStyle.Error:
                    return (FluentColors.ErrorLight, FluentColors.Error);
                case BadgeStyle.Info:
                    return (FluentColors.InfoLight, FluentColors.Info);
                default:
                    return (FluentColors.Gray20, FluentColors.Gray130);
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_useFluentRendering) return;

            var item = this.GetItemAt(0, e.Y);
            int newHoverIndex = item?.Index ?? -1;

            if (newHoverIndex != _hoveredItemIndex)
            {
                int oldHoverIndex = _hoveredItemIndex;
                _hoveredItemIndex = newHoverIndex;

                // 重绘受影响的行
                if (oldHoverIndex >= 0 && oldHoverIndex < this.Items.Count)
                {
                    this.Invalidate(this.Items[oldHoverIndex].Bounds);
                }
                if (_hoveredItemIndex >= 0 && _hoveredItemIndex < this.Items.Count)
                {
                    this.Invalidate(this.Items[_hoveredItemIndex].Bounds);
                }
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (_hoveredItemIndex >= 0)
            {
                int oldHoverIndex = _hoveredItemIndex;
                _hoveredItemIndex = -1;
                
                if (oldHoverIndex >= 0 && oldHoverIndex < this.Items.Count)
                {
                    this.Invalidate(this.Items[oldHoverIndex].Bounds);
                }
            }
        }

        /// <summary>
        /// 添加带有状态徽章的行
        /// </summary>
        public void AddFluentItem(string[] subItems, string status = null)
        {
            var item = new ListViewItem(subItems);
            if (!string.IsNullOrEmpty(status) && subItems.Length > 2)
            {
                item.SubItems[2].Text = status;
            }
            this.Items.Add(item);
        }
    }

    public enum BadgeStyle
    {
        Default,
        Success,
        Warning,
        Error,
        Info,
        Severe
    }
}
