# 代码修复实施计划 - WorkLogApp

本文档提供针对代码审查报告中发现问题的具体修复方案和实施步骤。

---

## 🔴 P0 - 严重问题修复方案

### 修复 1: 引入日志框架，消除空 catch 块

#### 1.1 添加日志基础设施

**新增文件**: `WorkLogApp.Core/Helpers/Logger.cs`

```csharp
using System;
using System.Diagnostics;
using System.IO;

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

        public static void Initialize(string logDirectory = null)
        {
            if (logDirectory == null)
            {
                logDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Logs");
            }
            Directory.CreateDirectory(logDirectory);
            _logPath = Path.Combine(logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
        }

        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", message, ex);
        }

        public static void Warning(string message)
        {
            Write("WARN", message, null);
        }

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Debug(string message)
        {
            Write("DEBUG", message, null);
        }

        private static void Write(string level, string message, Exception ex)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            if (ex != null)
            {
                logEntry += $"\nException: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
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
                    Debug.WriteLine(logEntry);
                }
            }
        }
    }
}
```

#### 1.2 修复 TemplateService.cs

**位置**: `WorkLogApp.Services/Implementations/TemplateService.cs`

```csharp
// 在文件顶部添加
using WorkLogApp.Core.Helpers;

// 修复 LoadTemplates 方法
public bool LoadTemplates(string templatesJsonPath)
{
    lock (_lock)
    {
        _templatesPath = templatesJsonPath;
        if (!File.Exists(templatesJsonPath))
        {
            Logger.Warning($"模板文件不存在: {templatesJsonPath}，将使用空存储");
            _store = new TemplateStore();
            return true;
        }

        try
        {
            var json = File.ReadAllText(templatesJsonPath);
            _store = JsonConvert.DeserializeObject<TemplateStore>(json);
            
            if (_store == null)
            {
                Logger.Warning($"模板文件反序列化返回 null: {templatesJsonPath}");
                _store = new TemplateStore();
            }
            
            if (_store.Categories == null)
            {
                _store.Categories = new List<Category>();
            }
            if (_store.Templates == null)
            {
                _store.Templates = new List<WorkTemplate>();
            }
            
            return true;
        }
        catch (JsonException ex)
        {
            Logger.Error($"模板文件 JSON 格式错误: {templatesJsonPath}", ex);
            _store = new TemplateStore();
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"加载模板文件失败: {templatesJsonPath}", ex);
            _store = new TemplateStore();
            return false;
        }
    }
}

// 修复 SaveTemplates 方法
public bool SaveTemplates()
{
    lock (_lock)
    {
        if (_store == null || string.IsNullOrWhiteSpace(_templatesPath))
        {
            Logger.Error("保存模板失败: store 为 null 或路径为空");
            return false;
        }
        try
        {
            var json = JsonConvert.SerializeObject(_store, Formatting.Indented);
            var dir = Path.GetDirectoryName(_templatesPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(_templatesPath, json);
            Logger.Info($"模板已保存: {_templatesPath}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"保存模板失败: {_templatesPath}", ex);
            return false;
        }
    }
}
```

#### 1.3 修复 PdfExportService.cs

**位置**: `WorkLogApp.Services/Implementations/PdfExportService.cs`

```csharp
// 添加 using
using WorkLogApp.Core.Helpers;

// 修复 ExportToPdf 方法
public bool ExportToPdf(WorkLog log, string outputPath, PdfExportOptions options = null)
{
    if (log == null)
    {
        Logger.Warning("PDF 导出失败: log 参数为 null");
        return false;
    }
    if (string.IsNullOrWhiteSpace(outputPath))
    {
        Logger.Warning("PDF 导出失败: 输出路径为空");
        return false;
    }

    options = options ?? new PdfExportOptions();

    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

        using (var document = new PdfDocument())
        {
            document.Info.Title = options.Title;
            document.Info.Author = "WorkLogApp";
            document.Info.CreationDate = DateTime.Now;

            AddWorkLogPage(document, log, options);
            document.Save(outputPath);
        }

        Logger.Info($"PDF 导出成功: {outputPath}");
        return true;
    }
    catch (Exception ex)
    {
        Logger.Error($"PDF 导出失败: {outputPath}", ex);
        return false;
    }
}

// 修复 ExportMonthToPdf 方法
public bool ExportMonthToPdf(DateTime month, IEnumerable<WorkLog> days, string outputPath, PdfExportOptions options = null)
{
    if (days == null)
    {
        Logger.Warning("PDF 月度导出失败: days 参数为 null");
        return false;
    }
    if (string.IsNullOrWhiteSpace(outputPath))
    {
        Logger.Warning("PDF 月度导出失败: 输出路径为空");
        return false;
    }

    options = options ?? new PdfExportOptions();

    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

        var validDays = (days ?? Enumerable.Empty<WorkLog>())
            .Where(d => d != null && d.LogDate.Year == month.Year && d.LogDate.Month == month.Month)
            .OrderBy(d => d.LogDate)
            .ToList();

        using (var document = new PdfDocument())
        {
            document.Info.Title = $"{month:yyyy年MM月} - {options.Title}";
            document.Info.Author = "WorkLogApp";
            document.Info.CreationDate = DateTime.Now;

            if (validDays.Any())
            {
                var weeks = validDays.GroupBy(d =>
                {
                    var diff = (7 + (d.LogDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    return d.LogDate.Date.AddDays(-1 * diff);
                }).OrderBy(g => g.Key);

                foreach (var weekGroup in weeks)
                {
                    var weekDays = weekGroup.OrderBy(d => d.LogDate).ToList();
                    AddWeekPage(document, weekDays, options, month);
                }
            }
            else
            {
                AddEmptyPage(document, options, "本月无数据");
            }

            document.Save(outputPath);
        }

        Logger.Info($"PDF 月度导出成功: {outputPath}, 包含 {validDays.Count} 天数据");
        return true;
    }
    catch (Exception ex)
    {
        Logger.Error($"PDF 月度导出失败: {outputPath}", ex);
        return false;
    }
}
```

