using System;
using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    public interface IImportExportService
    {
        bool ExportMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory);
        bool RewriteMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory);
        IEnumerable<WorkLog> ImportMonth(DateTime month, string inputDirectory);
        IEnumerable<WorkLog> ImportFromFile(string filePath);
        ImportResult ImportFromFileWithDiagnostics(string filePath);
        ImportResult ImportFromTxt(string filePath);
        ImportResult CompareAndVerify(IEnumerable<WorkLog> source, IEnumerable<WorkLog> target);
    }
}