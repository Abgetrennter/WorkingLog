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
            "LogDate","ItemTitle","ItemContent","CategoryId","Status","StartTime","EndTime","Tags","SortOrder"
        };
        private static readonly string[] HeaderZh = new[]
        {
            "日期","标题","内容","分类ID","状态","开始时间","结束时间","标签","排序"
        };
        private static readonly Dictionary<string, string> HeaderNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"日期","LogDate"},
            {"标题","ItemTitle"},
            {"内容","ItemContent"},
            {"分类ID","CategoryId"},
            {"状态","Status"},
            {"开始时间","StartTime"},
            {"结束时间","EndTime"},
            {"标签","Tags"},
            {"排序","SortOrder"}
        };

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

        // 覆盖写入整月数据：生成单个工作表并写入传入的所有当月记录（按日期分块）
        public bool RewriteMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory)
        {
            if (days == null) return false;
            if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
            Directory.CreateDirectory(outputDirectory);

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(outputDirectory, fileName);

            IWorkbook wb = new XSSFWorkbook();
            WriteMonthSheet(wb, month, days);
            using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                wb.Write(outFs);
            }
            return true;
        }

        private static void WriteMonthSheet(IWorkbook wb, DateTime month, IEnumerable<WorkLog> days)
        {
            var sheet = wb.CreateSheet(SheetName);

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
                .Where(d => d != null && d.LogDate.Year == month.Year && d.LogDate.Month == month.Month)
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
                    cell.SetCellValue(c == 0 ? $"===== {day.LogDate:yyyy年MM月dd日} {week} =====" : string.Empty);
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
                    // 分类ID列写模板名称；若无法解析则回退数值ID
                    var catName = idToName.TryGetValue(item.CategoryId ?? "", out var nm)
                        ? nm
                        : (!string.IsNullOrWhiteSpace(item.Tags) && idToName.ContainsValue(item.Tags) ? item.Tags : item.CategoryId);
                    row.GetCell(3).SetCellValue(catName);
                    row.GetCell(4).SetCellValue(ToChineseStatus(item.Status));
                    row.GetCell(5).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(6).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
                    row.GetCell(7).SetCellValue(item.Tags ?? string.Empty);
                    row.GetCell(8).SetCellValue(item.SortOrder ?? 0);
                    row.GetCell(0).CellStyle = blockStyle;
                    row.GetCell(1).CellStyle = titleStyle;
                    row.GetCell(2).CellStyle = blockStyle;
                    row.GetCell(3).CellStyle = numberStyle;
                    row.GetCell(4).CellStyle = blockStyle;
                    row.GetCell(5).CellStyle = timeStyle;
                    row.GetCell(6).CellStyle = timeStyle;
                    row.GetCell(7).CellStyle = blockStyle;
                    row.GetCell(8).CellStyle = numberStyle;
                }
                // 每个日期块的最后一行写入当日总结（标题=当日总结，内容=总结文本）
                rowIndex++;
                var summaryRow = sheet.CreateRow(rowIndex);
                for (int c = 0; c < HeaderZh.Length; c++) summaryRow.CreateCell(c);
                summaryRow.GetCell(1).SetCellValue("当日总结");
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
            var list = new List<WorkLog>();
            if (string.IsNullOrWhiteSpace(inputDirectory)) return list;
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".xlsx";
            var filePath = Path.Combine(inputDirectory, fileName);
            if (!File.Exists(filePath)) return list;

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Use WorkbookFactory to support both xls and xlsx
                    IWorkbook wb;
                    try
                    {
                        wb = WorkbookFactory.Create(fs);
                    }
                    catch (Exception)
                    {
                        // Fallback or retry? If it fails here, maybe not an Excel file.
                        return list;
                    }

                    var sheet = wb.GetSheet(SheetName) ?? (wb.NumberOfSheets > 0 ? wb.GetSheetAt(0) : null);
                    if (sheet == null) return list;

                    var indexes = GetHeaderIndexes(sheet);
                    DateTime currentDate = DateTime.MinValue;
                    var dayMap = new Dictionary<DateTime, WorkLog>();

                    for (int r = 1; r <= sheet.LastRowNum; r++)
                    {
                        try
                        {
                            var row = sheet.GetRow(r);
                            if (row == null) continue;
                            var firstCellText = GetString(row, 0);
                            if (!string.IsNullOrWhiteSpace(firstCellText) && firstCellText.StartsWith("====="))
                            {
                                var m = Regex.Match(firstCellText, "=+\\s*(\\d{4}年\\d{2}月\\d{2}日)(?:\\s+星期[一二三四五六日天])?\\s*=+");
                                if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyy年MM月dd日", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                                {
                                    currentDate = dt.Date;
                                    if (!dayMap.ContainsKey(currentDate))
                                    {
                                        dayMap[currentDate] = new WorkLog { LogDate = currentDate, Items = new List<WorkLogItem>() };
                                    }
                                }
                                continue;
                            }

                            // 识别总结行（标题=当日总结）
                            var title = GetValue(row, indexes, "ItemTitle");
                            if (string.Equals(title, "当日总结", StringComparison.OrdinalIgnoreCase))
                            {
                                var dt = currentDate != DateTime.MinValue
                                    ? currentDate
                                    : ParseNullableDateTime(GetValue(row, indexes, "LogDate"))?.Date ?? monthStart;
                                if (!dayMap.ContainsKey(dt))
                                {
                                    dayMap[dt] = new WorkLog { LogDate = dt, Items = new List<WorkLogItem>() };
                                }
                                dayMap[dt].DailySummary = GetValue(row, indexes, "ItemContent");
                                continue;
                            }

                            var dtItem = currentDate != DateTime.MinValue
                                ? currentDate
                                : ParseNullableDateTime(GetValue(row, indexes, "LogDate"))?.Date ?? monthStart;
                            if (!dayMap.ContainsKey(dtItem))
                            {
                                dayMap[dtItem] = new WorkLog { LogDate = dtItem, Items = new List<WorkLogItem>() };
                            }
                            var item = new WorkLogItem();
                            item.LogDate = dtItem;
                            item.ItemTitle = GetValue(row, indexes, "ItemTitle");
                            item.ItemContent = GetValue(row, indexes, "ItemContent");
                            // 分类ID：支持文字名称（模板名）或数字ID，若为名称则生成稳定数值ID
                            {
                                item.CategoryId = GetValue(row, indexes, "CategoryId");
                            }
                            
                            var statusStr = GetValue(row, indexes, "Status");
                            item.Status = !string.IsNullOrEmpty(statusStr) ? ParseStatus(statusStr) : StatusEnum.Todo;

                            item.StartTime = ParseNullableDateTime(GetValue(row, indexes, "StartTime"));
                            item.EndTime = ParseNullableDateTime(GetValue(row, indexes, "EndTime"));
                            item.Tags = GetValue(row, indexes, "Tags");
                            item.SortOrder = ParseNullableInt(GetValue(row, indexes, "SortOrder"));
                            dayMap[dtItem].Items.Add(item);
                        }
                        catch (Exception)
                        {
                            // Skip bad row
                            continue;
                        }
                    }
                    list = dayMap.Values.OrderBy(d => d.LogDate.Date).ToList();
                }
            }
            catch (Exception)
            {
                // File access error
            }
            return list;
        }

        public IEnumerable<WorkLog> ImportFromFile(string filePath)
        {
            var list = new List<WorkLog>();
            if (string.IsNullOrWhiteSpace(filePath)) return list;
            if (!File.Exists(filePath)) return list;

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    IWorkbook wb;
                    try
                    {
                        wb = WorkbookFactory.Create(fs);
                    }
                    catch
                    {
                        return list;
                    }

                    var sheet = wb.GetSheet(SheetName) ?? (wb.NumberOfSheets > 0 ? wb.GetSheetAt(0) : null);
                    if (sheet == null) return list;

                    var indexes = GetHeaderIndexes(sheet);
                    DateTime currentDate = DateTime.MinValue;
                    var dayMap = new Dictionary<DateTime, WorkLog>();

                    for (int r = 1; r <= sheet.LastRowNum; r++)
                    {
                        try
                        {
                            var row = sheet.GetRow(r);
                            if (row == null) continue;
                            var firstCellText = GetString(row, 0);
                            if (!string.IsNullOrWhiteSpace(firstCellText) && firstCellText.StartsWith("====="))
                            {
                                var m = Regex.Match(firstCellText, "=+\\s*(\\d{4}年\\d{2}月\\d{2}日)(?:\\s+星期[一二三四五六日天])?\\s*=+");
                                if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyy年MM月dd日", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                                {
                                    currentDate = dt.Date;
                                    if (!dayMap.ContainsKey(currentDate))
                                    {
                                        dayMap[currentDate] = new WorkLog { LogDate = currentDate, Items = new List<WorkLogItem>() };
                                    }
                                }
                                continue;
                            }

                            // 识别总结行（标题=当日总结）
                            var title = GetValue(row, indexes, "ItemTitle");
                            if (string.Equals(title, "当日总结", StringComparison.OrdinalIgnoreCase))
                            {
                                var dt = currentDate != DateTime.MinValue
                                    ? currentDate
                                    : ParseNullableDateTime(GetValue(row, indexes, "LogDate"))?.Date ?? DateTime.MinValue;
                                if (!dayMap.ContainsKey(dt))
                                {
                                    dayMap[dt] = new WorkLog { LogDate = dt, Items = new List<WorkLogItem>() };
                                }
                                dayMap[dt].DailySummary = GetValue(row, indexes, "ItemContent");
                                continue;
                            }

                            var dtItem = currentDate != DateTime.MinValue
                                ? currentDate
                                : ParseNullableDateTime(GetValue(row, indexes, "LogDate"))?.Date ?? DateTime.MinValue;
                            if (!dayMap.ContainsKey(dtItem))
                            {
                                dayMap[dtItem] = new WorkLog { LogDate = dtItem, Items = new List<WorkLogItem>() };
                            }
                            var item = new WorkLogItem();
                            item.LogDate = dtItem;
                            item.ItemTitle = GetValue(row, indexes, "ItemTitle");
                            item.ItemContent = GetValue(row, indexes, "ItemContent");
                            // 分类ID：支持文字名称（模板名）或数字ID，若为名称则生成稳定数值ID
                            {
                                item.CategoryId = GetValue(row, indexes, "CategoryId");
                            }
                            
                            var statusStr = GetValue(row, indexes, "Status");
                            item.Status = !string.IsNullOrEmpty(statusStr) ? ParseStatus(statusStr) : StatusEnum.Todo;

                            item.StartTime = ParseNullableDateTime(GetValue(row, indexes, "StartTime"));
                            item.EndTime = ParseNullableDateTime(GetValue(row, indexes, "EndTime"));
                            item.Tags = GetValue(row, indexes, "Tags");
                            item.SortOrder = ParseNullableInt(GetValue(row, indexes, "SortOrder"));
                            dayMap[dtItem].Items.Add(item);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    list = dayMap.Values.OrderBy(d => d.LogDate.Date).ToList();
                }
            }
            catch
            {
                // File access error
            }
            return list;
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
                // Handle different cell types safely
                switch (cell.CellType)
                {
                    case CellType.String:
                        return cell.StringCellValue ?? string.Empty;
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            return cell.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        return cell.NumericCellValue.ToString(CultureInfo.CurrentCulture);
                    case CellType.Boolean:
                        return cell.BooleanCellValue.ToString();
                    case CellType.Formula:
                        // Try to get the calculated value
                        try 
                        {
                             switch(cell.CachedFormulaResultType) 
                             {
                                 case CellType.String: return cell.StringCellValue;
                                 case CellType.Numeric: return cell.NumericCellValue.ToString(CultureInfo.CurrentCulture);
                                 case CellType.Boolean: return cell.BooleanCellValue.ToString();
                                 default: return cell.CellFormula;
                             }
                        }
                        catch 
                        {
                            return cell.CellFormula;
                        }
                    case CellType.Blank:
                        return string.Empty;
                    default:
                        // Try to force string conversion as a fallback, but non-destructively if possible
                        // Old method was: cell.SetCellType(CellType.String); return cell.StringCellValue;
                        // We'll try ToString() first which is safer
                        return cell.ToString();
                }
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

        private static string ToChineseStatus(StatusEnum status)
        {
            switch (status)
            {
                case StatusEnum.Todo: return "待办";
                case StatusEnum.Doing: return "进行中";
                case StatusEnum.Done: return "已完成";
                case StatusEnum.Blocked: return "阻塞";
                case StatusEnum.Cancelled: return "已取消";
                default: return "待办";
            }
        }

        private static StatusEnum ParseStatus(string s)
        {
            var text = (s ?? string.Empty).Trim();
            if (int.TryParse(text, out var iv))
            {
                if (Enum.IsDefined(typeof(StatusEnum), iv))
                    return (StatusEnum)iv;
            }
            if (Enum.TryParse<StatusEnum>(text, out var status)) return status;
            switch (text)
            {
                case "待办": return StatusEnum.Todo;
                case "进行中": return StatusEnum.Doing;
                case "已完成": return StatusEnum.Done;
                case "阻塞":
                case "受阻": return StatusEnum.Blocked;
                case "已取消":
                case "取消": return StatusEnum.Cancelled;
                default: return StatusEnum.Todo;
            }
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
            return dict;
        }
    }
}
