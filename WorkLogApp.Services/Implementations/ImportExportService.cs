using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class ImportExportService : IImportExportService
    {
        private const string FilePrefix = "worklog_";
        private const string SheetName = "工作日志";
        private static readonly string[] Header = new[]
        {
            // 新结构：移除 DailySummary 列
            "LogDate","ItemTitle","ItemContent","CategoryId","Status","Progress","StartTime","EndTime","Tags","SortOrder"
        };
        private static readonly string[] HeaderZh = new[]
        {
            // 与 Header 对应的中文显示名称（移除 当日总结）
            "日期","标题","内容","分类ID","状态","进度","开始时间","结束时间","标签","排序"
        };
        private static readonly Dictionary<string, string> HeaderNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"日期","LogDate"},
            {"标题","ItemTitle"},
            {"内容","ItemContent"},
            {"分类ID","CategoryId"},
            {"状态","Status"},
            {"进度","Progress"},
            {"开始时间","StartTime"},
            {"结束时间","EndTime"},
            {"标签","Tags"},
            {"排序","SortOrder"}
        };

        public bool ExportMonth(DateTime month, IEnumerable<WorkLogItem> items, string outputDirectory)
        {
            if (items == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            // 读取已存在的当月数据并与新数据合并，然后统一重写为单表结构
            var existing = ImportMonth(month, outputDirectory) ?? Enumerable.Empty<WorkLogItem>();
            var newItems = items.Where(i => i != null && i.LogDate.Year == month.Year && i.LogDate.Month == month.Month);
            var combined = existing.Concat(newItems).ToList();

            return RewriteMonth(month, combined, outputDirectory);
        }

        // 覆盖写入整月数据：生成单个工作表并写入传入的所有当月记录（按日期分块）
        public bool RewriteMonth(DateTime month, IEnumerable<WorkLogItem> items, string outputDirectory)
        {
            if (items == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            IWorkbook wb = new XSSFWorkbook();
            WriteMonthSheet(wb, month, items);
            using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                wb.Write(outFs);
            }
            return true;
        }

        private static void WriteMonthSheet(IWorkbook wb, DateTime month, IEnumerable<WorkLogItem> items)
        {
            var sheet = wb.CreateSheet(SheetName);

            // 写入中文表头
            var header = sheet.CreateRow(0);
            for (int i = 0; i < HeaderZh.Length; i++)
            {
                var hc = header.CreateCell(i);
                hc.SetCellValue(HeaderZh[i]);
            }

            // 样式：两种块背景色 + 加粗的日期标识行
            var boldFont = wb.CreateFont();
            boldFont.IsBold = true;

            var markerStyleA = wb.CreateCellStyle();
            markerStyleA.SetFont(boldFont);
            markerStyleA.FillPattern = FillPattern.SolidForeground;
            markerStyleA.FillForegroundColor = IndexedColors.LightCornflowerBlue.Index;
            markerStyleA.Alignment = HorizontalAlignment.Center;
            markerStyleA.VerticalAlignment = VerticalAlignment.Center;
            markerStyleA.BorderTop = BorderStyle.Thin;
            markerStyleA.BorderBottom = BorderStyle.Thin;
            markerStyleA.BorderLeft = BorderStyle.Thin;
            markerStyleA.BorderRight = BorderStyle.Thin;

            var markerStyleB = wb.CreateCellStyle();
            markerStyleB.SetFont(boldFont);
            markerStyleB.FillPattern = FillPattern.SolidForeground;
            markerStyleB.FillForegroundColor = IndexedColors.LightYellow.Index;
            markerStyleB.Alignment = HorizontalAlignment.Center;
            markerStyleB.VerticalAlignment = VerticalAlignment.Center;
            markerStyleB.BorderTop = BorderStyle.Thin;
            markerStyleB.BorderBottom = BorderStyle.Thin;
            markerStyleB.BorderLeft = BorderStyle.Thin;
            markerStyleB.BorderRight = BorderStyle.Thin;

            // 表头样式：加粗、居中、带边框
            var headerStyle = wb.CreateCellStyle();
            headerStyle.SetFont(boldFont);
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;
            for (int i = 0; i < HeaderZh.Length; i++)
            {
                header.GetCell(i).CellStyle = headerStyle;
            }

            var blockStyleA = wb.CreateCellStyle();
            blockStyleA.FillPattern = FillPattern.SolidForeground;
            blockStyleA.FillForegroundColor = IndexedColors.LightCornflowerBlue.Index;
            blockStyleA.WrapText = true;
            blockStyleA.Alignment = HorizontalAlignment.Left;
            blockStyleA.VerticalAlignment = VerticalAlignment.Top;
            blockStyleA.BorderTop = BorderStyle.Thin;
            blockStyleA.BorderBottom = BorderStyle.Thin;
            blockStyleA.BorderLeft = BorderStyle.Thin;
            blockStyleA.BorderRight = BorderStyle.Thin;

            var blockStyleB = wb.CreateCellStyle();
            blockStyleB.FillPattern = FillPattern.SolidForeground;
            blockStyleB.FillForegroundColor = IndexedColors.LightYellow.Index;
            blockStyleB.WrapText = true;
            blockStyleB.Alignment = HorizontalAlignment.Left;
            blockStyleB.VerticalAlignment = VerticalAlignment.Top;
            blockStyleB.BorderTop = BorderStyle.Thin;
            blockStyleB.BorderBottom = BorderStyle.Thin;
            blockStyleB.BorderLeft = BorderStyle.Thin;
            blockStyleB.BorderRight = BorderStyle.Thin;

            // 数字列样式（居中，带边框，继承块底色）
            var numberStyleA = wb.CreateCellStyle();
            numberStyleA.CloneStyleFrom(blockStyleA);
            numberStyleA.Alignment = HorizontalAlignment.Center;
            var numberStyleB = wb.CreateCellStyle();
            numberStyleB.CloneStyleFrom(blockStyleB);
            numberStyleB.Alignment = HorizontalAlignment.Center;

            // 时间列样式（右对齐，带边框，继承块底色）
            var timeStyleA = wb.CreateCellStyle();
            timeStyleA.CloneStyleFrom(blockStyleA);
            timeStyleA.Alignment = HorizontalAlignment.Right;
            var timeStyleB = wb.CreateCellStyle();
            timeStyleB.CloneStyleFrom(blockStyleB);
            timeStyleB.Alignment = HorizontalAlignment.Right;

            // 标题列样式（加粗）
            var titleStyleA = wb.CreateCellStyle();
            titleStyleA.CloneStyleFrom(blockStyleA);
            titleStyleA.SetFont(boldFont);
            var titleStyleB = wb.CreateCellStyle();
            titleStyleB.CloneStyleFrom(blockStyleB);
            titleStyleB.SetFont(boldFont);

            var ordered = (items ?? Enumerable.Empty<WorkLogItem>())
                .Where(i => i != null && i.LogDate.Year == month.Year && i.LogDate.Month == month.Month)
                .OrderBy(i => i.LogDate.Date)
                .ThenBy(i => i.SortOrder ?? 0)
                .ThenBy(i => i.StartTime ?? DateTime.MinValue)
                .ThenBy(i => i.ItemTitle ?? string.Empty)
                .ToList();

            int rowIndex = 0;
            int blockIndex = 0;
            foreach (var group in ordered.GroupBy(i => i.LogDate.Date).OrderBy(g => g.Key))
            {
                bool useA = (blockIndex % 2 == 0);
                var markerStyle = useA ? markerStyleA : markerStyleB;
                var blockStyle = useA ? blockStyleA : blockStyleB;
                var numberStyle = useA ? numberStyleA : numberStyleB;
                var timeStyle = useA ? timeStyleA : timeStyleB;
                var titleStyle = useA ? titleStyleA : titleStyleB;

                // 日期标识行（中文日期 + 中文星期）
                rowIndex++;
                var markerRow = sheet.CreateRow(rowIndex);
                for (int c = 0; c < HeaderZh.Length; c++)
                {
                    var cell = markerRow.CreateCell(c);
                    cell.CellStyle = markerStyle;
                    var week = GetChineseWeekday(group.Key);
                    cell.SetCellValue(c == 0 ? $"===== {group.Key:yyyy年MM月dd日} {week} =====" : string.Empty);
                }
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, HeaderZh.Length - 1));

                bool isFirstDataRow = true;
                string groupSummary = null;
                foreach (var item in group)
                {
                    rowIndex++;
                    var row = sheet.CreateRow(rowIndex);
                    for (int c = 0; c < HeaderZh.Length; c++) row.CreateCell(c);

                    // 中文日期格式
                    row.GetCell(0).SetCellValue(item.LogDate.ToString("yyyy年MM月dd日"));
                    row.GetCell(1).SetCellValue(item.ItemTitle ?? string.Empty);
                    row.GetCell(2).SetCellValue(item.ItemContent ?? string.Empty);
                    row.GetCell(3).SetCellValue(item.CategoryId);
                    row.GetCell(4).SetCellValue((int)item.Status);
                    row.GetCell(5).SetCellValue(item.Progress ?? 0);
                    row.GetCell(6).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(7).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(8).SetCellValue(item.Tags ?? string.Empty);
                    row.GetCell(9).SetCellValue(item.SortOrder ?? 0);
                    // 应用列样式（数字居中、时间右对齐、标题加粗、全表格边框）
                    row.GetCell(0).CellStyle = blockStyle;     // 日期
                    row.GetCell(1).CellStyle = titleStyle;     // 标题（加粗）
                    row.GetCell(2).CellStyle = blockStyle;     // 内容
                    row.GetCell(3).CellStyle = numberStyle;    // 分类ID（居中）
                    row.GetCell(4).CellStyle = numberStyle;    // 状态（居中）
                    row.GetCell(5).CellStyle = numberStyle;    // 进度（居中）
                    row.GetCell(6).CellStyle = timeStyle;      // 开始时间（右对齐）
                    row.GetCell(7).CellStyle = timeStyle;      // 结束时间（右对齐）
                    row.GetCell(8).CellStyle = blockStyle;     // 标签
                    row.GetCell(9).CellStyle = numberStyle;    // 排序（居中）
                    // 收集当日总结（取第一条非空）
                    if (isFirstDataRow && !string.IsNullOrWhiteSpace(item.DailySummary))
                        groupSummary = item.DailySummary;
                    isFirstDataRow = false;
                }
                // 每个日期块的最后一行写入当日总结（标题=当日总结，内容=总结文本）
                rowIndex++;
                var summaryRow = sheet.CreateRow(rowIndex);
                for (int c = 0; c < HeaderZh.Length; c++) summaryRow.CreateCell(c);
                summaryRow.GetCell(1).SetCellValue("当日总结");
                summaryRow.GetCell(2).SetCellValue(groupSummary ?? string.Empty);
                for (int c = 0; c < HeaderZh.Length; c++)
                {
                    summaryRow.GetCell(c).CellStyle = blockStyle;
                }
                summaryRow.GetCell(1).CellStyle = titleStyle; // 当日总结标题加粗
                // 合并内容列以更好显示总结
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, HeaderZh.Length - 1));
                blockIndex++;
            }

            // 列宽适配（可读性优化，内容列加宽，数字列缩窄）
            sheet.SetColumnWidth(0, 20 * 256); // 日期
            sheet.SetColumnWidth(1, 30 * 256); // 标题
            sheet.SetColumnWidth(2, 80 * 256); // 内容（很长）
            sheet.SetColumnWidth(3, 10 * 256); // 分类ID
            sheet.SetColumnWidth(4, 8 * 256);  // 状态
            sheet.SetColumnWidth(5, 8 * 256);  // 进度
            sheet.SetColumnWidth(6, 12 * 256); // 开始时间
            sheet.SetColumnWidth(7, 12 * 256); // 结束时间
            sheet.SetColumnWidth(8, 10 * 256); // 标签
            sheet.SetColumnWidth(9, 8 * 256);  // 排序
        }

        private static string GetChineseWeekday(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday: return "星期一";
                case DayOfWeek.Tuesday: return "星期二";
                case DayOfWeek.Wednesday: return "星期三";
                case DayOfWeek.Thursday: return "星期四";
                case DayOfWeek.Friday: return "星期五";
                case DayOfWeek.Saturday: return "星期六";
                default: return "星期日";
            }
        }

        public IEnumerable<WorkLogItem> ImportMonth(DateTime month, string inputDirectory)
        {
            var list = new List<WorkLogItem>();
            if (string.IsNullOrWhiteSpace(inputDirectory)) return list;
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(inputDirectory, fileName);
            if (!File.Exists(filePath)) return list;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var wb = new XSSFWorkbook(fs);
                var sheet = wb.GetSheet(SheetName) ?? (wb.NumberOfSheets > 0 ? wb.GetSheetAt(0) : null);
                if (sheet == null) return list;

                var indexes = GetHeaderIndexes(sheet);
                DateTime currentDate = DateTime.MinValue;
                var groupItems = new List<WorkLogItem>();
                string currentSummary = null;

                for (int r = 1; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;
                    var firstCellText = GetString(row, 0);
                    if (!string.IsNullOrWhiteSpace(firstCellText) && firstCellText.StartsWith("====="))
                    {
                        var m = Regex.Match(firstCellText, "=+\\s*(\\d{4}年\\d{2}月\\d{2}日)(?:\\s+星期[一二三四五六日天])?\\s*=+");
                        if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyy年MM月dd日", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            // flush previous group
                            if (groupItems.Count > 0)
                            {
                                if (!string.IsNullOrWhiteSpace(currentSummary))
                                    groupItems[0].DailySummary = currentSummary;
                                list.AddRange(groupItems);
                                groupItems.Clear();
                                currentSummary = null;
                            }
                            currentDate = dt;
                        }
                        continue;
                    }

                    // 识别总结行（标题=当日总结），仅记录总结文本
                    var title = GetString(row, indexes["ItemTitle"]);
                    if (string.Equals(title, "当日总结", StringComparison.OrdinalIgnoreCase))
                    {
                        currentSummary = GetString(row, indexes["ItemContent"]);
                        continue;
                    }

                    var item = new WorkLogItem();
                    item.LogDate = currentDate != DateTime.MinValue
                        ? currentDate
                        : ParseNullableDateTime(GetString(row, indexes["LogDate"]))?.Date ?? monthStart;
                    item.ItemTitle = GetString(row, indexes["ItemTitle"]);
                    item.ItemContent = GetString(row, indexes["ItemContent"]);
                    item.CategoryId = ParseInt(GetString(row, indexes["CategoryId"]));
                    item.Status = ParseStatus(GetString(row, indexes["Status"]));
                    item.Progress = ParseNullableInt(GetString(row, indexes["Progress"]));
                    item.StartTime = ParseNullableDateTime(GetString(row, indexes["StartTime"]));
                    item.EndTime = ParseNullableDateTime(GetString(row, indexes["EndTime"]));
                    item.Tags = GetString(row, indexes["Tags"]);
                    item.SortOrder = ParseNullableInt(GetString(row, indexes["SortOrder"]));
                    groupItems.Add(item);
                }
                // flush last group
                if (groupItems.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(currentSummary))
                        groupItems[0].DailySummary = currentSummary;
                    list.AddRange(groupItems);
                }
            }
            return list;
        }

        public IEnumerable<WorkLogItem> ImportFromFile(string filePath)
        {
            var list = new List<WorkLogItem>();
            if (string.IsNullOrWhiteSpace(filePath)) return list;
            if (!File.Exists(filePath)) return list;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var wb = new XSSFWorkbook(fs);
                var sheet = wb.GetSheet(SheetName) ?? (wb.NumberOfSheets > 0 ? wb.GetSheetAt(0) : null);
                if (sheet == null) return list;

                var indexes = GetHeaderIndexes(sheet);
                DateTime currentDate = DateTime.MinValue;
                var groupItems = new List<WorkLogItem>();
                string currentSummary = null;

                for (int r = 1; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;
                    var firstCellText = GetString(row, 0);
                    if (!string.IsNullOrWhiteSpace(firstCellText) && firstCellText.StartsWith("====="))
                    {
                        var m = Regex.Match(firstCellText, "=+\\s*(\\d{4}年\\d{2}月\\d{2}日)(?:\\s+星期[一二三四五六日天])?\\s*=+");
                        if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyy年MM月dd日", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            // flush previous group
                            if (groupItems.Count > 0)
                            {
                                if (!string.IsNullOrWhiteSpace(currentSummary))
                                    groupItems[0].DailySummary = currentSummary;
                                list.AddRange(groupItems);
                                groupItems.Clear();
                                currentSummary = null;
                            }
                            currentDate = dt;
                        }
                        continue;
                    }

                    // 识别总结行（标题=当日总结）
                    var title = GetString(row, indexes["ItemTitle"]);
                    if (string.Equals(title, "当日总结", StringComparison.OrdinalIgnoreCase))
                    {
                        currentSummary = GetString(row, indexes["ItemContent"]);
                        continue;
                    }

                    var item = new WorkLogItem();
                    item.LogDate = currentDate != DateTime.MinValue
                        ? currentDate
                        : ParseNullableDateTime(GetString(row, indexes["LogDate"]))?.Date ?? DateTime.MinValue;
                    item.ItemTitle = GetString(row, indexes["ItemTitle"]);
                    item.ItemContent = GetString(row, indexes["ItemContent"]);
                    item.CategoryId = ParseInt(GetString(row, indexes["CategoryId"]));
                    item.Status = ParseStatus(GetString(row, indexes["Status"]));
                    item.Progress = ParseNullableInt(GetString(row, indexes["Progress"]));
                    item.StartTime = ParseNullableDateTime(GetString(row, indexes["StartTime"]));
                    item.EndTime = ParseNullableDateTime(GetString(row, indexes["EndTime"]));
                    item.Tags = GetString(row, indexes["Tags"]);
                    item.SortOrder = ParseNullableInt(GetString(row, indexes["SortOrder"]));
                    groupItems.Add(item);
                }
                // flush last group
                if (groupItems.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(currentSummary))
                        groupItems[0].DailySummary = currentSummary;
                    list.AddRange(groupItems);
                }
            }
            return list;
        }


        private static string GetString(IRow row, int index)
        {
            var cell = row.GetCell(index);
            if (cell == null) return string.Empty;
            cell.SetCellType(CellType.String);
            return cell.StringCellValue ?? string.Empty;
        }

        private static int ParseInt(string s)
        {
            int v; return int.TryParse(s, out v) ? v : 0;
        }

        private static int? ParseNullableInt(string s)
        {
            int v; return int.TryParse(s, out v) ? (int?)v : null;
        }

        private static DateTime? ParseNullableDateTime(string s)
        {
            DateTime dt; return DateTime.TryParse(s, out dt) ? (DateTime?)dt : null;
        }

        private static StatusEnum ParseStatus(string s)
        {
            // 优先尝试解析为数值枚举
            if (int.TryParse(s, out var iv))
            {
                if (Enum.IsDefined(typeof(StatusEnum), iv))
                    return (StatusEnum)iv;
            }
            // 兼容旧文件以字符串方式存储
            if (Enum.TryParse<StatusEnum>(s, out var status)) return status;
            return StatusEnum.Todo;
        }

        private static Dictionary<string, int> GetHeaderIndexes(ISheet sheet)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var headerRow = sheet.GetRow(0);
            if (headerRow == null || headerRow.PhysicalNumberOfCells == 0)
            {
                for (int i = 0; i < Header.Length; i++) dict[Header[i]] = i;
                return dict;
            }
            for (int c = 0; c < headerRow.LastCellNum; c++)
            {
                var name = GetString(headerRow, c);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (HeaderNameMap.TryGetValue(name, out var key))
                    {
                        dict[key] = c;
                    }
                    else
                    {
                        dict[name] = c;
                    }
                }
            }
            foreach (var col in Header)
            {
                if (!dict.ContainsKey(col))
                {
                    var idx = Array.IndexOf(Header, col);
                    dict[col] = idx >= 0 ? idx : dict.Count;
                }
            }
            return dict;
        }
    }
}