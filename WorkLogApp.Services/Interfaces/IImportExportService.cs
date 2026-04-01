using System;
using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// 导入导出服务接口
    /// 提供工作日志的 Excel 导入/导出、PDF/Word 导出功能
    /// </summary>
    public interface IImportExportService
    {
        #region Excel 导出

        /// <summary>
        /// 导出指定月份的工作日志到 Excel 文件（合并模式）
        /// 将新数据与现有文件合并，保留已有数据
        /// </summary>
        /// <param name="month">要导出的月份</param>
        /// <param name="days">工作日志列表</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <returns>是否导出成功</returns>
        bool ExportMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory);

        /// <summary>
        /// 覆盖写入整月数据到 Excel 文件
        /// 生成按周分组的多个工作表，完全覆盖现有文件
        /// </summary>
        /// <param name="month">要导出的月份</param>
        /// <param name="days">工作日志列表</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <returns>是否导出成功</returns>
        bool RewriteMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory);

        #endregion

        #region Excel 导入

        /// <summary>
        /// 从 Excel 文件导入指定月份的工作日志
        /// </summary>
        /// <param name="month">要导入的月份</param>
        /// <param name="inputDirectory">输入目录</param>
        /// <returns>工作日志列表</returns>
        IEnumerable<WorkLog> ImportMonth(DateTime month, string inputDirectory);

        /// <summary>
        /// 从 Excel 文件导入工作日志
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <returns>工作日志列表</returns>
        IEnumerable<WorkLog> ImportFromFile(string filePath);

        /// <summary>
        /// 从 Excel 文件导入工作日志（带诊断信息）
        /// 返回包含导入数据和错误信息的完整结果
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <returns>包含导入数据和错误信息的结果对象</returns>
        ImportResult ImportFromFileWithDiagnostics(string filePath);

        /// <summary>
        /// 从 JSON 文件导入工作日志
        /// </summary>
        /// <param name="filePath">JSON 文件路径</param>
        /// <returns>包含导入数据和错误信息的结果对象</returns>
        ImportResult ImportFromTxt(string filePath);

        #endregion

        #region 数据验证

        /// <summary>
        /// 比较两组工作日志数据，验证一致性
        /// </summary>
        /// <param name="source">源数据</param>
        /// <param name="target">目标数据</param>
        /// <returns>比较结果，包含差异信息</returns>
        ImportResult CompareAndVerify(IEnumerable<WorkLog> source, IEnumerable<WorkLog> target);

        #endregion

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