using System;
using System.Diagnostics;
using System.IO;
using WorkLogApp.Core.Constants;

namespace WorkLogApp.Core.Helpers
{
    /// <summary>
    /// 简易日志记录器（可后续替换为 NLog/Serilog）
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logPath;

        static Logger()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize(string logDirectory = null)
        {
            if (logDirectory == null)
            {
                logDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    AppConstants.LogsDirectoryName);
            }
            Directory.CreateDirectory(logDirectory);
            _logPath = Path.Combine(logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", message, ex);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void Warning(string message)
        {
            Write("WARN", message, null);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public static void Debug(string message)
        {
            Write("DEBUG", message, null);
        }

        /// <summary>
        /// 释放日志资源
        /// </summary>
        public static void Dispose()
        {
            // 目前无需特殊清理，保留接口以便扩展
        }

        private static void Write(string level, string message, Exception ex)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            if (ex != null)
            {
                logEntry += $"\nException: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    logEntry += $"\nInner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                }
            }

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logPath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // 如果日志写入失败，至少输出到调试器
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
        }
    }
}