#### 1.4 修复 WordExportService.cs

**位置**: `WorkLogApp.Services/Implementations/WordExportService.cs`

```csharp
// 添加 using
using WorkLogApp.Core.Helpers;

// 修复 ExportToWord 方法
public bool ExportToWord(WorkLog log, string outputPath, WordExportOptions options = null)
{
    if (log == null)
    {
        Logger.Warning("Word 导出失败: log 参数为 null");
        return false;
    }
    if (string.IsNullOrWhiteSpace(outputPath))
    {
        Logger.Warning("Word 导出失败: 输出路径为空");
        return false;
    }

    options = options ?? new WordExportOptions();

    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

        using (var doc = DocX.Create(outputPath))
        {
            ConfigureDocument(doc, options);
            
            // 添加标题
            var titlePara = doc.InsertParagraph($"{log.LogDate:yyyy年MM月dd日} 工作日志");
            titlePara.FontSize(options.TitleFontSize)
                .Font(options.FontName)
                .Bold()
                .SpacingAfter(15);
            titlePara.Alignment = Alignment.center;

            // 当日总结
            if (!string.IsNullOrWhiteSpace(log.DailySummary))
            {
                doc.InsertParagraph("当日总结")
                    .FontSize(options.FontSize + 2)
                    .Font(options.FontName)
                    .Bold()
                    .SpacingBefore(10)
                    .SpacingAfter(5);

                doc.InsertParagraph(log.DailySummary)
                    .FontSize(options.FontSize)
                    .Font(options.FontName)
                    .SpacingAfter(10);
            }

            // 工作项表格
            if (log.Items?.Any() == true)
            {
                doc.InsertParagraph("工作事项")
                    .FontSize(options.FontSize + 2)
                    .Font(options.FontName)
                    .Bold()
                    .SpacingBefore(10)
                    .SpacingAfter(5);

                InsertWorkLogItemsTable(doc, log.Items, options);
            }

            if (options.IncludeHeader)
            {
                AddHeader(doc, options);
            }

            if (options.IncludeFooter)
            {
                AddFooter(doc);
            }

            doc.Save();
        }

        Logger.Info($"Word 导出成功: {outputPath}");
        return true;
    }
    catch (Exception ex)
    {
        Logger.Error($"Word 导出失败: {outputPath}", ex);
        return false;
    }
}
```

#### 1.5 修复 Program.cs 中的异常处理

**位置**: `WorkLogApp.UI/Program.cs`

```csharp
// 在文件顶部添加
using WorkLogApp.Core.Helpers;

// 修改全局异常捕获
Application.ThreadException += (s, e) =>
{
    var logMsg = $"UI线程异常: {e.Exception?.Message}";
    Logger.Error(logMsg, e.Exception);
    MessageBox.Show(
        $"操作过程中发生错误:\n{e.Exception?.Message}\n\n详细信息已记录到日志文件。",
        "错误",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
};

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    var ex = e.ExceptionObject as Exception;
    var logMsg = $"未处理异常: {ex?.Message}";
    Logger.Error(logMsg, ex);
    MessageBox.Show(
        $"应用程序遇到严重错误:\n{ex?.Message}\n\n详细信息已记录到日志文件。",
        "严重错误",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
};

// 在启动时初始化日志
try
{
    Logger.Initialize();
    Logger.Info("应用程序启动");
    // ... 其他启动代码
}
catch (Exception ex)
{
    Logger.Error("应用程序启动失败", ex);
    MessageBox.Show(
        $"应用程序启动失败:\n{ex.Message}\n\n详细信息已记录到日志文件。",
        "启动错误",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}
```

