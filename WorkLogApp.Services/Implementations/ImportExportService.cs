using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class ImportExportService : IImportExportService
    {
        public const string FilePrefix = "工作日志_";
        private const string LegacyFilePrefix = "worklog_";
        private const string SheetName = "工作日志";
        private static readonly string[] Header = new[]
        {
            "LogDate","ItemTitle","ItemContent","CategoryName","Status","StartTime","EndTime","Tags","SortOrder","Id","TrackingId"
        };
        private static readonly string[] HeaderZh = new[]
        {
            "日期","标题","内容","分类","状态","开始时间","结束时间","标签","排序","日志ID","追踪ID"
        };
        private static readonly Dictionary<string, string> HeaderNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"日期","LogDate"},
            {"标题","ItemTitle"},
            {"内容","ItemContent"},
            {"分类","CategoryName"},
            {"分类ID","CategoryName"}, // 兼容旧文件
            {"状态","Status"},
            {"开始时间","StartTime"},
            {"结束时间","EndTime"},
            {"标签","Tags"},
            {"排序","SortOrder"},
            {"日志ID","Id"},
            {"追踪ID","TrackingId"},
            {"TrackingId","TrackingId"}
        };

        // 列名常量
        private const string ColumnItemTitle = "ItemTitle";
        private const string ColumnItemContent = "ItemContent";
        private const string ColumnLogDate = "LogDate";
        private const string ColumnCategoryName = "CategoryName";
        private const string ColumnCategoryId = "CategoryId";
        private const string ColumnStatus = "Status";
        private const string ColumnStartTime = "StartTime";
        private const string ColumnEndTime = "EndTime";
        private const string ColumnTags = "Tags";
        private const string ColumnSortOrder = "SortOrder";
        private const string ColumnId = "Id";
        private const string ColumnTrackingId = "TrackingId";
        private const string SummaryTitle = "当日总结";

        public bool ExportMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory)
        {
            if (days == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            var existingDays = ImportMonth(month, outputDirectory) ?? Enumerable.Empty<WorkLog>();
            var newDays = days.Where(d => d != null && d.LogDate.Year == month.Year && d.LogDate.Month == month.Month);
            var combined = existingDays
                .Concat(newDays)
                .GroupBy(d => d.LogDate.Date)
                .Select(g => new WorkLog
                {
                    LogDate = g.Key,
                    DailySummary = g.Select(x => x.DailySummary).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty,
                    Items = g.SelectMany(x => x.Items ?? new List<WorkLogItem>()).ToList()
                })
                .ToList();

            return RewriteMonth(month, combined, outputDirectory);
        }

        // 覆盖写入整月数据：生成按周分组的多个工作表
        public bool RewriteMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory)
        {
            if (days == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            IWorkbook wb = new XSSFWorkbook();
            
            var validDays = (days ?? Enumerable.Empty<WorkLog>())
                .Where(d => d != null && d.LogDate.Year == month.Year && d.LogDate.Month == month.Month)
                .OrderBy(d => d.LogDate)
                .ToList();

            if (validDays.Any())
            {
                // 按周分组（周一为一周开始）
                var weeks = validDays.GroupBy(d =>
                {
                    // Calculate Monday of the week
                    var diff = (7 + (d.LogDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    return d.LogDate.Date.AddDays(-1 * diff);
                }).OrderBy(g => g.Key);

                foreach (var weekGroup in weeks)
                {
                    var weekDays = weekGroup.OrderBy(d => d.LogDate).ToList();
                    if (!weekDays.Any()) continue;

                    var firstDay = weekDays.First().LogDate;
                    var lastDay = weekDays.Last().LogDate;
                    
                    // Sheet naming: "d日-d日" (Actual data range)
                    string sheetName = $"{firstDay.Day}日-{lastDay.Day}日";
                    
                    // Ensure unique sheet name (just in case)
                    int suffix = 1;
                    string tempName = sheetName;
                    while (wb.GetSheet(tempName) != null)
                    {
                        tempName = $"{sheetName}_{suffix++}";
                    }
                    sheetName = tempName;

                    WriteSheet(wb, sheetName, weekDays, month);
                }
            }
            else
            {
                // No data, create a default empty sheet
                WriteSheet(wb, SheetName, new List<WorkLog>(), month);
            }

            using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                wb.Write(outFs);
            }
            return true;
        }

        private static void WriteSheet(IWorkbook wb, string sheetName, IEnumerable<WorkLog> days, DateTime monthContext)
        {
            var sheet = wb.CreateSheet(sheetName);

            // 准备模板名称映射：CategoryId(GUID) → 模板名称
            var idToName = new Dictionary<string, string>();
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var templatesPath = Path.Combine(baseDir, "Templates", "data.json");
                var tplSvc = new TemplateService();
                if (tplSvc.LoadTemplates(templatesPath))
                {
                    foreach (var cat in tplSvc.GetAllCategories())
                    {
                        idToName[cat.Id] = cat.Name;
                    }
                }
            }
            catch { /* 忽略模板加载失败 */ }

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

            var orderedDays = (days ?? Enumerable.Empty<WorkLog>())
                .Where(d => d != null && d.LogDate.Year == monthContext.Year && d.LogDate.Month == monthContext.Month)
                .OrderBy(d => d.LogDate.Date)
                .ToList();

            int rowIndex = 0;
            int blockIndex = 0;
            foreach (var day in orderedDays)
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
                    var week = GetChineseWeekday(day.LogDate.Date);
                    cell.SetCellValue(c == 0 ? $"—— {day.LogDate:yyyy年MM月dd日} {week} ——" : string.Empty);
                }
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, HeaderZh.Length - 1));

                var itemsOrdered = (day.Items ?? new List<WorkLogItem>())
                    .OrderBy(i => i.SortOrder ?? 0)
                    .ThenBy(i => i.StartTime ?? DateTime.MinValue)
                    .ThenBy(i => i.ItemTitle ?? string.Empty)
                    .ToList();
                foreach (var item in itemsOrdered)
                {
                    rowIndex++;
                    var row = sheet.CreateRow(rowIndex);
                    for (int c = 0; c < HeaderZh.Length; c++) row.CreateCell(c);

                    // 中文日期格式
                    row.GetCell(0).SetCellValue(day.LogDate.ToString("yyyy年MM月dd日"));
                    row.GetCell(1).SetCellValue(item.ItemTitle ?? string.Empty);
                    row.GetCell(2).SetCellValue(item.ItemContent ?? string.Empty);
                    // 分类列直接写名称
                    var catVal = item.CategoryName ?? string.Empty;
                    if (idToName.ContainsKey(catVal)) catVal = idToName[catVal];
                    row.GetCell(3).SetCellValue(catVal);
                    row.GetCell(4).SetCellValue(item.Status.ToChinese());
                    row.GetCell(5).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(6).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(7).SetCellValue(item.Tags ?? string.Empty);
                    row.GetCell(8).SetCellValue(item.SortOrder ?? 0);
                    row.GetCell(9).SetCellValue(item.Id ?? string.Empty);
                    row.GetCell(10).SetCellValue(item.TrackingId ?? string.Empty);
                    row.GetCell(0).CellStyle = blockStyle;
                    row.GetCell(1).CellStyle = titleStyle;
                    row.GetCell(2).CellStyle = blockStyle;
                    row.GetCell(3).CellStyle = numberStyle;
                    row.GetCell(4).CellStyle = blockStyle;
                    row.GetCell(5).CellStyle = timeStyle;
                    row.GetCell(6).CellStyle = timeStyle;
                    row.GetCell(7).CellStyle = blockStyle;
                    row.GetCell(8).CellStyle = numberStyle;
                    row.GetCell(9).CellStyle = blockStyle;
                    row.GetCell(10).CellStyle = blockStyle;
                }
                // 每个日期块的最后一行写入当日总结（标题=当日总结，内容=总结文本）
                rowIndex++;
                var summaryRow = sheet.CreateRow(rowIndex);
                for (int c = 0; c < HeaderZh.Length; c++) summaryRow.CreateCell(c);
                summaryRow.GetCell(1).SetCellValue(SummaryTitle);
                summaryRow.GetCell(2).SetCellValue(day.DailySummary ?? string.Empty);
                for (int c = 0; c < HeaderZh.Length; c++)
                {
                    summaryRow.GetCell(c).CellStyle = blockStyle;
                }
                summaryRow.GetCell(1).CellStyle = titleStyle;
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, HeaderZh.Length - 1));
                blockIndex++;
            }

            // 列宽适配（可读性优化，内容列加宽，数字列缩窄）
            sheet.SetColumnWidth(0, 20 * 256);
            sheet.SetColumnWidth(1, 30 * 256);
            sheet.SetColumnWidth(2, 80 * 256);
            sheet.SetColumnWidth(3, 12 * 256);
            sheet.SetColumnWidth(4, 10 * 256);
            sheet.SetColumnWidth(5, 12 * 256);
            sheet.SetColumnWidth(6, 12 * 256);
            sheet.SetColumnWidth(7, 10 * 256);
            sheet.SetColumnWidth(8, 8 * 256);
            sheet.SetColumnWidth(9, 36 * 256);
            sheet.SetColumnWidth(10, 30 * 256);
            sheet.SetColumnHidden(9, true);
            sheet.SetColumnHidden(10, true);
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

        public IEnumerable<WorkLog> ImportMonth(DateTime month, string inputDirectory)
        {
            if (string.IsNullOrWhiteSpace(inputDirectory)) return new List<WorkLog>();
            var monthStart = new DateTime(month.Year, month.Month, 1);
            
            // Try new name first
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(inputDirectory, fileName);

            // If not found, try legacy name
            if (!File.Exists(filePath))
            {
                var legacyName = LegacyFilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
                var legacyPath = Path.Combine(inputDirectory, legacyName);
                if (File.Exists(legacyPath))
                {
                    filePath = legacyPath;
                }
            }

            return ImportFromFile(filePath);
        }

        public IEnumerable<WorkLog> ImportFromFile(string filePath)
        {
            return ImportFromFileWithDiagnostics(filePath).Data;
        }

        public ImportResult ImportFromFileWithDiagnostics(string filePath)
        {
            var result = new ImportResult();
            
            // DEBUG LOGGING
            bool debugMode = false;
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "worklog_import_debug.txt");
            void Log(string msg) 
            { 
                if (!debugMode) return;
                try { File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n"); } catch {} 
            }
            
            Log("================ START IMPORT ================");
            Log($"File: {filePath}");

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) 
            {
                var msg = $"File not found: {filePath}";
                result.Errors.Add(msg);
                Log(msg);
                return result;
            }

            // Use a temp file to avoid locking issues with WPS/Excel
            string tempPath = Path.Combine(Path.GetTempPath(), $"worklog_import_{Guid.NewGuid()}{Path.GetExtension(filePath)}");
            Log($"Temp Path: {tempPath}");

            try 
            {
                // Try to copy the file first. This often resolves "file in use" errors better than just FileShare.ReadWrite
                File.Copy(filePath, tempPath, true);
            }
            catch (Exception ex)
            {
                var msg = $"Failed to copy to temp file (File might be locked): {ex.Message}";
                result.Errors.Add(msg);
                Log(msg);
                return result;
            }

            try
            {
                using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    IWorkbook wb;
                    try
                    {
                        wb = WorkbookFactory.Create(fs);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to open Excel file: {ex.Message}");
                        Log($"Open Error (Full): {ex}");
                        return result;
                    }

                    var dayMap = new Dictionary<DateTime, WorkLog>();
                    
                    // Try to guess month from filename if possible, for fallback
                    var match = Regex.Match(Path.GetFileName(filePath), @"(\d{6})");
                    var monthStart = DateTime.MinValue;
                    if (match.Success && DateTime.TryParseExact(match.Groups[1].Value, "yyyyMM", null, DateTimeStyles.None, out var parsedDate))
                    {
                        monthStart = parsedDate;
                        Log($"Month from filename: {monthStart:yyyy-MM}");
                    }

                    for (int sIdx = 0; sIdx < wb.NumberOfSheets; sIdx++)
                    {
                        var sheet = wb.GetSheetAt(sIdx);
                        if (sheet == null) continue;
                        if (wb.IsSheetHidden(sIdx)) continue;
                        
                        Log($"Processing Sheet [{sIdx}]: {sheet.SheetName}, LastRowNum: {sheet.LastRowNum}");
                        ParseSheet(sheet, dayMap, result.Errors, Log, monthStart);
                    }
                    
                    result.Data = dayMap.Values.OrderBy(d => d.LogDate.Date).ToList();
                    Log($"Import Finished. Total Days: {result.Data.Count}, Total Items: {result.Data.Sum(d => d.Items.Count)}");
                    
                    /*



                    for (int r = startRow; r <= sheet.LastRowNum; r++)
                    {
                        try
                        {
                            var row = sheet.GetRow(r);
                            if (row == null) continue;
                            var firstCellText = GetString(row, 0);
                            
                            // Move retrieval early to help distinguish row types
                            var title = GetValue(row, indexes, "ItemTitle");
                            var contentVal = GetValue(row, indexes, "ItemContent");

                            // Log first few chars of row for context
                            // Log($"Row {r} FirstCell: {firstCellText}");

                            if (!string.IsNullOrWhiteSpace(firstCellText))
                            {
                                // Loose check for date row: contains "Year" "Month" "Day" or starts with "——"
                                // FIX: Work items also have date in first cell. Must check if Title/Content is empty to confirm it is a Section Header.
                                bool isExplicitMarker = firstCellText.StartsWith("——");
                                bool looksLikeDate = firstCellText.Contains("年") && firstCellText.Contains("月") && firstCellText.Contains("日");
                                
                                bool isDateRow = isExplicitMarker || (looksLikeDate && string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(contentVal));
                                
                                if (isDateRow)
                                {
                                    var m = Regex.Match(firstCellText, @"(\d{4})年(\d{1,2})月(\d{1,2})日");
                                    if (m.Success)
                                    {
                                        if (DateTime.TryParseExact(m.Value, "yyyy年M月d日", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                                        {
                                            currentDate = dt.Date;
                                            Log($"[Row {r}] Found Date Row: {currentDate:yyyy-MM-dd}");
                                            if (!dayMap.ContainsKey(currentDate))
                                            {
                                                dayMap[currentDate] = new WorkLog { LogDate = currentDate, Items = new List<WorkLogItem>() };
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }

                            // 识别总结行（标题=当日总结）
                            // var title = GetValue(row, indexes, "ItemTitle"); // Moved up
                            if (string.Equals(title, "当日总结", StringComparison.OrdinalIgnoreCase))
                            {
                                var dt = currentDate != DateTime.MinValue
                                    ? currentDate
                                    : ParseNullableDateTime(GetValue(row, indexes, "LogDate"))?.Date ?? monthStart;
                                if (dt == DateTime.MinValue) { Log($"[Row {r}] Skip Summary: No Date"); continue; }

                                if (!dayMap.ContainsKey(dt))
                                {
                                    dayMap[dt] = new WorkLog { LogDate = dt, Items = new List<WorkLogItem>() };
                                }
                                dayMap[dt].DailySummary = contentVal;
                                Log($"[Row {r}] Read Summary for {dt:yyyy-MM-dd}");
                                continue;
                            }

                            // 尝试解析当前行的日期（备用方案）
                            DateTime itemDate = DateTime.MinValue;
                            var dateCellStr = GetValue(row, indexes, "LogDate");
                            if (!string.IsNullOrWhiteSpace(dateCellStr))
                            {
                                if (DateTime.TryParse(dateCellStr, out var d)) itemDate = d.Date;
                                else if (DateTime.TryParseExact(dateCellStr, "yyyyMMdd", null, DateTimeStyles.None, out d)) itemDate = d.Date;
                                // Add more formats if needed
                            }

                            // IMPORTANT FIX: If itemDate is found, it MUST take precedence.
                            // If no itemDate found, fallback to currentDate (from date row above).
                            // If currentDate is also missing, fallback to monthStart (from filename).
                            var dtItem = itemDate != DateTime.MinValue ? itemDate : (currentDate != DateTime.MinValue ? currentDate : monthStart);
                            
                            if (dtItem == DateTime.MinValue) 
                            {
                                // Skip rows that we can't determine date for, unless it's empty
                                if (string.IsNullOrWhiteSpace(title)) 
                                {
                                    // Empty row, skip silently
                                    continue;
                                }
                                Log($"[Row {r}] Skip Item: No Date. Title={title}, DateCell={dateCellStr}");
                                continue;
                            }

                            if (!dayMap.ContainsKey(dtItem))
                            {
                                dayMap[dtItem] = new WorkLog { LogDate = dtItem, Items = new List<WorkLogItem>() };
                            }
                            var item = new WorkLogItem();
                            item.LogDate = dtItem;
                            item.ItemTitle = title;
                            item.ItemContent = contentVal;
                            
                            // 读取分类名称
                            var catName = GetValue(row, indexes, "CategoryName");
                            // 兼容旧版本列名
                            if (string.IsNullOrWhiteSpace(catName)) catName = GetValue(row, indexes, "CategoryId");
                            item.CategoryName = catName;
                            
                            // Log item details for debugging
                            Log($"[Row {r}] Reading Item: Title='{item.ItemTitle}', Content='{item.ItemContent}', Status='{GetValue(row, indexes, "Status")}'");

                            // Skip completely empty items (no title, no content) to avoid clutter
                            if (string.IsNullOrWhiteSpace(item.ItemTitle) && string.IsNullOrWhiteSpace(item.ItemContent))
                            {
                                 continue;
                            }

                            var statusStr = GetValue(row, indexes, "Status");
                            item.Status = !string.IsNullOrEmpty(statusStr) ? StatusHelper.Parse(statusStr) : StatusEnum.Todo;

                            item.StartTime = ParseNullableDateTime(GetValue(row, indexes, "StartTime"));
                            item.EndTime = ParseNullableDateTime(GetValue(row, indexes, "EndTime"));
                            item.Tags = GetValue(row, indexes, "Tags");
                            item.SortOrder = ParseNullableInt(GetValue(row, indexes, "SortOrder"));
                            
                            var idStr = GetValue(row, indexes, "Id");
                            if (!string.IsNullOrWhiteSpace(idStr))
                            {
                                item.Id = idStr;
                            }
      
                            // 读取追踪ID
                            var trackingIdStr = GetValue(row, indexes, "TrackingId");
                            if (!string.IsNullOrWhiteSpace(trackingIdStr))
                            {
                                item.TrackingId = trackingIdStr;
                            }
                            else if (StatusHelper.IsIncomplete(item.Status))
                            {
                                // 迁移：为未完成条目自动生成追踪ID
                                item.TrackingId = Guid.NewGuid().ToString();
                            }
      
                            dayMap[dtItem].Items.Add(item);
                            // Log($"[Row {r}] Added Item: {item.ItemTitle} ({dtItem:yyyy-MM-dd})");
                        }
                        catch (Exception ex)
                        {
                            var msg = $"Row {r} parse error: {ex.Message}";
                            result.Errors.Add(msg);
                            Log(msg);
                        }
                    }
                    result.Data = dayMap.Values.OrderBy(d => d.LogDate.Date).ToList();
                    */
                }
            }
            catch (Exception ex)
            {
                var msg = $"File access error: {ex.Message}";
                result.Errors.Add(msg);
                Log(msg);
            }
            finally
            {
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch {}
            }
            return result;
        }

        public ImportResult ImportFromTxt(string filePath)
        {
            var result = new ImportResult();
            if (!File.Exists(filePath))
            {
                result.Errors.Add("File not found");
                return result;
            }
            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<List<WorkLog>>(json);
                if (data != null)
                {
                    result.Data = data;
                }
            }
            catch (Exception ex)
            {
                 result.Errors.Add("JSON Parse Error: " + ex.Message);
            }
            return result;
        }

        public ImportResult CompareAndVerify(IEnumerable<WorkLog> source, IEnumerable<WorkLog> target)
        {
            var result = new ImportResult();
            var srcList = source?.OrderBy(x => x.LogDate).ToList() ?? new List<WorkLog>();
            var tgtList = target?.OrderBy(x => x.LogDate).ToList() ?? new List<WorkLog>();

            if (srcList.Count != tgtList.Count)
            {
                 result.Errors.Add($"Day count mismatch: Source={srcList.Count}, Target={tgtList.Count}");
            }

            var srcDict = srcList.ToDictionary(x => x.LogDate.Date);
            var tgtDict = tgtList.ToDictionary(x => x.LogDate.Date);

            foreach (var kvp in srcDict)
            {
                if (!tgtDict.ContainsKey(kvp.Key))
                {
                    result.Errors.Add($"Date {kvp.Key:yyyy-MM-dd} missing in target.");
                    continue;
                }
                var sDay = kvp.Value;
                var tDay = tgtDict[kvp.Key];
                
                if (sDay.Items.Count != tDay.Items.Count)
                {
                     result.Errors.Add($"Date {kvp.Key:yyyy-MM-dd} item count mismatch: Source={sDay.Items.Count}, Target={tDay.Items.Count}");
                }
                // Can add deeper comparison if needed
            }

            foreach (var k in tgtDict.Keys)
            {
                if (!srcDict.ContainsKey(k))
                {
                    result.Errors.Add($"Date {k:yyyy-MM-dd} extra in target.");
                }
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }

        private static string GetValue(IRow row, Dictionary<string, int> indexes, string key)
        {
            if (indexes.TryGetValue(key, out var index))
            {
                return GetString(row, index);
            }
            return string.Empty;
        }

        private static string GetString(IRow row, int index)
        {
            var cell = row.GetCell(index);
            if (cell == null) return string.Empty;
            
            try 
            {
                string val;
                // Handle different cell types safely
                switch (cell.CellType)
                {
                    case CellType.String:
                        val = cell.StringCellValue;
                        break;
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            val = cell.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            val = cell.NumericCellValue.ToString(CultureInfo.CurrentCulture);
                        }
                        break;
                    case CellType.Boolean:
                        val = cell.BooleanCellValue.ToString();
                        break;
                    case CellType.Formula:
                        // Try to get the calculated value
                        try 
                        {
                             switch(cell.CachedFormulaResultType) 
                             {
                                 case CellType.String: val = cell.StringCellValue; break;
                                 case CellType.Numeric: val = cell.NumericCellValue.ToString(CultureInfo.CurrentCulture); break;
                                 case CellType.Boolean: val = cell.BooleanCellValue.ToString(); break;
                                 default: val = cell.CellFormula; break;
                             }
                        }
                        catch 
                        {
                            val = cell.CellFormula;
                        }
                        break;
                    case CellType.Blank:
                        val = string.Empty;
                        break;
                    default:
                        // Try to force string conversion as a fallback, but non-destructively if possible
                        val = cell.ToString();
                        break;
                }
                return val?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
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

        private void ParseSheet(ISheet sheet, Dictionary<DateTime, WorkLog> dayMap, List<string> errors, Action<string> Log, DateTime monthStart)
        {
             var headerRow = FindHeaderRow(sheet);
             Log($"Header Row Index: {headerRow?.RowNum ?? -1}");
              
             var indexes = GetHeaderIndexes(headerRow);
             Log($"Indexes Found: {string.Join(", ", indexes.Select(kv => $"{kv.Key}={kv.Value}"))}");
              
             // Safety check: if indexes found are too few, fallback to default 0..N
             int foundKnownHeaders = 0;
             foreach(var k in Header) { if (indexes.ContainsKey(k)) foundKnownHeaders++; }
             if (foundKnownHeaders < 2)
             {
                 Log("Too few headers found. Falling back to default mapping.");
                 // Fallback: assume standard order
                 for(int i=0; i<Header.Length; i++) indexes[Header[i]] = i;
                 // Also assume data starts from row 1 if headerRow was 0
                 if (headerRow == null || headerRow.RowNum == 0) headerRow = sheet.GetRow(0);
             }

             int startRow = headerRow != null ? headerRow.RowNum + 1 : 1;
             Log($"Start Row: {startRow}");

             DateTime currentDate = DateTime.MinValue;

             for (int r = startRow; r <= sheet.LastRowNum; r++)
             {
                 try
                 {
                     var row = sheet.GetRow(r);
                     if (row == null) continue;
                     ProcessRow(row, r, indexes, ref currentDate, dayMap, errors, Log, monthStart);
                 }
                 catch (Exception ex)
                 {
                     var msg = $"Row {r} parse error: {ex.Message}";
                     errors.Add(msg);
                     Log(msg);
                 }
             }
        }

        private void ProcessRow(IRow row, int rowIndex, Dictionary<string, int> indexes, ref DateTime currentDate, Dictionary<DateTime, WorkLog> dayMap, List<string> errors, Action<string> Log, DateTime monthStart)
        {
            var firstCellText = GetString(row, 0);
            var title = GetValue(row, indexes, ColumnItemTitle);
            var contentVal = GetValue(row, indexes, ColumnItemContent);

            // Check for date row
            if (TryProcessDateRow(firstCellText, title, contentVal, rowIndex, ref currentDate, dayMap, Log))
                return;

            // Check for summary row
            if (TryProcessSummaryRow(title, contentVal, currentDate, rowIndex, dayMap, Log, monthStart, row, indexes))
                return;

            // Process as regular item row
            ProcessItemRow(row, rowIndex, indexes, currentDate, dayMap, Log, monthStart);
        }

        private bool TryProcessDateRow(string firstCellText, string title, string contentVal, int rowIndex, ref DateTime currentDate, Dictionary<DateTime, WorkLog> dayMap, Action<string> Log)
        {
            if (!string.IsNullOrWhiteSpace(firstCellText))
            {
                bool isExplicitMarker = firstCellText.StartsWith("——");
                bool looksLikeDate = firstCellText.Contains("年") && firstCellText.Contains("月") && firstCellText.Contains("日");
                
                bool isDateRow = isExplicitMarker || (looksLikeDate && string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(contentVal));
                
                if (isDateRow)
                {
                    var m = Regex.Match(firstCellText, @"(\d{4})年(\d{1,2})月(\d{1,2})日");
                    if (m.Success)
                    {
                        if (DateTime.TryParseExact(m.Value, "yyyy年M月d日", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        {
                            currentDate = dt.Date;
                            Log($"[Row {rowIndex}] Found Date Row: {currentDate:yyyy-MM-dd}");
                            if (!dayMap.ContainsKey(currentDate))
                            {
                                dayMap[currentDate] = new WorkLog { LogDate = currentDate, Items = new List<WorkLogItem>() };
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryProcessSummaryRow(string title, string contentVal, DateTime currentDate, int rowIndex, Dictionary<DateTime, WorkLog> dayMap, Action<string> Log, DateTime monthStart, IRow row, Dictionary<string, int> indexes)
        {
            if (string.Equals(title, SummaryTitle, StringComparison.OrdinalIgnoreCase))
            {
                var dt = currentDate != DateTime.MinValue
                    ? currentDate
                    : ParseNullableDateTime(GetValue(row, indexes, ColumnLogDate))?.Date ?? monthStart;
                if (dt == DateTime.MinValue) { Log($"[Row {rowIndex}] Skip Summary: No Date"); return true; }

                if (!dayMap.ContainsKey(dt))
                {
                    dayMap[dt] = new WorkLog { LogDate = dt, Items = new List<WorkLogItem>() };
                }
                dayMap[dt].DailySummary = contentVal;
                Log($"[Row {rowIndex}] Read Summary for {dt:yyyy-MM-dd}");
                return true;
            }
            return false;
        }

        private void ProcessItemRow(IRow row, int rowIndex, Dictionary<string, int> indexes, DateTime currentDate, Dictionary<DateTime, WorkLog> dayMap, Action<string> Log, DateTime monthStart)
        {
            var title = GetValue(row, indexes, ColumnItemTitle);
            var contentVal = GetValue(row, indexes, ColumnItemContent);

            DateTime itemDate = DateTime.MinValue;
            var dateCellStr = GetValue(row, indexes, ColumnLogDate);
            if (!string.IsNullOrWhiteSpace(dateCellStr))
            {
                if (DateTime.TryParse(dateCellStr, out var d)) itemDate = d.Date;
                else if (DateTime.TryParseExact(dateCellStr, "yyyyMMdd", null, DateTimeStyles.None, out d)) itemDate = d.Date;
            }

            var dtItem = itemDate != DateTime.MinValue ? itemDate : (currentDate != DateTime.MinValue ? currentDate : monthStart);
            
            if (dtItem == DateTime.MinValue)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return;
                }
                Log($"[Row {rowIndex}] Skip Item: No Date. Title={title}, DateCell={dateCellStr}");
                return;
            }

            if (!dayMap.ContainsKey(dtItem))
            {
                dayMap[dtItem] = new WorkLog { LogDate = dtItem, Items = new List<WorkLogItem>() };
            }
            var item = new WorkLogItem();
            item.LogDate = dtItem;
            item.ItemTitle = title;
            item.ItemContent = contentVal;
            
            var catName = GetValue(row, indexes, ColumnCategoryName);
            if (string.IsNullOrWhiteSpace(catName)) catName = GetValue(row, indexes, ColumnCategoryId);
            item.CategoryName = catName;
            
            Log($"[Row {rowIndex}] Reading Item: Title='{item.ItemTitle}', Content='{item.ItemContent}', Status='{GetValue(row, indexes, ColumnStatus)}'");

            if (string.IsNullOrWhiteSpace(item.ItemTitle) && string.IsNullOrWhiteSpace(item.ItemContent))
            {
                return;
            }

            var statusStr = GetValue(row, indexes, ColumnStatus);
            item.Status = !string.IsNullOrEmpty(statusStr) ? StatusHelper.Parse(statusStr) : StatusEnum.Todo;

            item.StartTime = ParseNullableDateTime(GetValue(row, indexes, ColumnStartTime));
            item.EndTime = ParseNullableDateTime(GetValue(row, indexes, ColumnEndTime));
            item.Tags = GetValue(row, indexes, ColumnTags);
            item.SortOrder = ParseNullableInt(GetValue(row, indexes, ColumnSortOrder));
            
            var idStr = GetValue(row, indexes, ColumnId);
            if (!string.IsNullOrWhiteSpace(idStr))
            {
                item.Id = idStr;
            }
            
            dayMap[dtItem].Items.Add(item);
        }

        private static IRow FindHeaderRow(ISheet sheet)
        {
            for (int i = 0; i <= Math.Min(20, sheet.LastRowNum); i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                int matchCount = 0;
                for (int c = 0; c < row.LastCellNum; c++)
                {
                    var val = GetString(row, c);
                    // Loose matching: remove spaces, ignore case
                    if (!string.IsNullOrWhiteSpace(val)) 
                    {
                        // Check if val "looks like" a header
                        foreach(var h in Header) 
                        {
                            if (string.Equals(val, h, StringComparison.OrdinalIgnoreCase)) { matchCount++; break; }
                        }
                        foreach(var kvp in HeaderNameMap)
                        {
                            if (val.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0) { matchCount++; break; }
                        }
                    }
                }
                // If we find at least 2 known columns, assume this is the header
                if (matchCount >= 2) return row;
            }
            return sheet.GetRow(0);
        }

        private static Dictionary<string, int> GetHeaderIndexes(IRow headerRow)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
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
                    bool found = false;
                    foreach(var kvp in HeaderNameMap)
                    {
                        if (name.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            dict[kvp.Value] = c;
                            found = true;
                            break;
                        }
                    }
                    if (!found) 
                    {
                        // Try English headers
                         if (Header.Contains(name, StringComparer.OrdinalIgnoreCase))
                         {
                             dict[name] = c;
                         }
                         else
                         {
                            dict[name] = c;
                         }
                    }
                }
            }
            return dict;
        }
    }
}
