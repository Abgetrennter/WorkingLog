using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class ImportExportService : IImportExportService
    {
        private const string FilePrefix = "worklog_";
        private static readonly string[] Header = new[]
        {
            // 按设计文档的列顺序（并追加 DailySummary）
            "LogDate","ItemTitle","ItemContent","CategoryId","Status","Progress","StartTime","EndTime","Tags","SortOrder","DailySummary"
        };

        public bool ExportMonth(DateTime month, IEnumerable<WorkLogItem> items, string outputDirectory)
        {
            if (items == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            IWorkbook wb = null;
            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    wb = new XSSFWorkbook(fs);
                }
            }
            else
            {
                wb = new XSSFWorkbook();
            }

            foreach (var item in items)
            {
                if (item == null) continue;
                if (item.LogDate.Year != month.Year || item.LogDate.Month != month.Month) continue;
                WriteItem(wb, item);
            }

            using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                wb.Write(outFs);
            }
            return true;
        }

        // 覆盖写入整月数据：重新生成工作簿并写入传入的所有当月记录
        public bool RewriteMonth(DateTime month, IEnumerable<WorkLogItem> items, string outputDirectory)
        {
            if (items == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            IWorkbook wb = new XSSFWorkbook();
            foreach (var item in items)
            {
                if (item == null) continue;
                if (item.LogDate.Year != month.Year || item.LogDate.Month != month.Month) continue;
                WriteItem(wb, item);
            }
            using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                wb.Write(outFs);
            }
            return true;
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
                        startRow = 0; // 无表头则从0开始

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
                for (int i = 0; i < Header.Length; i++)
                {
                    header.CreateCell(i).SetCellValue(Header[i]);
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
                // 无表头，使用默认顺序（兼容旧格式）
                for (int i = 0; i < Header.Length; i++) dict[Header[i]] = i;
                // 兼容旧列顺序（没有 DailySummary，Status 在 CategoryId 之前）
                if (!dict.ContainsKey("DailySummary"))
                {
                    dict["DailySummary"] = 10; // 不存在时占位
                }
                return dict;
            }
            for (int c = 0; c < headerRow.LastCellNum; c++)
            {
                var name = GetString(headerRow, c);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (!dict.ContainsKey(name)) dict[name] = c;
                }
            }
            // 确保所有关键列都有索引（若缺失则用当前设计默认）
            foreach (var col in Header)
            {
                if (!dict.ContainsKey(col))
                {
                    // 回退到当前设计的固定索引
                    var idx = Array.IndexOf(Header, col);
                    dict[col] = idx >= 0 ? idx : dict.Count;
                }
            }
            return dict;
        }
    }
}