---

### 修复 2: TemplateService 并发锁优化

**位置**: `WorkLogApp.Services/Implementations/TemplateService.cs`

```csharp
// 替换现有的锁机制
private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

// 修改读操作（使用读锁）
public List<Category> GetAllCategories()
{
    _rwLock.EnterReadLock();
    try
    {
        return _store?.Categories.OrderBy(c => c.SortOrder).ToList() ?? new List<Category>();
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}

public Category GetCategory(string id)
{
    _rwLock.EnterReadLock();
    try
    {
        return _store?.Categories.FirstOrDefault(c => c.Id == id);
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}

public List<WorkTemplate> GetTemplatesByCategory(string categoryId)
{
    _rwLock.EnterReadLock();
    try
    {
        return _store?.Templates
            .Where(t => t.CategoryId == categoryId)
            .OrderBy(t => t.Name)
            .ToList() ?? new List<WorkTemplate>();
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}

// 修改写操作（使用写锁）
public Category CreateCategory(string name, string parentId)
{
    _rwLock.EnterWriteLock();
    try
    {
        if (_store == null) _store = new TemplateStore();

        if (_store.Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"分类名称 '{name}' 已存在。");
        }

        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ParentId = parentId,
            SortOrder = 0
        };
        _store.Categories.Add(category);
        SaveTemplatesInternal();  // 内部方法，不获取锁
        return category;
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}

public bool UpdateCategory(Category category)
{
    _rwLock.EnterWriteLock();
    try
    {
        var existing = _store?.Categories.FirstOrDefault(c => c.Id == category.Id);
        if (existing == null) return false;

        if (!string.Equals(existing.Name, category.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (_store.Categories.Any(c => c.Id != category.Id && 
                string.Equals(c.Name, category.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"分类名称 '{category.Name}' 已存在。");
            }
        }

        existing.Name = category.Name;
        existing.SortOrder = category.SortOrder;
        
        return SaveTemplatesInternal();
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}

public bool DeleteCategory(string id)
{
    _rwLock.EnterWriteLock();
    try
    {
        if (_store == null) return false;
        
        var toDelete = _store.Categories.FirstOrDefault(c => c.Id == id);
        if (toDelete == null) return false;

        _store.Categories.Remove(toDelete);
        _store.Templates.RemoveAll(t => t.CategoryId == id);

        // 递归删除子分类（避免死锁，先收集再删除）
        var childrenIds = new List<string>();
        CollectChildrenIds(id, childrenIds);
        foreach (var childId in childrenIds)
        {
            _store.Categories.RemoveAll(c => c.Id == childId);
            _store.Templates.RemoveAll(t => t.CategoryId == childId);
        }

        return SaveTemplatesInternal();
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}

// 内部方法，假设调用者已持有写锁
private bool SaveTemplatesInternal()
{
    if (_store == null || string.IsNullOrWhiteSpace(_templatesPath)) return false;
    try
    {
        var json = JsonConvert.SerializeObject(_store, Formatting.Indented);
        var dir = Path.GetDirectoryName(_templatesPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(_templatesPath, json);
        return true;
    }
    catch (Exception ex)
    {
        Logger.Error($"保存模板失败: {_templatesPath}", ex);
        return false;
    }
}

// 辅助方法：收集所有子分类ID
private void CollectChildrenIds(string parentId, List<string> childrenIds)
{
    var children = _store?.Categories.Where(c => c.ParentId == parentId).ToList();
    if (children == null) return;
    
    foreach (var child in children)
    {
        childrenIds.Add(child.Id);
        CollectChildrenIds(child.Id, childrenIds);
    }
}

// 修改 LoadTemplates 和 SaveTemplates 为内部方法或删除
public bool LoadTemplates(string templatesJsonPath)
{
    _rwLock.EnterWriteLock();
    try
    {
        // ... 原有逻辑
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}

public bool SaveTemplates()
{
    _rwLock.EnterWriteLock();
    try
    {
        return SaveTemplatesInternal();
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}
```

---

### 修复 3: UIStyleManager 资源释放

**位置**: `WorkLogApp.UI/UI/UIStyleManager.cs`

