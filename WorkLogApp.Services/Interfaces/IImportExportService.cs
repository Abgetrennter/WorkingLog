using System;
using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    public interface IImportExportService
    {
        bool ExportMonth(DateTime month, IEnumerable<WorkLogItem> items, string outputDirectory);
        IEnumerable<WorkLogItem> ImportMonth(DateTime month, string inputDirectory);
    }
}