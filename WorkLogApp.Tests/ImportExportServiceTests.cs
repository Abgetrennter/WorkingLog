using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Implementations;
using Xunit;

namespace WorkLogApp.Tests
{
    public class ImportExportServiceTests
    {
        [Fact]
        public void ExportMonth_ShouldGroupDataByWeeks()
        {
            // Arrange
            var pdfService = new PdfExportService();
            var wordService = new WordExportService();
            var service = new ImportExportService(pdfService, wordService);
            var month = new DateTime(2024, 1, 1);
            var outputDir = Path.Combine(Path.GetTempPath(), "WorkLogTests_" + Guid.NewGuid());
            Directory.CreateDirectory(outputDir);

            try
            {
                var days = new List<WorkLog>
                {
                    // Week 1: Jan 1 - Jan 7
                    new WorkLog { LogDate = new DateTime(2024, 1, 1), Items = new List<WorkLogItem> { new WorkLogItem { ItemTitle = "Task 1" } } },
                    new WorkLog { LogDate = new DateTime(2024, 1, 5), Items = new List<WorkLogItem> { new WorkLogItem { ItemTitle = "Task 2" } } },
                    
                    // Week 2: Jan 8 - Jan 14
                    new WorkLog { LogDate = new DateTime(2024, 1, 8), Items = new List<WorkLogItem> { new WorkLogItem { ItemTitle = "Task 3" } } },
                    
                    // Week 5: Jan 29 - Jan 31
                    new WorkLog { LogDate = new DateTime(2024, 1, 31), Items = new List<WorkLogItem> { new WorkLogItem { ItemTitle = "Task 4" } } }
                };

                // Act
                // We use RewriteMonth directly because ExportMonth merges with existing files which don't exist here.
                var success = service.RewriteMonth(month, days, outputDir);

                // Assert
                Assert.True(success);
                var filePath = Path.Combine(outputDir, "工作日志_202401.xlsx");
                Assert.True(File.Exists(filePath));

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var wb = new XSSFWorkbook(fs);
                    
                    // Should have 3 sheets
                    // Jan 1-7, Jan 8-14, Jan 29-31
                    Assert.Equal(3, wb.NumberOfSheets);

                    // Week 1: Jan 1-7. Data on 1, 5. Name: 1日-5日
                    var sheet1 = wb.GetSheet("1日-5日");
                    Assert.NotNull(sheet1);
                    Assert.True(CheckSheetContains(sheet1, "Task 1"));
                    Assert.True(CheckSheetContains(sheet1, "Task 2"));
                    Assert.False(CheckSheetContains(sheet1, "Task 3"));

                    // Week 2: Jan 8-14. Data on 8. Name: 8日-8日
                    var sheet2 = wb.GetSheet("8日-8日");
                    Assert.NotNull(sheet2);
                    Assert.True(CheckSheetContains(sheet2, "Task 3"));

                    // Week 5: Jan 29-31. Data on 31. Name: 31日-31日
                    var sheet3 = wb.GetSheet("31日-31日");
                    Assert.NotNull(sheet3);
                    Assert.True(CheckSheetContains(sheet3, "Task 4"));
                }
            }
            finally
            {
                if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            }
        }

        private bool CheckSheetContains(ISheet sheet, string value)
        {
            for (int r = 0; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;
                for (int c = 0; c < row.LastCellNum; c++)
                {
                    var cell = row.GetCell(c);
                    if (cell != null && cell.ToString() == value) return true;
                }
            }
            return false;
        }

        [Fact]
        public void ExportMonth_EmptyData_ShouldCreateDefaultSheet()
        {
            // Arrange
            var pdfService = new PdfExportService();
            var wordService = new WordExportService();
            var service = new ImportExportService(pdfService, wordService);
            var month = new DateTime(2024, 2, 1);
            var outputDir = Path.Combine(Path.GetTempPath(), "WorkLogTests_" + Guid.NewGuid());
            Directory.CreateDirectory(outputDir);

            try
            {
                // Act
                var success = service.RewriteMonth(month, new List<WorkLog>(), outputDir);

                // Assert
                Assert.True(success);
                var filePath = Path.Combine(outputDir, "工作日志_202402.xlsx");
                Assert.True(File.Exists(filePath));

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var wb = new XSSFWorkbook(fs);
                    Assert.Equal(1, wb.NumberOfSheets);
                    Assert.NotNull(wb.GetSheet("工作日志"));
                }
            }
            finally
            {
                if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            }
        }
    }
}