```csharp
// 添加 Dispose 方法
public static void Dispose()
{
    try
    {
        BodyFont?.Dispose();
        CompactFont?.Dispose();
        Heading1?.Dispose();
        Heading2?.Dispose();
        Heading3?.Dispose();
        
        _pfc?.Dispose();
        
        BodyFont = null;
        CompactFont = null;
        Heading1 = null;
        Heading2 = null;
        Heading3 = null;
        _pfc = null;
        _customFamily = null;
    }
    catch (Exception ex)
    {
        Logger.Error("释放 UI 样式资源失败", ex);
    }
}

// 在 Program.cs 中添加退出处理
// 在 Application.Run(main); 之前添加
Application.ApplicationExit += (s, e) =>
{
    Logger.Info("应用程序退出，清理资源");
    UIStyleManager.Dispose();
    Logger.Dispose();
};
```

---

## 🟠 P1 - 高优先级问题修复方案

### 修复 4: 添加单元测试

#### 4.1 新增 TemplateServiceTests.cs

**位置**: `WorkLogApp.Tests/TemplateServiceTests.cs`

```csharp
using System;
using System.IO;
using System.Linq;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Implementations;
using Xunit;

namespace WorkLogApp.Tests
{
    public class TemplateServiceTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly TemplateService _service;

        public TemplateServiceTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_templates_{Guid.NewGuid()}.json");
            _service = new TemplateService();
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public void LoadTemplates_FileNotExists_ReturnsTrueWithEmptyStore()
        {
            // Arrange
            var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");

            // Act
            var result = _service.LoadTemplates(nonExistentFile);

            // Assert
            Assert.True(result);
            Assert.Empty(_service.GetAllCategories());
            Assert.Empty(_service.GetTemplatesByCategory("any"));
        }

        [Fact]
        public void CreateCategory_ValidName_ReturnsCategory()
        {
            // Arrange
            _service.LoadTemplates(_testFilePath);

            // Act
            var category = _service.CreateCategory("测试分类", null);

            // Assert
            Assert.NotNull(category);
            Assert.Equal("测试分类", category.Name);
            Assert.Null(category.ParentId);
            Assert.Single(_service.GetAllCategories());
        }

        [Fact]
        public void CreateCategory_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            _service.LoadTemplates(_testFilePath);
            _service.CreateCategory("重复名称", null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _service.CreateCategory("重复名称", null));
        }

        [Fact]
        public void UpdateCategory_ExistingCategory_ReturnsTrue()
        {
            // Arrange
            _service.LoadTemplates(_testFilePath);
            var category = _service.CreateCategory("原名", null);
            category.Name = "新名称";

            // Act
            var result = _service.UpdateCategory(category);

            // Assert
            Assert.True(result);
            var updated = _service.GetCategory(category.Id);
            Assert.Equal("新名称", updated.Name);
        }

        [Fact]
        public void DeleteCategory_ExistingCategory_RemovesCategoryAndTemplates()
        {
            // Arrange
            _service.LoadTemplates(_testFilePath);
            var category = _service.CreateCategory("待删除", null);
            var template = new WorkTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = "模板1",
                CategoryId = category.Id
            };
            // 需要添加 AddTemplate 方法或通过其他方式添加模板

            // Act
            var result = _service.DeleteCategory(category.Id);

            // Assert
            Assert.True(result);
            Assert.Null(_service.GetCategory(category.Id));
        }

        [Fact]
        public void MoveCategory_ValidMove_ReturnsTrue()
        {
            // Arrange
            _service.LoadTemplates(_testFilePath);
            var parent = _service.CreateCategory("父分类", null);
            var child = _service.CreateCategory("子分类", null);

            // Act
            var result = _service.MoveCategory(child.Id, parent.Id);

            // Assert
            Assert.True(result);
            var updatedChild = _service.GetCategory(child.Id);
            Assert.Equal(parent.Id, updatedChild.ParentId);
        }

        [Fact]
        public void MoveCategory_CircularReference_ReturnsFalse()
        {
            // Arrange
            _service.LoadTemplates(_testFilePath);
            var parent = _service.CreateCategory("父分类", null);
            var child = _service.CreateCategory("子分类", parent.Id);

            // Act
            var result = _service.MoveCategory(parent.Id, child.Id);

            // Assert
            Assert.False(result);
        }
    }
}
```

#### 4.2 新增 StatusHelperTests.cs

**位置**: `WorkLogApp.Tests/StatusHelperTests.cs`

