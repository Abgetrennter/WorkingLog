using System.Drawing;

namespace WorkLogApp.UI.UI
{
    /// <summary>
    /// Microsoft Fluent Design System 字体层级规范
    /// 使用 Segoe UI Variable 字体家族
    /// </summary>
    public static class FluentTypography
    {
        #region Font Families - 字体家族

        /// <summary>Segoe UI Variable Display - 展示字体</summary>
        public const string FontFamilyDisplay = "Segoe UI Variable Display";
        
        /// <summary>Segoe UI Variable Text - 正文字体</summary>
        public const string FontFamilyText = "Segoe UI Variable Text";
        
        /// <summary>Segoe UI Variable Small - 小字体</summary>
        public const string FontFamilySmall = "Segoe UI Variable Small";
        
        /// <summary>Segoe UI - 后备字体</summary>
        public const string FontFamilySegoeUI = "Segoe UI";
        
        /// <summary>Microsoft YaHei UI - 中文后备</summary>
        public const string FontFamilyYaHei = "Microsoft YaHei UI";

        /// <summary>完整字体回退栈</summary>
        public static string FontFamilyStack => 
            FontFamilyText + ", " + FontFamilySegoeUI + ", " + FontFamilyYaHei + ", sans-serif";

        #endregion

        #region Typography Scale - 字体层级

        /// <summary>Header - 页面标题 (28px, Bold)</summary>
        public static Font Header => CreateFont(21f, FontStyle.Bold);
        
        /// <summary>Title Large - 大标题 (20px, SemiBold)</summary>
        public static Font TitleLarge => CreateFont(15f, FontStyle.Bold);
        
        /// <summary>Title - 标题 (16px, SemiBold)</summary>
        public static Font Title => CreateFont(12f, FontStyle.Bold);
        
        /// <summary>Subtitle - 副标题 (14px, SemiBold)</summary>
        public static Font Subtitle => CreateFont(10.5f, FontStyle.Bold);
        
        /// <summary>Body Large - 大正文 (15px, Regular)</summary>
        public static Font BodyLarge => CreateFont(11.25f, FontStyle.Regular);
        
        /// <summary>Body - 正文 (14px, Regular)</summary>
        public static Font Body => CreateFont(10.5f, FontStyle.Regular);
        
        /// <summary>Caption - 说明文字 (12px, Regular)</summary>
        public static Font Caption => CreateFont(9f, FontStyle.Regular);
        
        /// <summary>Overline - 标签 (10px, SemiBold)</summary>
        public static Font Overline => CreateFont(7.5f, FontStyle.Bold);
        
        /// <summary>Button - 按钮文字 (14px, SemiBold)</summary>
        public static Font Button => CreateFont(10.5f, FontStyle.Regular);

        #endregion

        #region Line Heights - 行高

        /// <summary>Header 行高</summary>
        public const float HeaderLineHeight = 36f;
        
        /// <summary>Title Large 行高</summary>
        public const float TitleLargeLineHeight = 28f;
        
        /// <summary>Title 行高</summary>
        public const float TitleLineHeight = 24f;
        
        /// <summary>Subtitle 行高</summary>
        public const float SubtitleLineHeight = 20f;
        
        /// <summary>Body Large 行高</summary>
        public const float BodyLargeLineHeight = 24f;
        
        /// <summary>Body 行高</summary>
        public const float BodyLineHeight = 20f;
        
        /// <summary>Caption 行高</summary>
        public const float CaptionLineHeight = 16f;
        
        /// <summary>Overline 行高</summary>
        public const float OverlineLineHeight = 12f;

        #endregion

        #region Letter Spacing - 字间距

        /// <summary>Header 字间距</summary>
        public const float HeaderLetterSpacing = -0.02f;
        
        /// <summary>Title Large 字间距</summary>
        public const float TitleLargeLetterSpacing = -0.01f;
        
        /// <summary>默认字间距</summary>
        public const float DefaultLetterSpacing = 0f;
        
        /// <summary>Caption 字间距</summary>
        public const float CaptionLetterSpacing = 0.01f;
        
        /// <summary>Overline 字间距</summary>
        public const float OverlineLetterSpacing = 0.04f;

        #endregion

        #region Private Helpers - 私有辅助方法

        /// <summary>
        /// 创建字体，自动处理字体回退
        /// </summary>
        private static Font CreateFont(float sizeInPoints, FontStyle style)
        {
            // 尝试使用 Segoe UI Variable
            try
            {
                using (var font = new Font(FontFamilyText, sizeInPoints, style, GraphicsUnit.Point))
                {
                    if (font.Name.Contains("Segoe UI Variable"))
                    {
                        return new Font(FontFamilyText, sizeInPoints, style, GraphicsUnit.Point);
                    }
                }
            }
            catch { }

            // 回退到标准 Segoe UI
            try
            {
                using (var font = new Font(FontFamilySegoeUI, sizeInPoints, style, GraphicsUnit.Point))
                {
                    if (font.Name.Contains("Segoe UI"))
                    {
                        return new Font(FontFamilySegoeUI, sizeInPoints, style, GraphicsUnit.Point);
                    }
                }
            }
            catch { }

            // 回退到 Microsoft YaHei UI (中文)
            try
            {
                return new Font(FontFamilyYaHei, sizeInPoints, style, GraphicsUnit.Point);
            }
            catch { }

            // 最终回退到系统默认
            return new Font(FontFamily.GenericSansSerif, sizeInPoints, style, GraphicsUnit.Point);
        }

        #endregion

        #region Public Methods - 公共方法

        /// <summary>
        /// 获取适合中文内容的字体（优先使用微软雅黑）
        /// </summary>
        public static Font GetChineseFont(float sizeInPoints, FontStyle style = FontStyle.Regular)
        {
            try
            {
                return new Font(FontFamilyYaHei, sizeInPoints, style, GraphicsUnit.Point);
            }
            catch
            {
                return CreateFont(sizeInPoints, style);
            }
        }

        /// <summary>
        /// 根据文本类型获取推荐字体
        /// </summary>
        public static Font GetFontForPurpose(TextPurpose purpose)
        {
            switch (purpose)
            {
                case TextPurpose.PageHeader:
                    return Header;
                case TextPurpose.SectionTitle:
                    return TitleLarge;
                case TextPurpose.CardTitle:
                    return Title;
                case TextPurpose.FormLabel:
                    return Subtitle;
                case TextPurpose.BodyText:
                    return Body;
                case TextPurpose.Caption:
                    return Caption;
                case TextPurpose.Overline:
                    return Overline;
                default:
                    return Body;
            }
        }

        /// <summary>
        /// 计算文本尺寸（考虑行高）
        /// </summary>
        public static SizeF MeasureText(Graphics g, string text, Font font, float lineHeight)
        {
            var size = g.MeasureString(text, font);
            size.Height = lineHeight;
            return size;
        }

        #endregion
    }

    /// <summary>
    /// 文本用途枚举
    /// </summary>
    public enum TextPurpose
    {
        PageHeader,
        SectionTitle,
        CardTitle,
        FormLabel,
        BodyText,
        Caption,
        Overline
    }
}
