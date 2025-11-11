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
            "LogDate","ItemTitle","ItemContent","Status","CategoryId","Progress","StartTime","EndTime","Tags","SortOrder"
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
                    if (firstRow == null || firstRow.PhysicalNumberOfCells == 0)
                        startRow = 0; // 无表头则从0开始

                    for (int r = startRow; r <= sheet.LastRowNum; r++)
                    {
                        var row = sheet.GetRow(r);
                        if (row == null) continue;
                        var item = new WorkLogItem { LogDate = logDate };
                        item.ItemTitle = GetString(row, 1);
                        item.ItemContent = GetString(row, 2);
                        item.Status = ParseStatus(GetString(row, 3));
                        item.CategoryId = ParseInt(GetString(row, 4));
                        item.Progress = ParseNullableInt(GetString(row, 5));
                        item.StartTime = ParseNullableDateTime(GetString(row, 6));
                        item.EndTime = ParseNullableDateTime(GetString(row, 7));
                        item.Tags = GetString(row, 8);
                        item.SortOrder = ParseNullableInt(GetString(row, 9));
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
            row.CreateCell(0).SetCellValue(item.LogDate.ToString("yyyy-MM-dd"));
            row.CreateCell(1).SetCellValue(item.ItemTitle ?? string.Empty);
            row.CreateCell(2).SetCellValue(item.ItemContent ?? string.Empty);
            row.CreateCell(3).SetCellValue(item.Status.ToString());
            row.CreateCell(4).SetCellValue(item.CategoryId);
            row.CreateCell(5).SetCellValue(item.Progress.HasValue ? item.Progress.Value : 0);
            row.CreateCell(6).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
            row.CreateCell(7).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
            row.CreateCell(8).SetCellValue(item.Tags ?? string.Empty);
            row.CreateCell(9).SetCellValue(item.SortOrder.HasValue ? item.SortOrder.Value : 0);
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
            if (Enum.TryParse<StatusEnum>(s, out var status)) return status;
            return StatusEnum.Todo;
        }
    }
}