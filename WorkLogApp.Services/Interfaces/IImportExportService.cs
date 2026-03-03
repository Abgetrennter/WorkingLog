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

        /// <summary>
        /// 导出整月工作日志为 PDF
        /// </summary>
        /// <param name="month">月份</param>
        /// <param name="days">工作日志列表</param>
        /// <param name="outputPath">输出文件路径（完整路径，含 .pdf 扩展名）</param>
        /// <param name="options">PDF 导出选项，null 使用默认选项</param>
        /// <returns>是否导出成功</returns>
        bool ExportMonthToPdf(DateTime month, IEnumerable<WorkLog> days, string outputPath, PdfExportOptions options = null);

        /// <summary>
        /// 导出整月工作日志为 Word
        /// </summary>
        /// <param name="month">月份</param>
        /// <param name="days">工作日志列表</param>
        /// <param name="outputPath">输出文件路径（完整路径，含 .docx 扩展名）</param>
        /// <param name="options">Word 导出选项，null 使用默认选项</param>
        /// <returns>是否导出成功</returns>
        bool ExportMonthToWord(DateTime month, IEnumerable<WorkLog> days, string outputPath, WordExportOptions options = null);
    }
}