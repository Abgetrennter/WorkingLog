using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Core.Helpers;

namespace WorkLogApp.Services.Implementations
{
    /// <summary>
    /// PDF 导出服务实现 - 基于 PdfSharp，支持 Win7 离线环境
    /// </summary>
    public class PdfExportService : IPdfExportService
    {
        // 系统常见中文字体
        private static readonly string[] ChineseFontNames = new[]
        {
            "Microsoft YaHei",      // 微软雅黑
            "SimHei",               // 黑体
            "SimSun",               // 宋体
            "NSimSun",              // 新宋体
            "KaiTi",                // 楷体
            "FangSong",             // 仿宋
            "DengXian"              // 等线 (Win10+)
        };

        /// <summary>
        /// 获取系统已安装的中文字体名称列表
        /// </summary>
        public IEnumerable<string> GetAvailableChineseFonts()
        {
            var availableFonts = new List<string>();
            
            foreach (var fontName in ChineseFontNames)
            {
                try
                {
                    // 尝试创建字体来验证是否存在
                    using (var font = new Font(fontName, 10))
                    {
                        if (font.Name == fontName || font.Name.Contains(fontName))
                        {
                            availableFonts.Add(fontName);
                        }
                    }
                }
                catch
                {
                    // 字体不存在，跳过
                }
            }

            return availableFonts.Any() ? availableFonts : new List<string> { "Arial" };
        }

