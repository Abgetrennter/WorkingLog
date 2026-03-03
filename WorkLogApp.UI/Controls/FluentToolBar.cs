using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Controls
{
    /// <summary>
    /// Fluent Design 风格的现代化工具栏
    /// 48px 高度，按钮分组，圆角设计
    /// </summary>
    public class FluentToolBar : Panel
    {
        private Panel _leftGroup;
        private Panel _centerGroup;
        private Panel _rightGroup;
        private Panel _separator;
        #pragma warning disable
        private bool _isInitialized = false;

        /// <summary>
        /// 左侧按钮组（主要操作）
        /// </summary>
        public Control LeftGroup => _leftGroup;

        /// <summary>
        /// 中间按钮组（视图切换/筛选）
        /// </summary>
        public Control CenterGroup => _centerGroup;

        /// <summary>
        /// 右侧按钮组（工具/设置）
        /// </summary>
        public Control RightGroup => _rightGroup;

        public FluentToolBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 基础样式 - Fluent 工具栏标准高度 48px
            this.Height = 56; // 48px + 上下 padding
            this.Dock = DockStyle.Top;
            this.BackColor = FluentColors.Gray10;
            this.Padding = new Padding(16, 4, 16, 4);

            // 左侧按钮组容器
            _leftGroup = new Panel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // 中间按钮组容器
            _centerGroup = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // 右侧按钮组容器
            _rightGroup = new Panel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // 分隔线 - 底部 1px
            _separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = FluentColors.Gray40
            };

            // 创建内部布局容器
            var innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Height = 48
            };

            innerPanel.Controls.Add(_centerGroup);
            innerPanel.Controls.Add(_rightGroup);
            innerPanel.Controls.Add(_leftGroup);

            this.Controls.Add(_separator);
            this.Controls.Add(innerPanel);

            _isInitialized = true;
        }

        /// <summary>
        /// 创建主要操作按钮（蓝色背景）
        /// </summary>
        public Button CreatePrimaryButton(string text, Image icon = null)
        {
            var btn = new Button
            {
                Text = text,
                Image = icon,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Height = 32,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12, 0, 12, 0),
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Font = FluentTypography.Button,
                ForeColor = Color.White,
                BackColor = FluentColors.Primary
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = FluentColors.PrimaryHover;
            btn.FlatAppearance.MouseDownBackColor = FluentColors.PrimaryPressed;

            // 应用圆角
            FluentStyleManager.ApplyRoundedCorners(btn, 4);

            return btn;
        }

        /// <summary>
        /// 创建次要操作按钮（白色/灰色背景）
        /// </summary>
        public Button CreateSecondaryButton(string text, Image icon = null)
        {
            var btn = new Button
            {
                Text = text,
                Image = icon,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Height = 32,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12, 0, 12, 0),
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Font = FluentTypography.Button,
                ForeColor = FluentColors.Gray160,
                BackColor = FluentColors.Gray20
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = FluentColors.Gray30;
            btn.FlatAppearance.MouseDownBackColor = FluentColors.Gray40;

            // 应用圆角
            FluentStyleManager.ApplyRoundedCorners(btn, 4);

            return btn;
        }

        /// <summary>
        /// 创建幽灵按钮（透明背景）
        /// </summary>
        public Button CreateGhostButton(string text, Image icon = null)
        {
            var btn = new Button
            {
                Text = text,
                Image = icon,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Height = 32,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12, 0, 12, 0),
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Font = FluentTypography.Button,
                ForeColor = FluentColors.Gray150,
                BackColor = Color.Transparent
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = FluentColors.Gray20;
            btn.FlatAppearance.MouseDownBackColor = FluentColors.Gray30;

            return btn;
        }

        /// <summary>
        /// 添加分隔条
        /// </summary>
        public void AddSeparator(Control group)
        {
            var separator = new Panel
            {
                Width = 1,
                Height = 24,
                BackColor = FluentColors.Gray40,
                Margin = new Padding(8, 4, 8, 4),
                Dock = DockStyle.Left
            };
            group.Controls.Add(separator);
        }

        /// <summary>
        /// 添加按钮到左侧组
        /// </summary>
        public void AddToLeft(Button button, bool addSeparator = false)
        {
            if (addSeparator && _leftGroup.Controls.Count > 0)
            {
                AddSeparator(_leftGroup);
            }
            button.Dock = DockStyle.Left;
            _leftGroup.Controls.Add(button);
        }

        /// <summary>
        /// 添加按钮到中间组
        /// </summary>
        public void AddToCenter(Button button, bool addSeparator = false)
        {
            if (addSeparator && _centerGroup.Controls.Count > 0)
            {
                AddSeparator(_centerGroup);
            }
            button.Dock = DockStyle.Left;
            _centerGroup.Controls.Add(button);
        }

        /// <summary>
        /// 添加按钮到右侧组
        /// </summary>
        public void AddToRight(Button button, bool addSeparator = false)
        {
            if (addSeparator && _rightGroup.Controls.Count > 0)
            {
                AddSeparator(_rightGroup);
            }
            button.Dock = DockStyle.Right;
            _rightGroup.Controls.Add(button);
        }

        /// <summary>
        /// 添加控件到左侧组
        /// </summary>
        public void AddToLeft(Control control)
        {
            control.Dock = DockStyle.Left;
            _leftGroup.Controls.Add(control);
        }

        /// <summary>
        /// 添加控件到中间组
        /// </summary>
        public void AddToCenter(Control control)
        {
            control.Dock = DockStyle.Left;
            _centerGroup.Controls.Add(control);
        }

        /// <summary>
        /// 添加控件到右侧组
        /// </summary>
        public void AddToRight(Control control)
        {
            control.Dock = DockStyle.Right;
            _rightGroup.Controls.Add(control);
        }
    }
}
