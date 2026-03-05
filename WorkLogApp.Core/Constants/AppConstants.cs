using System;

namespace WorkLogApp.Core.Constants
{
    /// <summary>
    /// 应用程序常量集中管理
    /// 用于消除硬编码字符串和魔法数字
    /// </summary>
    public static class AppConstants
    {
        #region 目录名称

        /// <summary>
        /// 数据目录名称
        /// </summary>
        public const string DataDirectoryName = "Data";

        /// <summary>
        /// 日志目录名称
        /// </summary>
        public const string LogsDirectoryName = "Logs";

        /// <summary>
        /// 模板目录名称
        /// </summary>
        public const string TemplatesDirectoryName = "Templates";

        /// <summary>
        /// 配置目录名称
        /// </summary>
        public const string ConfigsDirectoryName = "Configs";

        #endregion

        #region 文件名称

        /// <summary>
        /// 模板配置文件名
        /// </summary>
        public const string TemplatesFileName = "templates.json";

        /// <summary>
        /// 开发环境配置文件名
        /// </summary>
        public const string DevConfigFileName = "dev.config.json";

        /// <summary>
        /// 生产环境配置文件名
        /// </summary>
        public const string ProdConfigFileName = "prod.config.json";

        /// <summary>
        /// 线程异常日志文件名
        /// </summary>
        public const string ThreadExceptionLogFileName = "thread_exception.log";

        /// <summary>
        /// 未处理异常日志文件名
        /// </summary>
        public const string UnhandledExceptionLogFileName = "unhandled_exception.log";

        /// <summary>
        /// 启动错误日志文件名
        /// </summary>
        public const string StartupErrorLogFileName = "startup_error.log";

        #endregion

        #region 配置键名

        /// <summary>
        /// 数据路径配置键名
        /// </summary>
        public const string DataPathConfigKey = "DataPath";

        /// <summary>
        /// 模板路径配置键名
        /// </summary>
        public const string TemplatesPathConfigKey = "TemplatesPath";

        /// <summary>
        /// 配置环境键名
        /// </summary>
        public const string ConfigEnvironmentKey = "ConfigEnvironment";

        /// <summary>
        /// 默认环境名称
        /// </summary>
        public const string DefaultEnvironment = "dev";

        #endregion

        #region 资源命名空间

        /// <summary>
        /// 配置资源命名空间前缀
        /// </summary>
        public const string ConfigResourceNamespace = "Configs.";

        /// <summary>
        /// 模板资源命名空间前缀
        /// </summary>
        public const string TemplateResourceNamespace = "Templates.";

        #endregion

        #region 文件大小限制

        /// <summary>
        /// 最大文件大小限制（字节）- 1KB
        /// </summary>
        public const int MaxFileSize1KB = 1024;

        /// <summary>
        /// 最大文件大小限制（字节）- 4KB
        /// </summary>
        public const int MaxFileSize4KB = 4096;

        /// <summary>
        /// 最大文件大小限制（字节）- 8KB
        /// </summary>
        public const int MaxFileSize8KB = 8192;

        #endregion

        #region 日期时间格式

        /// <summary>
        /// 日期格式（yyyy-MM-dd）
        /// </summary>
        public const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// 时间格式（HH:mm）
        /// </summary>
        public const string TimeFormat = "HH:mm";

        /// <summary>
        /// 日期时间格式（yyyy-MM-dd HH:mm）
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        /// <summary>
        /// 月份格式（yyyyMM）
        /// </summary>
        public const string MonthFormat = "yyyyMM";

        #endregion

        #region 文件名格式

        /// <summary>
        /// 工作日志Excel文件名格式
        /// </summary>
        public const string WorkLogExcelFileNameFormat = "工作日志_{0:yyyyMM}.xlsx";

        /// <summary>
        /// 工作日志PDF文件名格式
        /// </summary>
        public const string WorkLogPdfFileNameFormat = "工作日志_{0:yyyyMMdd}_{1:yyyyMMdd}.pdf";

        /// <summary>
        /// 工作日志Word文件名格式
        /// </summary>
        public const string WorkLogWordFileNameFormat = "工作日志_{0:yyyyMMdd}_{1:yyyyMMdd}.docx";

        #endregion

        #region UI 相关

        /// <summary>
        /// 默认字体大小
        /// </summary>
        public const float DefaultFontSize = 9f;

        /// <summary>
        /// 标题字体大小
        /// </summary>
        public const float HeadingFontSize = 12f;

        /// <summary>
        /// 紧凑模式字体大小
        /// </summary>
        public const float CompactFontSize = 8f;

        #endregion

        #region Excel 列宽配置

        /// <summary>
        /// Excel 列宽基础单位（NPOI 使用 1/256 字符宽度）
        /// </summary>
        public const int ExcelColumnWidthUnit = 256;

        /// <summary>
        /// 日期列宽度（字符数）
        /// </summary>
        public const int ExcelColumnDateWidth = 20;

        /// <summary>
        /// 标题列宽度（字符数）
        /// </summary>
        public const int ExcelColumnTitleWidth = 30;

        /// <summary>
        /// 内容列宽度（字符数）
        /// </summary>
        public const int ExcelColumnContentWidth = 80;

        /// <summary>
        /// 分类列宽度（字符数）
        /// </summary>
        public const int ExcelColumnCategoryWidth = 12;

        /// <summary>
        /// 状态列宽度（字符数）
        /// </summary>
        public const int ExcelColumnStatusWidth = 10;

        /// <summary>
        /// 时间列宽度（字符数）
        /// </summary>
        public const int ExcelColumnTimeWidth = 12;

        /// <summary>
        /// 标签列宽度（字符数）
        /// </summary>
        public const int ExcelColumnTagsWidth = 10;

        /// <summary>
        /// 排序列宽度（字符数）
        /// </summary>
        public const int ExcelColumnSortOrderWidth = 8;

        /// <summary>
        /// ID 列宽度（字符数）
        /// </summary>
        public const int ExcelColumnIdWidth = 36;

        #endregion

        #region ListView 列宽配置

        /// <summary>
        /// ListView 日期列最小宽度
        /// </summary>
        public const int ListViewColumnDateMinWidth = 120;

        /// <summary>
        /// ListView 标题列最小宽度
        /// </summary>
        public const int ListViewColumnTitleMinWidth = 150;

        /// <summary>
        /// ListView 状态列最小宽度
        /// </summary>
        public const int ListViewColumnStatusMinWidth = 80;

        /// <summary>
        /// ListView 状态列绝对最小宽度
        /// </summary>
        public const int ListViewColumnStatusAbsoluteMinWidth = 60;

        /// <summary>
        /// ListView 内容列最小宽度
        /// </summary>
        public const int ListViewColumnContentMinWidth = 200;

        /// <summary>
        /// ListView 标签列最小宽度
        /// </summary>
        public const int ListViewColumnTagsMinWidth = 100;

        /// <summary>
        /// ListView 时间列最小宽度
        /// </summary>
        public const int ListViewColumnTimeMinWidth = 100;

        /// <summary>
        /// ListView 日期选择器宽度
        /// </summary>
        public const int ListViewDatePickerWidth = 140;

        /// <summary>
        /// ListView 月份选择器宽度
        /// </summary>
        public const int ListViewMonthPickerWidth = 100;

        #endregion
    }
}
