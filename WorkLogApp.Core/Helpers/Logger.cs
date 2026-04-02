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
        private static StreamWriter _writer;

        static Logger()
        {
            // 静态构造函数中不自动初始化，等待显式调用 Initialize()
            // 避免 AppDomain.CurrentDomain.BaseDirectory 在静态构造时不可用
        }

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize(string logDirectory = null)
        {
            lock (_lock)
            {
                // 关闭旧的 Writer（支持重复调用 Initialize）
                if (_writer != null)
                {
                    try { _writer.Flush(); _writer.Dispose(); }
                    catch { /* 忽略关闭异常 */ }
                    _writer = null;
                }

                if (logDirectory == null)
                {
                    logDirectory = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        AppConstants.LogsDirectoryName);
                }
                Directory.CreateDirectory(logDirectory);
                _logPath = Path.Combine(logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");

                _writer = new StreamWriter(new FileStream(_logPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };
            }
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
            lock (_lock)
            {
                if (_writer != null)
                {
                    try
                    {
                        _writer.Flush();
                        _writer.Dispose();
                    }
                    catch { /* 忽略关闭异常 */ }
                    _writer = null;
                }
            }
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
                    if (_writer != null)
                    {
                        _writer.WriteLine(logEntry);
                    }
                    else
                    {
                        // Writer 未初始化，回退到 File.AppendAllText
                        if (_logPath != null)
                            File.AppendAllText(_logPath, logEntry + Environment.NewLine);
                        else
                            System.Diagnostics.Debug.WriteLine(logEntry);
                    }
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