```csharp
using WorkLogApp.Core.Enums;
using WorkLogApp.Core.Helpers;
using Xunit;

namespace WorkLogApp.Tests
{
    public class StatusHelperTests
    {
        [Theory]
        [InlineData(StatusEnum.Todo, "待办")]
        [InlineData(StatusEnum.Doing, "进行中")]
        [InlineData(StatusEnum.Done, "已完成")]
        [InlineData(StatusEnum.Blocked, "已阻塞")]
        [InlineData(StatusEnum.Cancelled, "已取消")]
        public void ToChinese_ReturnsCorrectChinese(StatusEnum status, string expected)
        {
            // Act
            var result = status.ToChinese();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("待办", StatusEnum.Todo)]
        [InlineData("进行中", StatusEnum.Doing)]
        [InlineData("已完成", StatusEnum.Done)]
        [InlineData("Todo", StatusEnum.Todo)]
        [InlineData("Doing", StatusEnum.Doing)]
        [InlineData("Done", StatusEnum.Done)]
        [InlineData("0", StatusEnum.Todo)]
        [InlineData("1", StatusEnum.Doing)]
        [InlineData("2", StatusEnum.Done)]
        public void Parse_ValidString_ReturnsCorrectStatus(string input, StatusEnum expected)
        {
            // Act
            var result = StatusHelper.Parse(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(StatusEnum.Todo, true)]
        [InlineData(StatusEnum.Doing, true)]
        [InlineData(StatusEnum.Blocked, true)]
        [InlineData(StatusEnum.Done, false)]
        [InlineData(StatusEnum.Cancelled, false)]
        public void IsIncomplete_ReturnsExpected(StatusEnum status, bool expected)
        {
            // Act
            var result = StatusHelper.IsIncomplete(status);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
```

---

### 修复 5: 提取重复代码

#### 5.1 新增 ServiceProviderExtensions.cs

**位置**: `WorkLogApp.Core/Helpers/ServiceProviderExtensions.cs`

```csharp
using System;
using SimpleInjector;
using WorkLogApp.Services.Implementations;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Core.Helpers
{
    /// <summary>
    /// SimpleInjector 容器扩展方法
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 从容器获取服务，如果容器不可用则创建新实例
        /// </summary>
        public static T GetServiceOrCreate<T>(this Container container) where T : class
        {
            var service = container?.GetInstance<T>();
            if (service != null) return service;

            // 容器不可用时的降级处理
            return CreateDefaultService<T>();
        }

        private static T CreateDefaultService<T>() where T : class
        {
            if (typeof(T) == typeof(IImportExportService))
            {
                var pdfService = new PdfExportService();
                var wordService = new WordExportService();
                return new ImportExportService(pdfService, wordService) as T;
            }
            
            throw new InvalidOperationException($"无法创建服务实例: {typeof(T).Name}");
        }
    }
}
```

#### 5.2 修复 ImportWizardForm.cs

**位置**: `WorkLogApp.UI/Forms/ImportWizardForm.cs`

```csharp
// 添加 using
using WorkLogApp.Core.Helpers;

// 替换重复的服务获取代码
private void PreviewFile()
{
    try
    {
        var svc = Program.Container?.GetService<IImportExportService>() ?? 
                  Program.Container?.GetServiceOrCreate<IImportExportService>();
        
        if (svc == null)
        {
            MessageBox.Show(this, "无法获取导入服务", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var days = svc.ImportFromFile(_sourcePath) ?? Enumerable.Empty<WorkLog>();
        _imported = days.ToList();
        
        _previewList.BeginUpdate();
        _previewList.Items.Clear();
        foreach (var it in _imported.SelectMany(d => d.Items ?? new List<WorkLogItem>()).Take(10))
        {
            var lv = new ListViewItem(new[]
            {
                it.LogDate.ToString("yyyy-MM-dd"),
                it.ItemTitle ?? string.Empty,
                it.Tags ?? string.Empty,
                it.Status.ToChinese(),
                $"{(it.StartTime.HasValue ? it.StartTime.Value.ToString("HH:mm") : "")}-{(it.EndTime.HasValue ? it.EndTime.Value.ToString("HH:mm") : "")}"
            });
            _previewList.Items.Add(lv);
        }
        _previewList.EndUpdate();
        _btnImport.Enabled = _imported.Count > 0;
    }
    catch (Exception ex)
    {
        Logger.Error($"预览文件失败: {_sourcePath}", ex);
        MessageBox.Show(this, "预览失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        _btnImport.Enabled = false;
    }
}
```

---

### 修复 6: 提取硬编码常量

#### 6.1 新增 AppConstants.cs

**位置**: `WorkLogApp.Core/Constants/AppConstants.cs`

