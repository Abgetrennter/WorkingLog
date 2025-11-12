using System;
using System.Drawing;
using System.Drawing.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WorkLogApp.UI.UI
{
    public static class UIStyleManager
    {
        private static PrivateFontCollection _pfc;
        private static FontFamily _customFamily;

        // 全局缩放比例常量（修改此值可统一调整 UI 缩放）
        public static float ScaleFactor { get; set; } = 1.0f;

        public static Font BodyFont { get; private set; }
        public static Font CompactFont { get; private set; }
        public static Font Heading1 { get; private set; }
        public static Font Heading2 { get; private set; }
        public static Font Heading3 { get; private set; }

        // 是否启用自定义字体（默认关闭，以避免非系统字体的渲染发虚）
        public static bool EnableCustomFont { get; set; } = true;

        // 统一的设计期检测：在设计器中返回 true
        public static bool IsDesignMode
        {
            get
            {
                try
                {
                    if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
                    var procName = Process.GetCurrentProcess()?.ProcessName ?? string.Empty;
                    if (procName.Equals("devenv", StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch { }
                return false;
            }
        }

        public static void Initialize()
        {
            _customFamily = null;
            if (EnableCustomFont)
            {
                TryLoadCustomFont();
            }
            var family = _customFamily ?? GetPreferredDefaultFamily();
            // 以更接近系统默认的字号作为基础，避免在高 DPI 环境下出现过度放大
            BodyFont = new Font(family, 12f, FontStyle.Regular, GraphicsUnit.Point);
            // 紧凑字体用于工具栏按钮等需要更小字号的控件
            CompactFont = new Font(family, 15f, FontStyle.Regular, GraphicsUnit.Point);
            // 标题字号适度增大，但不至于导致整体控件高度剧增
            Heading1 = new Font(family, 16f, FontStyle.Bold, GraphicsUnit.Point);
            Heading2 = new Font(family, 14f, FontStyle.Bold, GraphicsUnit.Point);
            Heading3 = new Font(family, 14f, FontStyle.Bold, GraphicsUnit.Point);
        }

        // 便捷重载：使用全局缩放比例
        public static void ApplyVisualEnhancements(Form form)
        {
            ApplyVisualEnhancements(form, ScaleFactor);
        }

        public static void ApplyVisualEnhancements(Form form, float scaleFactor = 1.5f)
        {
            if (BodyFont == null) Initialize();
            // 使用 DPI 自动缩放，避免与手动 Scale 叠加造成重复缩放
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.Font = BodyFont;

            // 减少闪烁
            TryEnableDoubleBuffer(form);

            // 标签使用 GDI+ 文本渲染（可应用 ClearType）
            ApplyToControlTree(form);
        }

        private static void ApplyToControlTree(Control root)
        {
            foreach (Control c in root.Controls)
            {
                // 标签采用兼容文本渲染以获得更好的抗锯齿
                if (c is Label lbl)
                {
                    // 保持系统默认的 GDI 文本渲染，更清晰（尤其在高 DPI 下）
                    lbl.UseCompatibleTextRendering = false;
                    ApplyHeadingIfTagged(lbl);
                }

                // 文本控件统一字体
                if (c is TextBoxBase tbb)
                {
                    tbb.Font = BodyFont;
                }

                // 列表、按钮、下拉框等统一字体（支持紧凑模式：Tag == "compact"）
                if (c is ListView || c is Button || c is ComboBox || c is DateTimePicker || c is DataGridView)
                {
                    var tagStr = c.Tag as string;
                    if (!string.IsNullOrWhiteSpace(tagStr) && string.Equals(tagStr.Trim(), "compact", StringComparison.OrdinalIgnoreCase))
                    {
                        c.Font = CompactFont;
                    }
                    else
                    {
                        c.Font = BodyFont;
                    }
                }

                // 富文本框设置 1.5 倍行距
                if (c is RichTextBox rtb)
                {
                    rtb.Font = BodyFont;
                    SetLineSpacing(rtb, 1.5f);
                }

                // 顶部/底部停靠的面板根据子控件动态增高，避免缩放后按钮被裁剪
                if (c is Panel pnl && (pnl.Dock == DockStyle.Top || pnl.Dock == DockStyle.Bottom))
                {
                    try
                    {
                        int maxBottom = 0;
                        foreach (Control child in pnl.Controls)
                        {
                            if (!child.Visible) continue;
                            maxBottom = Math.Max(maxBottom, child.Bottom + child.Margin.Bottom);
                        }
                        var needed = maxBottom + pnl.Padding.Bottom;
                        if (needed > pnl.Height)
                        {
                            pnl.Height = needed + pnl.Padding.Top;
                        }
                    }
                    catch { }
                }

                TryEnableDoubleBuffer(c);
                if (c.HasChildren)
                {
                    ApplyToControlTree(c);
                }
            }
        }

        private static void ApplyHeadingIfTagged(Label lbl)
        {
            var tag = lbl.Tag as string;
            if (string.IsNullOrWhiteSpace(tag)) return;
            switch (tag.Trim().ToLowerInvariant())
            {
                case "h1":
                    lbl.Font = Heading1;
                    break;
                case "h2":
                    lbl.Font = Heading2;
                    break;
                case "h3":
                    lbl.Font = Heading3;
                    break;
            }
        }

        private static void TryEnableDoubleBuffer(Control c)
        {
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                prop?.SetValue(c, true, null);
            }
            catch { }
        }

        private static FontFamily GetPreferredDefaultFamily()
        {
            // 中文界面优先雅黑，其次系统默认 Sans Serif
            try
            {
                foreach (var fam in FontFamily.Families)
                {
                    if (string.Equals(fam.Name, "Microsoft YaHei UI", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(fam.Name, "Microsoft YaHei", StringComparison.OrdinalIgnoreCase))
                    {
                        return fam;
                    }
                }
            }
            catch { }
            return FontFamily.GenericSansSerif;
        }

        private static void TryLoadCustomFont()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var fontPath = System.IO.Path.Combine(baseDir, "font.ttf");
                if (System.IO.File.Exists(fontPath))
                {
                    _pfc = new PrivateFontCollection();
                    _pfc.AddFontFile(fontPath);
                    if (_pfc.Families != null && _pfc.Families.Length > 0)
                    {
                        _customFamily = _pfc.Families[0];
                    }
                }
            }
            catch
            {
                _customFamily = null;
            }
        }

        // 通过 EM_SETPARAFORMAT 设置富文本框的行距
        public static void SetLineSpacing(RichTextBox rtb, float multiple)
        {
            try
            {
                var format = new PARAFORMAT2
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(PARAFORMAT2)),
                    dwMask = PFM_LINESPACING
                };
                format.cbSize = (uint)Marshal.SizeOf(typeof(PARAFORMAT2));
                format.dwMask = PFM_LINESPACING;

                // 1.5 倍行距使用规则 1（忽略 dyLineSpacing）
                if (multiple >= 1.49f && multiple <= 1.51f)
                {
                    format.bLineSpacingRule = 1; // one-and-a-half spacing
                }
                else if (multiple >= 1.99f && multiple <= 2.01f)
                {
                    format.bLineSpacingRule = 2; // double spacing
                }
                else if (multiple <= 1.01f)
                {
                    format.bLineSpacingRule = 0; // single
                }
                else
                {
                    // 多倍行距（相对字体高度），这里用 rule 5 + 近似 twips
                    format.bLineSpacingRule = 5;
                    // 近似按字体高度的 1/20（twips）换算，240 twips ≈ 单倍
                    format.dyLineSpacing = (int)(240f * multiple);
                }

                // 应用到全部文本
                rtb.SelectAll();
                SendMessage(rtb.Handle, EM_SETPARAFORMAT, SCF_SELECTION, ref format);
                rtb.Select(0, 0);
            }
            catch { }
        }

        // 尝试以 @2x/@3x 规则加载位图（供后续需要时使用）
        public static Image LoadImageWithScale(string path, float scaleFactor)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            try
            {
                var dir = System.IO.Path.GetDirectoryName(path);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                var ext = System.IO.Path.GetExtension(path);

                string candidate = null;
                if (scaleFactor >= 2.5f)
                {
                    candidate = System.IO.Path.Combine(dir, name + "@3x" + ext);
                    if (!System.IO.File.Exists(candidate)) candidate = null;
                }
                if (candidate == null && scaleFactor >= 1.5f)
                {
                    var p2 = System.IO.Path.Combine(dir, name + "@2x" + ext);
                    if (System.IO.File.Exists(p2)) candidate = p2;
                }

                var target = candidate ?? path;
                using (var fs = System.IO.File.OpenRead(target))
                {
                    return Image.FromStream(fs);
                }
            }
            catch { return null; }
        }

        // P/Invoke 定义
        private const int SCF_SELECTION = 1;
        private const int EM_SETPARAFORMAT = 1095; // 0x447
        private const int PFM_LINESPACING = 0x00000100;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PARAFORMAT2
        {
            public uint cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);
    }
}