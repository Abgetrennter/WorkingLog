using System;
using System.Collections.Generic;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class ImportExportService : IImportExportService
    {
        public bool ExportMonth(DateTime month, IEnumerable<WorkLogItem> items, string outputDirectory)
        {
            // TODO: 使用 NPOI 实现导出
            return true;
        }

        public IEnumerable<WorkLogItem> ImportMonth(DateTime month, string inputDirectory)
        {
            // TODO: 使用 NPOI 实现导入
            return new List<WorkLogItem>();
        }
    }
}