```csharp
namespace WorkLogApp.Core.Constants
{
    /// <summary>
    /// 应用程序常量定义
    /// </summary>
    public static class AppConstants
    {
        #region 文件和目录
        
        /// <summary>
        /// 导出文件前缀
        /// </summary>
        public const string FilePrefix = "工作日志_";
        
        /// <summary>
        /// 旧版导出文件前缀（向后兼容）
        /// </summary>
        public const string LegacyFilePrefix = "worklog_";
        
        /// <summary>
        /// 默认工作表名称
        /// </summary>
        public const string DefaultSheetName = "工作日志";
        
        /// <summary>
        /// 配置文件目录名
        /// </summary>
        public const string ConfigsDirectory = "Configs";
        
        /// <summary>
        /// 模板文件目录名
        /// </summary>
        public const string TemplatesDirectory = "Templates";
        
        /// <summary>
        /// 数据文件目录名
        /// </summary>
        public const string DataDirectory = "Data";
        
        /// <summary>
        /// 日志文件目录名
        /// </summary>
        public const string LogsDirectory = "Logs";
        
        #endregion

        #region Excel 列名
        
        public const string ColumnLogDate = "LogDate";
        public const string ColumnItemTitle = "ItemTitle";
        public const string ColumnItemContent = "ItemContent";
        public const string ColumnCategoryName = "CategoryName";
        public const string ColumnStatus = "Status";
        public const string ColumnStartTime = "StartTime";
        public const string ColumnEndTime = "EndTime";
        public const string ColumnTags = "Tags";
        public const string ColumnSortOrder = "SortOrder";
        public const string ColumnId = "Id";
        public const string ColumnTrackingId = "TrackingId";
        public const string ColumnSummaryTitle = "当日总结";
        
        #endregion

        #region UI 常量
        
        /// <summary>
        /// 列表视图各列最小宽度
        /// </summary>
        public static class ListViewColumnWidths
        {
            public const int DateMin = 120;
            public const int TitleMin = 150;
            public const int StatusMin = 80;
            public const int ContentMin = 200;
            public const int TagsMin = 100;
            public const int StartTimeMin = 100;
            public const int EndTimeMin = 100;
        }
        
        /// <summary>
        /// 表单控件高度
        /// </summary>
        public static class ControlHeights
        {
            public const int DefaultButton = 36;
            public const int CompactButton = 32;
            public const int TextArea = 80;
            public const int DateTimePicker = 24;
        }
        
        #endregion

        #region 默认值
        
        /// <summary>
        /// 默认环境配置名称
        /// </summary>
        public const string DefaultEnvironment = "dev";
        
        /// <summary>
        /// 默认字体大小
        /// </summary>
        public const float DefaultFontSize = 15f;
        
        #endregion
    }
}
```

#### 6.2 更新 ImportExportService.cs

```csharp
// 添加 using
using WorkLogApp.Core.Constants;

// 替换硬编码字符串
public const string FilePrefix = AppConstants.FilePrefix;
private const string LegacyFilePrefix = AppConstants.LegacyFilePrefix;
private const string SheetName = AppConstants.DefaultSheetName;

// 使用常量替换列名
private const string ColumnItemTitle = AppConstants.ColumnItemTitle;
// ... 其他列名
private const string SummaryTitle = AppConstants.ColumnSummaryTitle;
```

---

## 🟡 P2 - 中优先级问题修复方案

### 修复 7: 重构长方法

#### 7.1 重构 ImportExportService.WriteSheet

**位置**: `WorkLogApp.Services/Implementations/ImportExportService.cs`