        /// <summary>
        /// 导出单条工作日志为 PDF
        /// </summary>
        public bool ExportToPdf(WorkLog log, string outputPath, PdfExportOptions options = null)
        {
            if (log == null) return false;
            if (string.IsNullOrWhiteSpace(outputPath)) return false;

            options = options ?? new PdfExportOptions();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

                using (var document = new PdfDocument())
                {
                    document.Info.Title = options.Title;
                    document.Info.Author = "WorkLogApp";
                    document.Info.CreationDate = DateTime.Now;

                    AddWorkLogPage(document, log, options);
                    document.Save(outputPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                // 可以在这里添加日志记录
                System.Diagnostics.Debug.WriteLine($"PDF export error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 导出整月工作日志为 PDF（按周分组）
        /// </summary>
        public bool ExportMonthToPdf(DateTime month, IEnumerable<WorkLog> days, string outputPath, PdfExportOptions options = null)
        {
            if (days == null) return false;
            if (string.IsNullOrWhiteSpace(outputPath)) return false;

            options = options ?? new PdfExportOptions();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

                var validDays = (days ?? Enumerable.Empty<WorkLog>())
                    .Where(d => d != null && d.LogDate.Year == month.Year && d.LogDate.Month == month.Month)
                    .OrderBy(d => d.LogDate)
                    .ToList();

                using (var document = new PdfDocument())
                {
                    document.Info.Title = $"{month:yyyy年MM月} - {options.Title}";
                    document.Info.Author = "WorkLogApp";
                    document.Info.CreationDate = DateTime.Now;

                    if (validDays.Any())
                    {
                        // 按周分组（周一为一周开始）
                        var weeks = validDays.GroupBy(d =>
                        {
                            var diff = (7 + (d.LogDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                            return d.LogDate.Date.AddDays(-1 * diff);
                        }).OrderBy(g => g.Key);

                        foreach (var weekGroup in weeks)
                        {
                            var weekDays = weekGroup.OrderBy(d => d.LogDate).ToList();
                            AddWeekPage(document, weekDays, options, month);
                        }
                    }
                    else
                    {
                        // 无数据时添加空白页
                        AddEmptyPage(document, options, "本月无数据");
                    }

                    document.Save(outputPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PDF month export error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 添加单条工作日志页面
        /// </summary>
        private void AddWorkLogPage(PdfDocument document, WorkLog log, PdfExportOptions options)
        {
            var page = document.AddPage();
            page.Orientation = options.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
            SetPageMargins(page, options.Margins);

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                var fonts = GetFonts(options);
                double y = options.Margins.Top;
                double pageWidth = page.Width.Point - options.Margins.Left - options.Margins.Right;

                // 标题
                DrawTitle(gfx, $"{log.LogDate:yyyy年MM月dd日} 工作日志", fonts.TitleFont, options.Margins.Left, ref y, pageWidth);
                y += 10;

                // 当日总结
                if (!string.IsNullOrWhiteSpace(log.DailySummary))
                {
                    DrawSectionHeader(gfx, "当日总结", fonts.HeaderFont, options.Margins.Left, ref y, pageWidth);
                    DrawTextBlock(gfx, log.DailySummary, fonts.BodyFont, options.Margins.Left, ref y, pageWidth);
                    y += 10;
                }

                // 日志项表格
                if (log.Items?.Any() == true)
                {
                    DrawSectionHeader(gfx, "工作事项", fonts.HeaderFont, options.Margins.Left, ref y, pageWidth);
                    DrawWorkLogItemsTable(gfx, log.Items, fonts, options.Margins.Left, ref y, pageWidth, options);
                }
            }
        }

        /// <summary>
        /// 添加整周页面
        /// </summary>
        private void AddWeekPage(PdfDocument document, List<WorkLog> weekDays, PdfExportOptions options, DateTime month)
        {
            var page = document.AddPage();
            page.Orientation = options.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
            SetPageMargins(page, options.Margins);

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                var fonts = GetFonts(options);
                double y = options.Margins.Top;
                double pageWidth = page.Width.Point - options.Margins.Left - options.Margins.Right;

                var firstDay = weekDays.First().LogDate;
                var lastDay = weekDays.Last().LogDate;
                var weekTitle = firstDay.Month == lastDay.Month
                    ? $"{month:yyyy年MM月} - {firstDay.Day}日至{lastDay.Day}日"
                    : $"{firstDay:MM月dd日} 至 {lastDay:MM月dd日}";

                DrawTitle(gfx, weekTitle, fonts.TitleFont, options.Margins.Left, ref y, pageWidth);
                y += 15;

                // 绘制一周的表格
                DrawWeekTable(gfx, weekDays, fonts, options.Margins.Left, ref y, pageWidth, options);
            }
        }

        /// <summary>
        /// 绘制一周表格（7天）
        /// </summary>
        private void DrawWeekTable(XGraphics gfx, List<WorkLog> weekDays, PdfFonts fonts, double x, ref double y, double width, PdfExportOptions options)
        {
            // 表头
            var headers = new[] { "日期", "事项", "内容", "分类", "状态", "时间" };
            var colWidths = CalculateColumnWidths(width, new[] { 12, 20, 35, 12, 8, 13 });

            double rowHeight = 25;
            double headerY = y;

            // 绘制表头背景
            gfx.DrawRectangle(XBrushes.LightGray, x, headerY, width, rowHeight);

            // 绘制表头文字
            double colX = x;
            for (int i = 0; i < headers.Length; i++)
            {
                var rect = new XRect(colX + 2, headerY + 2, colWidths[i] - 4, rowHeight - 4);
                gfx.DrawString(headers[i], fonts.HeaderFont, XBrushes.Black, rect, XStringFormats.CenterLeft);
                colX += colWidths[i];
            }

            // 绘制表格外框
            gfx.DrawRectangle(XPens.Black, x, headerY, width, rowHeight);
            colX = x;
            for (int i = 0; i < colWidths.Length - 1; i++)
            {
                colX += colWidths[i];
                gfx.DrawLine(XPens.Gray, colX, headerY, colX, headerY + rowHeight);
            }

            y += rowHeight;

            // 数据行
            foreach (var day in weekDays)
            {
                var items = day.Items?.Where(i => i != null).ToList() ?? new List<WorkLogItem>();
                
                if (!items.Any())
                {
                    // 空行
                    DrawTableRow(gfx, day, null, fonts, x, ref y, colWidths, rowHeight);
                }
                else
                {
                    foreach (var item in items)
                    {
                        DrawTableRow(gfx, day, item, fonts, x, ref y, colWidths, rowHeight);
                    }
                }
            }

            // 绘制总结行
            y += 10;
            DrawSummarySection(gfx, weekDays, fonts, x, ref y, width);
        }

        /// <summary>
        /// 绘制表格行
        /// </summary>
        private void DrawTableRow(XGraphics gfx, WorkLog day, WorkLogItem item, PdfFonts fonts, double x, ref double y, double[] colWidths, double rowHeight)
        {
            double colX = x;
            var values = new[]
            {
                day.LogDate.ToString("MM-dd"),
                item?.ItemTitle ?? "",
                item?.ItemContent ?? "",
                item?.CategoryName ?? "",
                item != null ? StatusHelper.ToChinese(item.Status) : "",
                FormatTimeRange(item?.StartTime, item?.EndTime)
            };

            // 绘制行背景交替色
            bool isEvenRow = ((int)(y / rowHeight)) % 2 == 0;
            if (isEvenRow)
            {
                gfx.DrawRectangle(XBrushes.WhiteSmoke, x, y, colWidths.Sum(), rowHeight);
            }

            for (int i = 0; i < values.Length; i++)
            {
                var rect = new XRect(colX + 2, y + 2, colWidths[i] - 4, rowHeight - 4);
                var text = TruncateText(values[i], 100);
                gfx.DrawString(text, fonts.BodyFont, XBrushes.Black, rect, XStringFormats.CenterLeft);
                colX += colWidths[i];
            }

            // 绘制行边框
            gfx.DrawRectangle(XPens.LightGray, x, y, colWidths.Sum(), rowHeight);

            y += rowHeight;
        }

        /// <summary>
        /// 绘制工作项表格
        /// </summary>
        private void DrawWorkLogItemsTable(XGraphics gfx, List<WorkLogItem> items, PdfFonts fonts, double x, ref double y, double width, PdfExportOptions options)
        {
            var headers = new[] { "序号", "标题", "内容", "分类", "状态", "时间范围" };
            var colWidths = CalculateColumnWidths(width, new[] { 8, 20, 35, 12, 10, 15 });

            double rowHeight = 25;

            // 表头
            gfx.DrawRectangle(XBrushes.LightGray, x, y, width, rowHeight);
            double colX = x;
            for (int i = 0; i < headers.Length; i++)
            {
                var rect = new XRect(colX + 2, y + 2, colWidths[i] - 4, rowHeight - 4);
                gfx.DrawString(headers[i], fonts.HeaderFont, XBrushes.Black, rect, XStringFormats.CenterLeft);
                colX += colWidths[i];
            }
            gfx.DrawRectangle(XPens.Black, x, y, width, rowHeight);
            y += rowHeight;

            // 数据行
            int index = 1;
            foreach (var item in items)
            {
                colX = x;
                var values = new[]
                {
                    index.ToString(),
                    item.ItemTitle ?? "",
                    item.ItemContent ?? "",
                    item.CategoryName ?? "",
                    StatusHelper.ToChinese(item.Status),
                    FormatTimeRange(item.StartTime, item.EndTime)
                };

                for (int i = 0; i < values.Length; i++)
                {
                    var rect = new XRect(colX + 2, y + 2, colWidths[i] - 4, rowHeight - 4);
                    gfx.DrawString(TruncateText(values[i], 100), fonts.BodyFont, XBrushes.Black, rect, XStringFormats.CenterLeft);
                    colX += colWidths[i];
                }

                gfx.DrawRectangle(XPens.LightGray, x, y, width, rowHeight);
                y += rowHeight;
                index++;
            }
        }

        /// <summary>
        /// 绘制总结部分
        /// </summary>
        private void DrawSummarySection(XGraphics gfx, List<WorkLog> weekDays, PdfFonts fonts, double x, ref double y, double width)
        {
            gfx.DrawString("每日总结", fonts.HeaderFont, XBrushes.Black, x, y);
            y += 20;

            foreach (var day in weekDays)
            {
                if (!string.IsNullOrWhiteSpace(day.DailySummary))
                {
                    var summaryText = $"{day.LogDate:MM-dd}: {day.DailySummary}";
                    DrawTextBlock(gfx, summaryText, fonts.BodyFont, x, ref y, width);
                    y += 5;
                }
            }
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawTitle(XGraphics gfx, string text, XFont font, double x, ref double y, double width)
        {
            var rect = new XRect(x, y, width, font.Height * 2);
            gfx.DrawString(text, font, XBrushes.Black, rect, XStringFormats.Center);
            y += font.Height * 2 + 5;
        }

        /// <summary>
        /// 绘制小节标题
        /// </summary>
        private void DrawSectionHeader(XGraphics gfx, string text, XFont font, double x, ref double y, double width)
        {
            gfx.DrawString(text, font, XBrushes.Black, x, y);
            y += font.Height + 5;
        }

        /// <summary>
        /// 绘制文本块（自动换行）
        /// </summary>
        private void DrawTextBlock(XGraphics gfx, string text, XFont font, double x, ref double y, double width)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // 简单换行处理
            var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var line = new System.Text.StringBuilder();
            double lineHeight = font.Height * 1.2;

            foreach (var word in words)
            {
                var testLine = line.Length > 0 ? line + " " + word : word;
                var size = gfx.MeasureString(testLine, font);

                if (size.Width > width && line.Length > 0)
                {
                    gfx.DrawString(line.ToString(), font, XBrushes.Black, x, y);
                    y += lineHeight;
                    line.Clear();
                    line.Append(word);
                }
                else
                {
                    line.Clear();
                    line.Append(testLine);
                }
            }

            if (line.Length > 0)
            {
                gfx.DrawString(line.ToString(), font, XBrushes.Black, x, y);
                y += lineHeight;
            }
        }

        /// <summary>
        /// 添加空白页面
        /// </summary>
        private void AddEmptyPage(PdfDocument document, PdfExportOptions options, string message)
        {
            var page = document.AddPage();
            page.Orientation = options.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
            SetPageMargins(page, options.Margins);

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                var fonts = GetFonts(options);
                double x = page.Width.Point / 2;
                double y = page.Height.Point / 2;
                gfx.DrawString(message, fonts.TitleFont, XBrushes.Gray, x, y, XStringFormats.Center);
            }
        }

        /// <summary>
        /// 设置页面边距
        /// </summary>
        private void SetPageMargins(PdfPage page, PageMargins margins)
        {
             #pragma warning disable
            // PdfSharp 默认使用点（1/72英寸）
            double mmToPoint = 72.0 / 25.4;
            
            // 注意：PdfSharp 的 TrimMargins 可能在某些版本不可用
            // 这里我们通过绘制时的偏移量来处理边距
        }

        /// <summary>
        /// 获取字体（支持自定义字体）
        /// </summary>
        private PdfFonts GetFonts(PdfExportOptions options)
        {
            XFont baseFont;

            if (!string.IsNullOrWhiteSpace(options.CustomFontPath) && File.Exists(options.CustomFontPath))
            {
                // 使用自定义字体文件
                try
                {
                    // PdfSharp 1.5 不支持直接加载 TTF 文件创建 XFont
                    // 需要通过 GDI+ 字体或系统字体
                    baseFont = new XFont(options.FontName, options.FontSize, XFontStyle.Regular);
                }
                catch
                {
                    baseFont = GetSystemChineseFont(options.FontSize);
                }
            }
            else
            {
                // 使用系统字体
                baseFont = GetSystemChineseFont(options.FontSize, options.FontName);
            }

            return new PdfFonts
            {
                TitleFont = new XFont(baseFont.Name, options.TitleFontSize, XFontStyle.Bold),
                HeaderFont = new XFont(baseFont.Name, options.FontSize + 1, XFontStyle.Bold),
                BodyFont = baseFont
            };
        }

        /// <summary>
        /// 获取系统中文字体
        /// </summary>
        private XFont GetSystemChineseFont(double size, string preferredFont = null)
        {
            var fontNames = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(preferredFont))
            {
                fontNames.Add(preferredFont);
            }
            
            fontNames.AddRange(ChineseFontNames);

            foreach (var fontName in fontNames)
            {
                try
                {
                    var font = new XFont(fontName, size, XFontStyle.Regular);
                    // 验证字体是否有效
                    if (font.Name.Contains(fontName) || fontName.Contains(font.Name))
                    {
                        return font;
                    }
                }
                catch
                {
                    continue;
                }
            }

            // 最终回退到 Arial
            return new XFont("Arial", size, XFontStyle.Regular);
        }

        /// <summary>
        /// 计算列宽
        /// </summary>
        private double[] CalculateColumnWidths(double totalWidth, int[] ratios)
        {
            int totalRatio = ratios.Sum();
            return ratios.Select(r => totalWidth * r / totalRatio).ToArray();
        }

        /// <summary>
        /// 格式化时间范围
        /// </summary>
        private string FormatTimeRange(DateTime? start, DateTime? end)
        {
            if (!start.HasValue && !end.HasValue) return "";
            if (!start.HasValue) return $"- {end:HH:mm}";
            if (!end.HasValue) return $"{start:HH:mm} -";
            return $"{start:HH:mm}-{end:HH:mm}";
        }

        /// <summary>
        /// 截断文本
        /// </summary>
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text ?? "";
            return text.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// 字体集合
        /// </summary>
        private class PdfFonts
        {
            public XFont TitleFont { get; set; }
            public XFont HeaderFont { get; set; }
            public XFont BodyFont { get; set; }
        }
    }
}
