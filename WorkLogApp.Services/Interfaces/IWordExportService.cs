using System;
using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// Word 导出服务接口
    /// </summary>
    public interface IWordExportService
    {
        /// <summary>
        /// 导出单条工作日志为 Word
        /// </summary>
        /// <param name="log">工作日志</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="options">导出选项，null 使用默认选项</param>
        /// <returns>是否导出成功</returns>
        bool ExportToWord(WorkLog log, string outputPath, WordExportOptions options = null);

        /// <summary>
        /// 导出整月工作日志为 Word（按周分组）
        /// </summary>
        /// <param name="month">月份</param>
        /// <param name="days">工作日志列表</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="options">导出选项，null 使用默认选项</param>
        /// <returns>是否导出成功</returns>
        bool ExportMonthToWord(DateTime month, IEnumerable<WorkLog> days, string outputPath, WordExportOptions options = null);
    }

    /// <summary>
    /// Word 导出选项
    /// </summary>
    public class WordExportOptions
    {
        /// <summary>
        /// 文档标题
        /// </summary>
        public string Title { get; set; } = "工作日志";

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName { get; set; } = "Microsoft YaHei";

        /// <summary>
        /// 正文字号（磅）
        /// </summary>
        public double FontSize { get; set; } = 10.5;

        /// <summary>
        /// 标题字号（磅）
        /// </summary>
        public double TitleFontSize { get; set; } = 16;

        /// <summary>
        /// 页面方向：true=横向，false=纵向
        /// </summary>
        public bool Landscape { get; set; } = true;

        /// <summary>
        /// 页边距（厘米）
        /// </summary>
        public WordPageMargins Margins { get; set; } = new WordPageMargins { Top = 2, Bottom = 2, Left = 2, Right = 2 };

        /// <summary>
        /// 是否包含页眉
        /// </summary>
        public bool IncludeHeader { get; set; } = true;

        /// <summary>
        /// 是否包含页脚（页码）
        /// </summary>
        public bool IncludeFooter { get; set; } = true;
    }

    /// <summary>
    /// Word 页边距
    /// </summary>
    public class WordPageMargins
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }
}