```csharp
private static void WriteSheet(IWorkbook wb, string sheetName, IEnumerable<WorkLog> days, DateTime monthContext)
{
    var sheet = wb.CreateSheet(sheetName);
    var styles = CreateSheetStyles(wb);
    var idToName = LoadCategoryNames();

    WriteSheetHeader(sheet, styles.HeaderStyle);
    
    var orderedDays = GetOrderedDays(days, monthContext);
    var rowIndex = 0;
    
    for (int blockIndex = 0; blockIndex < orderedDays.Count; blockIndex++)
    {
        var day = orderedDays[blockIndex];
        var blockStyles = GetBlockStyles(styles, blockIndex);
        
        rowIndex = WriteDayBlock(sheet, day, rowIndex, blockStyles, idToName);
    }

    SetColumnWidths(sheet);
}

private static SheetStyles CreateSheetStyles(IWorkbook wb)
{
    var boldFont = wb.CreateFont();
    boldFont.IsBold = true;

    return new SheetStyles
    {
        HeaderStyle = CreateHeaderStyle(wb, boldFont),
        MarkerStyleA = CreateMarkerStyle(wb, boldFont, IndexedColors.LightCornflowerBlue.Index),
        MarkerStyleB = CreateMarkerStyle(wb, boldFont, IndexedColors.LightYellow.Index),
        BlockStyleA = CreateBlockStyle(wb, IndexedColors.LightCornflowerBlue.Index),
        BlockStyleB = CreateBlockStyle(wb, IndexedColors.LightYellow.Index),
        NumberStyleA = CreateNumberStyle(wb, IndexedColors.LightCornflowerBlue.Index),
        NumberStyleB = CreateNumberStyle(wb, IndexedColors.LightYellow.Index),
        TimeStyleA = CreateTimeStyle(wb, IndexedColors.LightCornflowerBlue.Index),
        TimeStyleB = CreateTimeStyle(wb, IndexedColors.LightYellow.Index),
        TitleStyleA = CreateTitleStyle(wb, IndexedColors.LightCornflowerBlue.Index, boldFont),
        TitleStyleB = CreateTitleStyle(wb, IndexedColors.LightYellow.Index, boldFont)
    };
}

private class SheetStyles
{
    public ICellStyle HeaderStyle { get; set; }
    public ICellStyle MarkerStyleA { get; set; }
    public ICellStyle MarkerStyleB { get; set; }
    public ICellStyle BlockStyleA { get; set; }
    public ICellStyle BlockStyleB { get; set; }
    public ICellStyle NumberStyleA { get; set; }
    public ICellStyle NumberStyleB { get; set; }
    public ICellStyle TimeStyleA { get; set; }
    public ICellStyle TimeStyleB { get; set; }
    public ICellStyle TitleStyleA { get; set; }
    public ICellStyle TitleStyleB { get; set; }
}

private static void WriteSheetHeader(ISheet sheet, ICellStyle headerStyle)
{
    var header = sheet.CreateRow(0);
    for (int i = 0; i < HeaderZh.Length; i++)
    {
        var hc = header.CreateCell(i);
        hc.SetCellValue(HeaderZh[i]);
        hc.CellStyle = headerStyle;
    }
}

private static List<WorkLog> GetOrderedDays(IEnumerable<WorkLog> days, DateTime monthContext)
{
    return (days ?? Enumerable.Empty<WorkLog>())
        .Where(d => d != null && d.LogDate.Year == monthContext.Year && d.LogDate.Month == monthContext.Month)
        .OrderBy(d => d.LogDate.Date)
        .ToList();
}

private static BlockStyles GetBlockStyles(SheetStyles styles, int blockIndex)
{
    var useA = (blockIndex % 2 == 0);
    return new BlockStyles
    {
        MarkerStyle = useA ? styles.MarkerStyleA : styles.MarkerStyleB,
        BlockStyle = useA ? styles.BlockStyleA : styles.BlockStyleB,
        NumberStyle = useA ? styles.NumberStyleA : styles.NumberStyleB,
        TimeStyle = useA ? styles.TimeStyleA : styles.TimeStyleB,
        TitleStyle = useA ? styles.TitleStyleA : styles.TitleStyleB
    };
}

private class BlockStyles
{
    public ICellStyle MarkerStyle { get; set; }
    public ICellStyle BlockStyle { get; set; }
    public ICellStyle NumberStyle { get; set; }
    public ICellStyle TimeStyle { get; set; }
    public ICellStyle TitleStyle { get; set; }
}

private static int WriteDayBlock(ISheet sheet, WorkLog day, int rowIndex, BlockStyles styles, Dictionary<string, string> idToName)
{
    rowIndex = WriteDateMarker(sheet, day, rowIndex, styles.MarkerStyle);
    rowIndex = WriteWorkItems(sheet, day, rowIndex, styles, idToName);
    rowIndex = WriteSummaryRow(sheet, day, rowIndex, styles);
    return rowIndex;
}

private static int WriteDateMarker(ISheet sheet, WorkLog day, int rowIndex, ICellStyle markerStyle)
{
    rowIndex++;
    var markerRow = sheet.CreateRow(rowIndex);
    for (int c = 0; c < HeaderZh.Length; c++)
    {
        var cell = markerRow.CreateCell(c);
        cell.CellStyle = markerStyle;
        var week = GetChineseWeekday(day.LogDate.Date);
        cell.SetCellValue(c == 0 ? $"—— {day.LogDate:yyyy年MM月dd日} {week} ——" : string.Empty);
    }
    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, HeaderZh.Length - 1));
    return rowIndex;
}

private static int WriteWorkItems(ISheet sheet, WorkLog day, int rowIndex, BlockStyles styles, Dictionary<string, string> idToName)
{
    var itemsOrdered = (day.Items ?? new List<WorkLogItem>())
        .OrderBy(i => i.SortOrder ?? 0)
        .ThenBy(i => i.StartTime ?? DateTime.MinValue)
        .ThenBy(i => i.ItemTitle ?? string.Empty)
        .ToList();
        
    foreach (var item in itemsOrdered)
    {
        rowIndex++;
        var row = sheet.CreateRow(rowIndex);
        WriteWorkItemRow(row, item, day.LogDate, styles, idToName);
    }
    return rowIndex;
}

private static void WriteWorkItemRow(IRow row, WorkLogItem item, DateTime logDate, BlockStyles styles, Dictionary<string, string> idToName)
{
    for (int c = 0; c < HeaderZh.Length; c++) row.CreateCell(c);

    row.GetCell(0).SetCellValue(logDate.ToString("yyyy年MM月dd日"));
    row.GetCell(1).SetCellValue(item.ItemTitle ?? string.Empty);
    row.GetCell(2).SetCellValue(item.ItemContent ?? string.Empty);
    
    var catVal = item.CategoryName ?? string.Empty;
    if (idToName.ContainsKey(catVal)) catVal = idToName[catVal];
    row.GetCell(3).SetCellValue(catVal);
    
    row.GetCell(4).SetCellValue(item.Status.ToChinese());
    row.GetCell(5).SetCellValue(item.StartTime.HasValue ? item.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
    row.GetCell(6).SetCellValue(item.EndTime.HasValue ? item.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty);
    row.GetCell(7).SetCellValue(item.Tags ?? string.Empty);
    row.GetCell(8).SetCellValue(item.SortOrder ?? 0);
    row.GetCell(9).SetCellValue(item.Id ?? string.Empty);
    row.GetCell(10).SetCellValue(item.TrackingId ?? string.Empty);

    row.GetCell(0).CellStyle = styles.BlockStyle;
    row.GetCell(1).CellStyle = styles.TitleStyle;
    row.GetCell(2).CellStyle = styles.BlockStyle;
    row.GetCell(3).CellStyle = styles.NumberStyle;
    row.GetCell(4).CellStyle = styles.BlockStyle;
    row.GetCell(5).CellStyle = styles.TimeStyle;
    row.GetCell(6).CellStyle = styles.TimeStyle;
    row.GetCell(7).CellStyle = styles.BlockStyle;
    row.GetCell(8).CellStyle = styles.NumberStyle;
    row.GetCell(9).CellStyle = styles.BlockStyle;
    row.GetCell(10).CellStyle = styles.BlockStyle;
}

private static int WriteSummaryRow(ISheet sheet, WorkLog day, int rowIndex, BlockStyles styles)
{
    rowIndex++;
    var summaryRow = sheet.CreateRow(rowIndex);
    for (int c = 0; c < HeaderZh.Length; c++) summaryRow.CreateCell(c);
    summaryRow.GetCell(1).SetCellValue(SummaryTitle);
    summaryRow.GetCell(2).SetCellValue(day.DailySummary ?? string.Empty);
    for (int c = 0; c < HeaderZh.Length; c++)
    {
        summaryRow.GetCell(c).CellStyle = styles.BlockStyle;
    }
    summaryRow.GetCell(1).CellStyle = styles.TitleStyle;
    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, HeaderZh.Length - 1));
    return rowIndex;
}

private static void SetColumnWidths(ISheet sheet)
{
    sheet.SetColumnWidth(0, 20 * 256);
    sheet.SetColumnWidth(1, 30 * 256);
    sheet.SetColumnWidth(2, 80 * 256);
    sheet.SetColumnWidth(3, 12 * 256);
    sheet.SetColumnWidth(4, 10 * 256);
    sheet.SetColumnWidth(5, 12 * 256);
    sheet.SetColumnWidth(6, 12 * 256);
    sheet.SetColumnWidth(7, 10 * 256);
    sheet.SetColumnWidth(8, 8 * 256);
    sheet.SetColumnWidth(9, 36 * 256);
    sheet.SetColumnWidth(10, 30 * 256);
    sheet.SetColumnHidden(9, true);
    sheet.SetColumnHidden(10, true);
}
```

