using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".csv";
            var filePath = Path.Combine(outputDirectory, fileName);

            var existed = File.Exists(filePath);
            using (var sw = new StreamWriter(filePath, append: true))
            {
                if (!existed)
                {
                    sw.WriteLine(string.Join(",", Header));
                }
                foreach (var item in items)
                {
                    if (item == null) continue;
                    if (item.LogDate.Year != month.Year || item.LogDate.Month != month.Month) continue;
                    var row = new string[]
                    {
                        item.LogDate.ToString("yyyy-MM-dd"),
                        EscapeCsv(item.ItemTitle ?? string.Empty),
                        EscapeCsv(item.ItemContent ?? string.Empty),
                        item.Status.ToString(),
                        (item.CategoryId).ToString(),
                        (item.Progress ?? 0).ToString(),
                        item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                        item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                        EscapeCsv(item.Tags ?? string.Empty),
                        (item.SortOrder ?? 0).ToString()
                    };
                    sw.WriteLine(string.Join(",", row));
                }
            }
            return true;
        }

        public IEnumerable<WorkLogItem> ImportMonth(DateTime month, string inputDirectory)
        {
            var list = new List<WorkLogItem>();
            if (string.IsNullOrWhiteSpace(inputDirectory)) return list;
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var fileName = FilePrefix + monthStart.ToString("yyyyMM") + ".csv";
            var filePath = Path.Combine(inputDirectory, fileName);
            if (!File.Exists(filePath)) return list;

            using (var sr = new StreamReader(filePath))
            {
                string line;
                bool isHeader = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (isHeader) { isHeader = false; continue; }
                    var parts = ParseCsvLine(line);
                    if (parts.Length < Header.Length) continue;
                    var item = new WorkLogItem();
                    item.LogDate = DateTime.ParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    item.ItemTitle = parts[1];
                    item.ItemContent = parts[2];
                    item.Status = ParseStatus(parts[3]);
                    item.CategoryId = ParseInt(parts[4]);
                    item.Progress = ParseNullableInt(parts[5]);
                    item.StartTime = ParseNullableDateTime(parts[6]);
                    item.EndTime = ParseNullableDateTime(parts[7]);
                    item.Tags = parts[8];
                    item.SortOrder = ParseNullableInt(parts[9]);
                    list.Add(item);
                }
            }
            return list;
        }

        private static string EscapeCsv(string input)
        {
            if (input == null) return string.Empty;
            var needsQuote = input.Contains(",") || input.Contains("\n") || input.Contains("\r") || input.Contains("\"");
            var escaped = input.Replace("\"", "\"\"");
            return needsQuote ? $"\"{escaped}\"" : escaped;
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // skip escaped quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
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