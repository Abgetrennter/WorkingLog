using System.Drawing;

namespace WorkLogApp.UI.UI
{
    /// <summary>
    /// Microsoft Fluent Design System 色彩常量
    /// 遵循 WinUI 3 设计规范
    /// </summary>
    public static class FluentColors
    {
        #region Primary - 品牌主色

        /// <summary>主色调 - Azure Blue</summary>
        public static readonly Color Primary = Color.FromArgb(0, 120, 212);
        
        /// <summary>主色亮色 - Hover状态</summary>
        public static readonly Color PrimaryLight = Color.FromArgb(79, 161, 228);
        
        /// <summary>主色悬停状态</summary>
        public static readonly Color PrimaryHover = Color.FromArgb(16, 110, 190);
        
        /// <summary>主色按下状态</summary>
        public static readonly Color PrimaryPressed = Color.FromArgb(0, 90, 158);
        
        /// <summary>主色暗色 - Pressed状态</summary>
        public static readonly Color PrimaryDark = Color.FromArgb(0, 90, 158);
        
        /// <summary>主色浅色背景</summary>
        public static readonly Color PrimaryLighter = Color.FromArgb(229, 241, 250);
        
        /// <summary>主色最浅背景</summary>
        public static readonly Color PrimaryLightest = Color.FromArgb(239, 246, 252);

        #endregion

        #region Neutral Grays - 中性色阶

        /// <summary>Gray190 - 主要文本</summary>
        public static readonly Color Gray190 = Color.FromArgb(32, 31, 30);
        
        /// <summary>Gray160 - 次要文本</summary>
        public static readonly Color Gray160 = Color.FromArgb(50, 49, 48);
        
        /// <summary>Gray150 - 辅助文本</summary>
        public static readonly Color Gray150 = Color.FromArgb(66, 65, 64);
        
        /// <summary>Gray130 - 第三级文本</summary>
        public static readonly Color Gray130 = Color.FromArgb(96, 94, 92);
        
        /// <summary>Gray100 - 禁用/占位文本</summary>
        public static readonly Color Gray100 = Color.FromArgb(138, 136, 134);
        
        /// <summary>Gray80 - 边框悬停色</summary>
        public static readonly Color Gray80 = Color.FromArgb(200, 198, 196);
        
        /// <summary>Gray60 - 默认边框色</summary>
        public static readonly Color Gray60 = Color.FromArgb(200, 198, 196);
        
        /// <summary>Gray40 - 分割线</summary>
        public static readonly Color Gray40 = Color.FromArgb(225, 223, 221);
        
        /// <summary>Gray30 - 悬停背景</summary>
        public static readonly Color Gray30 = Color.FromArgb(237, 235, 233);
        
        /// <summary>Gray20 - 交替背景</summary>
        public static readonly Color Gray20 = Color.FromArgb(243, 242, 241);
        
        /// <summary>Gray10 - 页面背景</summary>
        public static readonly Color Gray10 = Color.FromArgb(250, 249, 248);
        
        /// <summary>White - 纯白表面</summary>
        public static readonly Color White = Color.White;
        
        /// <summary>Black - 纯黑</summary>
        public static readonly Color Black = Color.Black;

        #endregion

        #region Semantic Colors - 语义色

        /// <summary>成功 - 绿色</summary>
        public static readonly Color Success = Color.FromArgb(16, 124, 16);
        
        /// <summary>成功浅色背景</summary>
        public static readonly Color SuccessLight = Color.FromArgb(223, 246, 221);
        
        /// <summary>警告 - 黄色</summary>
        public static readonly Color Warning = Color.FromArgb(255, 185, 0);
        
        /// <summary>警告浅色背景</summary>
        public static readonly Color WarningLight = Color.FromArgb(255, 244, 206);
        
        /// <summary>错误 - 红色</summary>
        public static readonly Color Error = Color.FromArgb(209, 52, 56);
        
        /// <summary>错误浅色背景</summary>
        public static readonly Color ErrorLight = Color.FromArgb(253, 231, 233);
        
        /// <summary>信息 - 蓝色</summary>
        public static readonly Color Info = Color.FromArgb(0, 120, 212);
        
        /// <summary>信息浅色背景</summary>
        public static readonly Color InfoLight = Color.FromArgb(230, 243, 252);

        #endregion

        #region Elevation Shadows - 阴影色

        /// <summary>Level 1 阴影</summary>
        public static readonly Color Shadow1 = Color.FromArgb(10, 0, 0, 0);
        
        /// <summary>Level 2 阴影</summary>
        public static readonly Color Shadow2 = Color.FromArgb(20, 0, 0, 0);
        
        /// <summary>Level 3 阴影</summary>
        public static readonly Color Shadow3 = Color.FromArgb(30, 0, 0, 0);
        
        /// <summary>Level 4 阴影</summary>
        public static readonly Color Shadow4 = Color.FromArgb(40, 0, 0, 0);

        #endregion

        #region Helper Methods - 辅助方法

        /// <summary>
        /// 根据状态获取对应的徽章背景色
        /// </summary>
        public static Color GetBadgeBackgroundColor(string status)
        {
            switch (status?.ToLowerInvariant())
            {
                case "done":
                case "完成":
                case "已完成":
                    return SuccessLight;
                case "doing":
                case "进行中":
                    return WarningLight;
                case "todo":
                case "待办":
                    return Gray20;
                case "error":
                case "错误":
                    return ErrorLight;
                default:
                    return Gray20;
            }
        }

        /// <summary>
        /// 根据状态获取对应的徽章文本色
        /// </summary>
        public static Color GetBadgeTextColor(string status)
        {
            switch (status?.ToLowerInvariant())
            {
                case "done":
                case "完成":
                case "已完成":
                    return Success;
                case "doing":
                case "进行中":
                    return Color.FromArgb(138, 87, 0); // Darker yellow for text
                case "todo":
                case "待办":
                    return Gray130;
                case "error":
                case "错误":
                    return Error;
                default:
                    return Gray130;
            }
        }

        /// <summary>
        /// 混合颜色（前景色透明度混合到背景色）
        /// </summary>
        public static Color Blend(Color background, Color foreground, float opacity)
        {
            int r = (int)(background.R + (foreground.R - background.R) * opacity);
            int g = (int)(background.G + (foreground.G - background.G) * opacity);
            int b = (int)(background.B + (foreground.B - background.B) * opacity);
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// 使颜色变暗
        /// </summary>
        public static Color Darken(Color color, float factor)
        {
            return Color.FromArgb(
                (int)(color.R * (1 - factor)),
                (int)(color.G * (1 - factor)),
                (int)(color.B * (1 - factor))
            );
        }

        /// <summary>
        /// 使颜色变亮
        /// </summary>
        public static Color Lighten(Color color, float factor)
        {
            return Color.FromArgb(
                (int)(color.R + (255 - color.R) * factor),
                (int)(color.G + (255 - color.G) * factor),
                (int)(color.B + (255 - color.B) * factor)
            );
        }

        #endregion
    }
}