---

## 📋 实施检查清单

### 第一阶段（P0 问题）- 1-2天

- [ ] 创建 `Logger.cs` 日志类
- [ ] 修复 `TemplateService.cs` 中的空 catch 块
- [ ] 修复 `PdfExportService.cs` 中的空 catch 块
- [ ] 修复 `WordExportService.cs` 中的空 catch 块
- [ ] 修复 `Program.cs` 中的异常处理
- [ ] 优化 `TemplateService` 的并发锁机制
- [ ] 添加 `UIStyleManager.Dispose()` 方法
- [ ] 在 `Program.cs` 中添加资源清理

### 第二阶段（P1 问题）- 3-5天

- [ ] 创建 `TemplateServiceTests.cs`
- [ ] 创建 `StatusHelperTests.cs`
- [ ] 创建 `ServiceProviderExtensions.cs`
- [ ] 重构 `ImportWizardForm` 使用扩展方法
- [ ] 创建 `AppConstants.cs`
- [ ] 更新 `ImportExportService` 使用常量
- [ ] 统一空检查策略

### 第三阶段（P2 问题）- 1周内

- [ ] 重构 `ImportExportService.WriteSheet` 方法
- [ ] 补充公共 API 的 XML 文档注释
- [ ] 添加配置抽象层（可选）
- [ ] 评估依赖包升级

---

**注意**: 在实施每个修复后，建议运行现有测试确保没有引入回归错误。
