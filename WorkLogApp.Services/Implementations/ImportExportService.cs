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
            // 按设计文档的列顺序（并追加 DailySummary）
            "LogDate","ItemTitle","ItemContent","CategoryId","Status","Progress","StartTime","EndTime","Tags","SortOrder","DailySummary"
        };
        private static readonly string[] HeaderZh = new[]
        {
            // 与 Header 对应的中文显示名称
            "日期","标题","内容","分类ID","状态","进度","开始时间","结束时间","标签","排序","当日总结"
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
            {"排序","SortOrder"},
            {"当日总结","DailySummary"}
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
                header.CreateCell(i).SetCellValue(HeaderZh[i]);
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

            var markerStyleB = wb.CreateCellStyle();
            markerStyleB.SetFont(boldFont);
            markerStyleB.FillPattern = FillPattern.SolidForeground;
            markerStyleB.FillForegroundColor = IndexedColors.LightYellow.Index;
            markerStyleB.Alignment = HorizontalAlignment.Center;
            markerStyleB.VerticalAlignment = VerticalAlignment.Center;

            var blockStyleA = wb.CreateCellStyle();
            blockStyleA.FillPattern = FillPattern.SolidForeground;
            blockStyleA.FillForegroundColor = IndexedColors.LightCornflowerBlue.Index;
            blockStyleA.WrapText = true;

            var blockStyleB = wb.CreateCellStyle();
            blockStyleB.FillPattern = FillPattern.SolidForeground;
            blockStyleB.FillForegroundColor = IndexedColors.LightYellow.Index;
            blockStyleB.WrapText = true;

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

                // 日期标识行
                rowIndex++;
                var markerRow = sheet.CreateRow(rowIndex);
                for (int c = 0; c < HeaderZh.Length; c++)
                {
                    var cell = markerRow.CreateCell(c);
                    cell.CellStyle = markerStyle;
                    cell.SetCellValue(c == 0 ? $"===== {group.Key:yyyy-MM-dd} =====" : string.Empty);
                }
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, HeaderZh.Length - 1));

                bool isFirstDataRow = true;
                foreach (var item in group)
                {
                    rowIndex++;
                    var row = sheet.CreateRow(rowIndex);
                    for (int c = 0; c < HeaderZh.Length; c++)
                    {
                        var cell = row.CreateCell(c);
                        cell.CellStyle = blockStyle;
                    }

                    row.GetCell(0).SetCellValue(item.LogDate.ToString("yyyy-MM-dd"));
                    row.GetCell(1).SetCellValue(item.ItemTitle ?? string.Empty);
                    row.GetCell(2).SetCellValue(item.ItemContent ?? string.Empty);
                    row.GetCell(3).SetCellValue(item.CategoryId);
                    row.GetCell(4).SetCellValue((int)item.Status);
                    row.GetCell(5).SetCellValue(item.Progress ?? 0);
                    row.GetCell(6).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(7).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(8).SetCellValue(item.Tags ?? string.Empty);
                    row.GetCell(9).SetCellValue(item.SortOrder ?? 0);
                    row.GetCell(10).SetCellValue(isFirstDataRow ? (item.DailySummary ?? string.Empty) : string.Empty);
                    isFirstDataRow = false;
                }
                blockIndex++;
            }

            // 列宽适配（可读性优化）
            for (int i = 0; i < HeaderZh.Length; i++)
            {
                sheet.SetColumnWidth(i, 20 * 256);
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

                for (int r = 1; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;
                    var firstCellText = GetString(row, 0);
                    if (!string.IsNullOrWhiteSpace(firstCellText) && firstCellText.StartsWith("====="))
                    {
                        var m = Regex.Match(firstCellText, "=+\\s*(\\d{4}-\\d{2}-\\d{2})\\s*=+");
                        if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            currentDate = dt;
                        }
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
                    item.DailySummary = GetString(row, indexes["DailySummary"]);
                    list.Add(item);
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
                for (int i = 0; i < wb.NumberOfSheets; i++)
                {
                    var sheet = wb.GetSheetAt(i);
                    if (sheet == null) continue;
                    DateTime logDate;
                    if (!DateTime.TryParseExact(sheet.SheetName, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out logDate))
                        continue;

                    var firstRow = sheet.GetRow(0);
                    int startRow = 1;
                    var hasHeader = !(firstRow == null || firstRow.PhysicalNumberOfCells == 0);
                    if (!hasHeader)
                        startRow = 0;

                    var indexes = GetHeaderIndexes(sheet);

                    for (int r = startRow; r <= sheet.LastRowNum; r++)
                    {
                        var row = sheet.GetRow(r);
                        if (row == null) continue;
                        var item = new WorkLogItem { LogDate = logDate };
                        item.ItemTitle = GetString(row, indexes["ItemTitle"]);
                        item.ItemContent = GetString(row, indexes["ItemContent"]);
                        item.CategoryId = ParseInt(GetString(row, indexes["CategoryId"]));
                        item.Status = ParseStatus(GetString(row, indexes["Status"]));
                        item.Progress = ParseNullableInt(GetString(row, indexes["Progress"]));
                        item.StartTime = ParseNullableDateTime(GetString(row, indexes["StartTime"]));
                        item.EndTime = ParseNullableDateTime(GetString(row, indexes["EndTime"]));
                        item.Tags = GetString(row, indexes["Tags"]);
                        item.SortOrder = ParseNullableInt(GetString(row, indexes["SortOrder"]));
                        var isFirstDataRow = hasHeader ? (r == 1) : (r == 0);
                        if (indexes.ContainsKey("DailySummary") && isFirstDataRow)
                            item.DailySummary = GetString(row, indexes["DailySummary"]);
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        private static void WriteItem(IWorkbook wb, WorkLogItem item)
        {
            var sheetName = item.LogDate.ToString("yyyy-MM-dd");
            var sheet = wb.GetSheet(sheetName) ?? wb.CreateSheet(sheetName);
            if (sheet.PhysicalNumberOfRows == 0)
            {
                var header = sheet.CreateRow(0);
                for (int i = 0; i < HeaderZh.Length; i++)
                {
                    header.CreateCell(i).SetCellValue(HeaderZh[i]);
                }
            }
            else
            {
                // 如果已存在表头且为英文，则替换为中文显示
                var headerRow = sheet.GetRow(0);
                if (headerRow != null && headerRow.PhysicalNumberOfCells > 0)
                {
                    var rewriteToChinese = false;
                    for (int i = 0; i < Header.Length && i < headerRow.LastCellNum; i++)
                    {
                        var name = GetString(headerRow, i);
                        if (string.Equals(name, Header[i], StringComparison.OrdinalIgnoreCase))
                        {
                            rewriteToChinese = true;
                            break;
                        }
                    }
                    if (rewriteToChinese)
                    {
                        for (int i = 0; i < HeaderZh.Length; i++)
                        {
                            var cell = headerRow.GetCell(i) ?? headerRow.CreateCell(i);
                            cell.SetCellValue(HeaderZh[i]);
                        }
                    }
                }
            }
            var rowIndex = Math.Max(sheet.LastRowNum + 1, 1);
            var row = sheet.CreateRow(rowIndex);
            // 使用新列顺序
            row.CreateCell(0).SetCellValue(item.LogDate.ToString("yyyy-MM-dd"));
            row.CreateCell(1).SetCellValue(item.ItemTitle ?? string.Empty);
            row.CreateCell(2).SetCellValue(item.ItemContent ?? string.Empty);
            row.CreateCell(3).SetCellValue(item.CategoryId);
            row.CreateCell(4).SetCellValue((int)item.Status);
            row.CreateCell(5).SetCellValue(item.Progress.HasValue ? item.Progress.Value : 0);
            row.CreateCell(6).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
            row.CreateCell(7).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
            row.CreateCell(8).SetCellValue(item.Tags ?? string.Empty);
            row.CreateCell(9).SetCellValue(item.SortOrder.HasValue ? item.SortOrder.Value : 0);
            // 当日总结仅在第一条记录中写入
            var dailySummaryCell = row.CreateCell(10);
            if (rowIndex == 1)
                dailySummaryCell.SetCellValue(item.DailySummary ?? string.Empty);
            else
                dailySummaryCell.SetCellValue(string.Empty);
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
                if (!dict.ContainsKey("DailySummary"))
                {
                    dict["DailySummary"] = 10;
                }
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