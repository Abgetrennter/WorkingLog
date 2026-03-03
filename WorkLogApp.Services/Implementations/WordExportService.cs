using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;
using WorkLogApp.Core.Helpers;

namespace WorkLogApp.Services.Implementations
{
    /// <summary>
    /// Word 导出服务实现 - 基于 DocX，支持 Win7 离线环境
    /// </summary>
    public class WordExportService : IWordExportService
    {
        /// <summary>
        /// 导出单条工作日志为 Word
        /// </summary>
        public bool ExportToWord(WorkLog log, string outputPath, WordExportOptions options = null)
        {
            if (log == null) return false;
            if (string.IsNullOrWhiteSpace(outputPath)) return false;

            options = options ?? new WordExportOptions();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

                using (var doc = DocX.Create(outputPath))
                {
                    ConfigureDocument(doc, options);
                    
                    // 添加标题
                    var titlePara = doc.InsertParagraph($"{log.LogDate:yyyy年MM月dd日} 工作日志");
                    titlePara.FontSize(options.TitleFontSize)
                        .Font(options.FontName)
                        .Bold()
                        .SpacingAfter(15);
                    titlePara.Alignment = Alignment.center;

                    // 当日总结
                    if (!string.IsNullOrWhiteSpace(log.DailySummary))
                    {
                        doc.InsertParagraph("当日总结")
                            .FontSize(options.FontSize + 2)
                            .Font(options.FontName)
                            .Bold()
                            .SpacingBefore(10)
                            .SpacingAfter(5);

                        doc.InsertParagraph(log.DailySummary)
                            .FontSize(options.FontSize)
                            .Font(options.FontName)
                            .SpacingAfter(10);
                    }

                    // 工作项表格
                    if (log.Items?.Any() == true)
                    {
                        doc.InsertParagraph("工作事项")
                            .FontSize(options.FontSize + 2)
                            .Font(options.FontName)
                            .Bold()
                            .SpacingBefore(10)
                            .SpacingAfter(5);

                        InsertWorkLogItemsTable(doc, log.Items, options);
                    }

                    if (options.IncludeHeader)
                    {
                        AddHeader(doc, options);
                    }

                    if (options.IncludeFooter)
                    {
                        AddFooter(doc);
                    }

                    doc.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Word export error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 导出整月工作日志为 Word（按周分组）
        /// </summary>
        public bool ExportMonthToWord(DateTime month, IEnumerable<WorkLog> days, string outputPath, WordExportOptions options = null)
        {
            if (days == null) return false;
            if (string.IsNullOrWhiteSpace(outputPath)) return false;

            options = options ?? new WordExportOptions();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

                var validDays = (days ?? Enumerable.Empty<WorkLog>())
                    .Where(d => d != null && d.LogDate.Year == month.Year && d.LogDate.Month == month.Month)
                    .OrderBy(d => d.LogDate)
                    .ToList();

                using (var doc = DocX.Create(outputPath))
                {
                    ConfigureDocument(doc, options);

                    // 文档标题
                    var titlePara = doc.InsertParagraph($"{month:yyyy年MM月} 工作日志");
                    titlePara.FontSize(options.TitleFontSize)
                        .Font(options.FontName)
                        .Bold()
                        .SpacingAfter(20);
                    titlePara.Alignment = Alignment.center;

                    if (validDays.Any())
                    {
                        // 按周分组
                        var weeks = validDays.GroupBy(d =>
                        {
                            var diff = (7 + (d.LogDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                            return d.LogDate.Date.AddDays(-1 * diff);
                        }).OrderBy(g => g.Key);

                        foreach (var weekGroup in weeks)
                        {
                            var weekDays = weekGroup.OrderBy(d => d.LogDate).ToList();
                            InsertWeekTable(doc, weekDays, options);
                            doc.InsertParagraph().SpacingAfter(10);
                        }
                    }
                    else
                    {
                        var emptyPara = doc.InsertParagraph("本月无数据");
                        emptyPara.FontSize(options.FontSize)
                            .Font(options.FontName)
                            .Italic();
                        emptyPara.Alignment = Alignment.center;
                    }

                    if (options.IncludeHeader)
                    {
                        AddHeader(doc, options);
                    }

                    if (options.IncludeFooter)
                    {
                        AddFooter(doc);
                    }

                    doc.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Word month export error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 配置文档页面设置
        /// </summary>
        private void ConfigureDocument(DocX doc, WordExportOptions options)
        {
            // 设置页面方向
            doc.PageLayout.Orientation = options.Landscape 
                ? Orientation.Landscape 
                : Orientation.Portrait;

            // 设置页边距（厘米转英寸，1英寸 = 2.54厘米）
            doc.MarginTop = (float)(options.Margins.Top / 2.54);
            doc.MarginBottom = (float)(options.Margins.Bottom / 2.54);
            doc.MarginLeft = (float)(options.Margins.Left / 2.54);
            doc.MarginRight = (float)(options.Margins.Right / 2.54);
        }

        /// <summary>
        /// 插入一周数据表格
        /// </summary>
        private void InsertWeekTable(DocX doc, List<WorkLog> weekDays, WordExportOptions options)
        {
            var firstDay = weekDays.First().LogDate;
            var lastDay = weekDays.Last().LogDate;
            
            var weekTitle = firstDay.Month == lastDay.Month
                ? $"{firstDay.Day}日至{lastDay.Day}日"
                : $"{firstDay:MM月dd日} 至 {lastDay:MM月dd日}";

            // 周标题
            doc.InsertParagraph(weekTitle)
                .FontSize(options.FontSize + 2)
                .Font(options.FontName)
                .Bold()
                .SpacingBefore(15)
                .SpacingAfter(10);

            // 创建表格
            var table = doc.AddTable(1, 6);
            table.Design = TableDesign.TableGrid;

            // 表头
            var headers = new[] { "日期", "事项", "内容", "分类", "状态", "时间" };
            var headerRow = table.Rows[0];
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cells[i].Paragraphs[0]
                    .Append(headers[i])
                    .FontSize(options.FontSize)
                    .Font(options.FontName)
                    .Bold();
                headerRow.Cells[i].FillColor = System.Drawing.Color.LightGray;
            }

            // 数据行
            foreach (var day in weekDays)
            {
                var items = day.Items?.Where(i => i != null).ToList() ?? new List<WorkLogItem>();
                
                if (!items.Any())
                {
                    // 添加空行
                    AddTableRow(table, day, null, options);
                }
                else
                {
                    foreach (var item in items)
                    {
                        AddTableRow(table, day, item, options);
                    }
                }
            }

            doc.InsertTable(table);

            // 添加该周总结
            AddWeekSummary(doc, weekDays, options);
        }

        /// <summary>
        /// 添加表格行
        /// </summary>
        private void AddTableRow(Xceed.Document.NET.Table table, WorkLog day, WorkLogItem item, WordExportOptions options)
        {
            var row = table.InsertRow();
            
            row.Cells[0].Paragraphs[0].Append(day.LogDate.ToString("MM-dd"))
                .FontSize(options.FontSize).Font(options.FontName);
            row.Cells[1].Paragraphs[0].Append(item?.ItemTitle ?? "")
                .FontSize(options.FontSize).Font(options.FontName);
            row.Cells[2].Paragraphs[0].Append(item?.ItemContent ?? "")
                .FontSize(options.FontSize).Font(options.FontName);
            row.Cells[3].Paragraphs[0].Append(item?.CategoryName ?? "")
                .FontSize(options.FontSize).Font(options.FontName);
            row.Cells[4].Paragraphs[0].Append(item != null ? StatusHelper.ToChinese(item.Status) : "")
                .FontSize(options.FontSize).Font(options.FontName);
            row.Cells[5].Paragraphs[0].Append(FormatTimeRange(item?.StartTime, item?.EndTime))
                .FontSize(options.FontSize).Font(options.FontName);

            // 交替行背景色
            if (table.RowCount % 2 == 0)
            {
                foreach (var cell in row.Cells)
                {
                    cell.FillColor = System.Drawing.Color.WhiteSmoke;
                }
            }
        }

        /// <summary>
        /// 添加周总结
        /// </summary>
        private void AddWeekSummary(DocX doc, List<WorkLog> weekDays, WordExportOptions options)
        {
            var hasSummaries = weekDays.Any(d => !string.IsNullOrWhiteSpace(d.DailySummary));
            if (!hasSummaries) return;

            doc.InsertParagraph("本周总结")
                .FontSize(options.FontSize + 1)
                .Font(options.FontName)
                .Bold()
                .SpacingBefore(10)
                .SpacingAfter(5);

            foreach (var day in weekDays)
            {
                if (!string.IsNullOrWhiteSpace(day.DailySummary))
                {
                    doc.InsertParagraph($"• {day.LogDate:MM-dd}: {day.DailySummary}")
                        .FontSize(options.FontSize)
                        .Font(options.FontName)
                        .SpacingAfter(3);
                }
            }
        }

        /// <summary>
        /// 插入工作项表格
        /// </summary>
        private void InsertWorkLogItemsTable(DocX doc, List<WorkLogItem> items, WordExportOptions options)
        {
            var table = doc.AddTable(1, 6);
            table.Design = TableDesign.TableGrid;

            // 表头
            var headers = new[] { "序号", "标题", "内容", "分类", "状态", "时间范围" };
            var headerRow = table.Rows[0];
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cells[i].Paragraphs[0]
                    .Append(headers[i])
                    .FontSize(options.FontSize)
                    .Font(options.FontName)
                    .Bold();
                headerRow.Cells[i].FillColor = System.Drawing.Color.LightGray;
            }

            // 数据行
            int index = 1;
            foreach (var item in items)
            {
                var row = table.InsertRow();
                
                row.Cells[0].Paragraphs[0].Append(index.ToString())
                    .FontSize(options.FontSize).Font(options.FontName);
                row.Cells[1].Paragraphs[0].Append(item.ItemTitle ?? "")
                    .FontSize(options.FontSize).Font(options.FontName);
                row.Cells[2].Paragraphs[0].Append(item.ItemContent ?? "")
                    .FontSize(options.FontSize).Font(options.FontName);
                row.Cells[3].Paragraphs[0].Append(item.CategoryName ?? "")
                    .FontSize(options.FontSize).Font(options.FontName);
                row.Cells[4].Paragraphs[0].Append(StatusHelper.ToChinese(item.Status))
                    .FontSize(options.FontSize).Font(options.FontName);
                row.Cells[5].Paragraphs[0].Append(FormatTimeRange(item.StartTime, item.EndTime))
                    .FontSize(options.FontSize).Font(options.FontName);

                index++;
            }

            doc.InsertTable(table);
        }

        /// <summary>
        /// 添加页眉
        /// </summary>
        private void AddHeader(DocX doc, WordExportOptions options)
        {
            doc.AddHeaders();
            var header = doc.Headers.Odd;
            header.Paragraphs[0]
                .Append(options.Title)
                .FontSize(options.FontSize - 1)
                .Font(options.FontName);
            header.Paragraphs[0].Alignment = Alignment.right;
            
            // 添加下划线
            header.Paragraphs[0].InsertHorizontalLine(HorizontalBorderPosition.bottom, BorderStyle.Tcbs_single, 6, 0, System.Drawing.Color.Gray);
        }

        /// <summary>
        /// 添加页脚（页码）
        /// </summary>
        private void AddFooter(DocX doc)
        {
            doc.AddFooters();
            var footer = doc.Footers.Odd;
            footer.Paragraphs[0].Append("第 ").FontSize(9);
            footer.Paragraphs[0].AppendPageNumber(PageNumberFormat.normal);
            footer.Paragraphs[0].Append(" 页").FontSize(9);
            footer.Paragraphs[0].Alignment = Alignment.center;
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
    }
}
