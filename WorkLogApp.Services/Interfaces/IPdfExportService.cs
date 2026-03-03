using System;
using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    /// <summary>
    /// PDF 导出服务接口
    /// </summary>
    public interface IPdfExportService
    {
        /// <summary>
        /// 导出单条工作日志为 PDF
        /// </summary>
        /// <param name="log">工作日志</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="options">导出选项，null 使用默认选项</param>
        /// <returns>是否导出成功</returns>
        bool ExportToPdf(WorkLog log, string outputPath, PdfExportOptions options = null);

        /// <summary>
        /// 导出整月工作日志为 PDF（按周分组）
        /// </summary>
        /// <param name="month">月份</param>
        /// <param name="days">工作日志列表</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="options">导出选项，null 使用默认选项</param>
        /// <returns>是否导出成功</returns>
        bool ExportMonthToPdf(DateTime month, IEnumerable<WorkLog> days, string outputPath, PdfExportOptions options = null);

        /// <summary>
        /// 获取系统已安装的中文字体名称列表
        /// </summary>
        /// <returns>字体名称列表</returns>
        IEnumerable<string> GetAvailableChineseFonts();
    }

    /// <summary>
    /// PDF 导出选项
    /// </summary>
    public class PdfExportOptions
    {
        /// <summary>
        /// 页面标题
        /// </summary>
        public string Title { get; set; } = "工作日志";

        /// <summary>
        /// 字体名称（系统已安装字体或自定义字体路径）
        /// </summary>
        public string FontName { get; set; } = "Microsoft YaHei";

        /// <summary>
        /// 自定义字体文件路径（优先使用）
        /// </summary>
        public string CustomFontPath { get; set; }

        /// <summary>
        /// 是否显示网格线
        /// </summary>
        public bool ShowGridLines { get; set; } = true;

        /// <summary>
        /// 页面方向：true=横向，false=纵向
        /// </summary>
        public bool Landscape { get; set; } = true;

        /// <summary>
        /// 页边距（毫米）
        /// </summary>
        public PageMargins Margins { get; set; } = new PageMargins { Top = 15, Bottom = 15, Left = 15, Right = 15 };

        /// <summary>
        /// 字体大小（磅）
        /// </summary>
        public double FontSize { get; set; } = 10;

        /// <summary>
        /// 标题字体大小（磅）
        /// </summary>
        public double TitleFontSize { get; set; } = 16;
    }

    /// <summary>
    /// 页边距
    /// </summary>
    public class PageMargins
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }
}
