# SQLite 数据库与数据访问层设计方案

## 1. 目标

将现有基于 **Excel (.xlsx)** 的文件存储方式改造为 SQLite 关系型数据库存储，同时保持现有的 Excel 导出/导入能力（用于数据备份、分享和向后兼容）。

> **重要说明**：原设计方案错误假设数据存储在 JSON 中。实际上工作日志数据存储在 Excel 文件（`工作日志_yyyyMM.xlsx`）中，仅分类/模板数据使用 JSON 存储。

---

## 2. 数据库表结构设计

我们使用 Entity Framework Core (SQLite) 作为 ORM。根据现有的领域模型 `WorkLog`, `WorkLogItem` 设计如下表结构：

> **关于分类/模板数据**：经评估，`Category` 和 `WorkTemplate` 数据保持 JSON 文件存储，不迁移到 SQLite。原因：
> 1. 模板包含复杂嵌套字典结构（`Placeholders`、`Options`），不适合关系型存储
> 2. 数据量极小（分类<50，模板<200），全量加载到内存是最优方案
> 3. 访问模式适合内存缓存（读多写少，启动加载一次）
> 4. 迁移成本大于收益，现有实现稳定
>
> 详见：`plans/Category_Template_SQLite_Evaluation.md`

### 表：WorkLogs (每日日志主表)

| 字段名 | 类型 | 约束 | 描述 | 对应模型属性 |
|---|---|---|---|---|
| Id | INTEGER | PRIMARY KEY AUTOINCREMENT | 自增主键 | 新增 |
| LogDate | TEXT | UNIQUE NOT NULL | 日期(格式: yyyy-MM-dd) | `LogDate` |
| DailySummary | TEXT | NULLABLE | 每日总结/感想 | `DailySummary` |

> **设计变更**：使用自增 INTEGER 作为主键，LogDate 改为唯一索引。这比字符串主键性能更好，且避免时区/格式问题。

### 表：WorkLogItems (工作日志明细表)

| 字段名 | 类型 | 约束 | 描述 | 对应模型属性 |
|---|---|---|---|---|
| Id | TEXT | PRIMARY KEY | 唯一标识(GUID字符串) | `Id` |
| WorkLogId | INTEGER | NOT NULL | 外键->WorkLogs.Id | 新增 |
| LogDate | TEXT | NOT NULL | 归属日期(冗余，便于查询) | `LogDate` |
| ItemTitle | TEXT | NOT NULL | 标题 | `ItemTitle` |
| ItemContent | TEXT | NULLABLE | 详细内容 | `ItemContent` |
| CategoryName | TEXT | NULLABLE | 分类名称(保持与Excel兼容) | `CategoryName` |
| Status | INTEGER | NOT NULL | 状态枚举(0:Todo,1:Doing等) | `Status` |
| StartTime | TEXT | NULLABLE | 开始时间(ISO 8601字符串) | `StartTime` |
| EndTime | TEXT | NULLABLE | 结束时间(ISO 8601字符串) | `EndTime` |
| Tags | TEXT | NULLABLE | 标签(逗号分隔) | `Tags` |
| SortOrder | INTEGER | NULLABLE | 排序 | `SortOrder` |
| TrackingId | TEXT | NULLABLE | 外部系统追踪ID | `TrackingId` |

*外键关系：`WorkLogItems.WorkLogId` -> `WorkLogs.Id` (ON DELETE CASCADE)*
*索引建议：在 `WorkLogId`、`LogDate`、`Status` 和 `CategoryName` 上创建索引。*

### 表：MigrationLog (迁移日志表)

| 字段名 | 类型 | 约束 | 描述 |
|---|---|---|---|
| Id | INTEGER | PRIMARY KEY AUTOINCREMENT | 自增主键 |
| FileName | TEXT | NOT NULL | Excel文件名 |
| FilePath | TEXT | NOT NULL | 完整路径 |
| FileSize | INTEGER | | 文件大小(字节) |
| Status | TEXT | NOT NULL | 状态: Pending/Processing/Completed/Failed |
| RecordsImported | INTEGER | | 导入记录数 |
| ErrorMessage | TEXT | | 错误信息 |
| StartedAt | DATETIME | | 开始时间 |
| CompletedAt | DATETIME | | 完成时间 |

*用于追踪每个Excel文件的迁移状态，支持断点续传和故障排查。*

---

## 2.2 表结构汇总

需要创建的数据库表：
| 表名 | 说明 | 来源 |
|------|------|------|
| `WorkLogs` | 每日日志主表 | Excel 迁移 |
| `WorkLogItems` | 工作日志明细表 | Excel 迁移 |
| `MigrationLog` | 迁移日志表 | 新增 |

不纳入 SQLite 的表：
| 表名 | 说明 | 存储方式 |
|------|------|---------|
| `Categories` | 分类表 | JSON 文件（保持现状） |
| `WorkTemplates` | 模板表 | JSON 文件（保持现状） |

---

## 3. 架构改造方案

### 3.1 引入 NuGet 包

在 `WorkLogApp.Services` 中引入：
- `Microsoft.EntityFrameworkCore.Sqlite` (版本 3.1.32 - 最后一个支持 .NET Framework 4.7.2 的版本)
- `Microsoft.EntityFrameworkCore.Design` (版本 3.1.32)

> **注意**：EF Core 5.0+ 不支持 .NET Framework，必须使用 3.1.x LTS 版本。

### 3.2 DbContext 设计

```csharp
public class AppDbContext : DbContext
{
    // 仅工作日志相关表（分类/模板保持 JSON 存储）
    public DbSet<WorkLog> WorkLogs { get; set; }
    public DbSet<WorkLogItem> WorkLogItems { get; set; }
    public DbSet<MigrationLog> MigrationLogs { get; set; }

    private readonly string _connectionString;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
        
        #if DEBUG
        optionsBuilder.LogTo(Console.WriteLine);
        #endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // WorkLogs 配置
        modelBuilder.Entity<WorkLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LogDate).IsUnique();
            
            entity.HasMany(w => w.Items)
                .WithOne()
                .HasForeignKey(i => i.WorkLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkLogItems 配置
        modelBuilder.Entity<WorkLogItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkLogId);
            entity.HasIndex(e => e.LogDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CategoryName);
            entity.HasIndex(e => e.TrackingId);
        });

        // MigrationLog 配置
        modelBuilder.Entity<MigrationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.Status);
        });
    }
}
```

> **注意**：`Category` 和 `WorkTemplate` 不纳入 DbContext，继续使用 `TemplateService` 管理 JSON 文件存储。

### 3.3 数据访问层重构

`ImportExportService` 需要重构为支持双模式（SQLite 主存储 + Excel 导入导出）：

```csharp
public class ImportExportService : IImportExportService
{
    private readonly AppDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IWordExportService _wordExportService;

    // 从 SQLite 读取（新的主逻辑）
    public IEnumerable<WorkLog> ImportMonth(DateTime month)
    {
        return _dbContext.WorkLogs
            .Include(w => w.Items)
            .Where(w => w.LogDate.Year == month.Year && w.LogDate.Month == month.Month)
            .OrderBy(w => w.LogDate)
            .ToList();
    }

    // 保存到 SQLite（新的主逻辑）
    public bool ExportMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory)
    {
        // 1. 保存到 SQLite
        foreach (var day in days)
        {
            var existing = _dbContext.WorkLogs
                .Include(w => w.Items)
                .FirstOrDefault(w => w.LogDate == day.LogDate);

            if (existing != null)
            {
                // 更新现有记录
                existing.DailySummary = day.DailySummary;
                // 合并或替换 Items...
            }
            else
            {
                _dbContext.WorkLogs.Add(day);
            }
        }
        _dbContext.SaveChanges();

        // 2. 可选：同时导出到 Excel（用于备份）
        ExportMonthToExcel(month, outputDirectory);
        
        return true;
    }

    // 保留：从 Excel 导入（用于数据迁移和外部导入）
    public IEnumerable<WorkLog> ImportMonthFromExcel(DateTime month, string inputDirectory)
    {
        // 复用现有的 ImportMonth 逻辑
        // ...
    }

    // 保留：导出到 Excel（用于备份和分享）
    public bool ExportMonthToExcel(DateTime month, string outputDirectory)
    {
        // 从 SQLite 读取，生成 Excel
        var days = ImportMonth(month);
        // 使用现有的 NPOI 逻辑生成 Excel
        // ...
    }
}
```

---

## 4. 兼容 Excel 导入/导出

### 4.1 导出 (Export)

**新逻辑**：
1. 从 SQLite 查询指定月份的所有 `WorkLog` 及其 `WorkLogItems`
2. 在内存中组装成 DTO
3. 使用现有的 NPOI 逻辑生成 Excel（按周分 Sheet，保留中文表头和样式）
4. 文件保存到 `Data/工作日志_yyyyMM.xlsx`

**关键点**：
- 保留按周分 Sheet 的格式（`d日-d日`）
- 保留中文表头（日期、标题、内容、分类、状态等）
- 保留交替行背景色样式
- 保留每日总结行格式

### 4.2 导入 (Import)

**双模式支持**：

**模式 A：从 SQLite 读取（默认）**
```csharp
var logs = _importExportService.ImportMonth(month); // 从 SQLite
```

**模式 B：从 Excel 导入（兼容旧数据/外部导入）**
```csharp
var logs = _importExportService.ImportMonthFromExcel(month, dataDir);
// 解析后可以选择导入到 SQLite
foreach (var log in logs) {
    _dbContext.WorkLogs.Add(log);
}
_dbContext.SaveChanges();
```

**Excel 解析复用**：
- 复用 `ImportFromFileWithDiagnostics` 方法
- 保留临时文件复制策略（解决文件锁定）
- 保留多格式日期解析
- 保留新旧文件名兼容（`worklog_` -> `工作日志_`）

---

## 5. 数据迁移计划 (Excel to SQLite)

> **重要**：原方案错误地从 JSON 迁移。实际必须从 Excel 文件迁移。

### 5.1 迁移前准备

1. **完整备份**：将 `Data/` 目录完整复制到 `Data/Backup/PreSQLite_{timestamp}/`
2. **创建 MigrationLog 表**：用于追踪每个 Excel 文件的处理状态
3. **检查磁盘空间**：确保有足够空间存储 SQLite 数据库

### 5.2 自动迁移流程

在应用启动时（`Program.cs`），加入自动迁移逻辑：

```csharp
public class ExcelToSQLiteMigrationService
{
    private readonly AppDbContext _dbContext;
    private readonly IImportExportService _importService;

    public MigrationResult MigrateAllExcelFiles(string dataDirectory)
    {
        var result = new MigrationResult();
        
        // 1. 扫描所有 Excel 文件
        var excelFiles = Directory.GetFiles(dataDirectory, "*.xlsx")
            .Where(f => {
                var name = Path.GetFileName(f);
                return name.StartsWith("工作日志_") || name.StartsWith("worklog_");
            })
            .OrderBy(f => f); // 按年月顺序处理

        // 2. 检查是否已迁移
        if (_dbContext.MigrationLogs.Any() && 
            !_dbContext.MigrationLogs.Any(m => m.Status == "Failed"))
        {
            // 已完全迁移，跳过
            return result;
        }

        // 3. 逐文件迁移
        foreach (var file in excelFiles)
        {
            var fileName = Path.GetFileName(file);
            
            // 检查是否已处理
            if (_dbContext.MigrationLogs.Any(m => m.FileName == fileName && m.Status == "Completed"))
            {
                continue; // 已处理，跳过
            }

            // 记录开始
            var log = new MigrationLog
            {
                FileName = fileName,
                FilePath = file,
                FileSize = new FileInfo(file).Length,
                Status = "Processing",
                StartedAt = DateTime.Now
            };
            _dbContext.MigrationLogs.Add(log);
            _dbContext.SaveChanges();

            try
            {
                // 4. 使用现有逻辑解析 Excel
                var importResult = _importService.ImportFromFileWithDiagnostics(file);
                
                if (importResult.Data != null)
                {
                    // 5. 保存到 SQLite（使用事务）
                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        foreach (var workLog in importResult.Data)
                        {
                            // 检查是否已存在
                            var existing = _dbContext.WorkLogs
                                .FirstOrDefault(w => w.LogDate == workLog.LogDate);
                            
                            if (existing == null)
                            {
                                _dbContext.WorkLogs.Add(workLog);
                            }
                            else
                            {
                                // 合并数据（根据业务需求决定）
                                MergeWorkLogs(existing, workLog);
                            }
                        }
                        _dbContext.SaveChanges();
                        transaction.Commit();
                    }
                }

                // 6. 更新状态
                log.Status = "Completed";
                log.RecordsImported = importResult.Data?.Sum(d => d.Items.Count) ?? 0;
                log.CompletedAt = DateTime.Now;
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTime.Now;
                _dbContext.SaveChanges();
                
                result.Errors.Add($"文件 {fileName} 迁移失败: {ex.Message}");
                // 继续处理下一个文件
            }
        }

        return result;
    }
}
```

### 5.3 迁移执行顺序

1. **分类数据**：`categories.json` -> `Categories` 表（保持现有 JSON 或迁移到 SQLite）
2. **工作日志数据**：按年月顺序处理 `工作日志_yyyyMM.xlsx`
3. **验证**：迁移完成后校验记录总数

### 5.4 保留 Excel 文件

**重要**：迁移后不删除原始 Excel 文件，作为：
- 回滚备份
- 外部查看/编辑
- 灾难恢复

---

## 6. 回滚与故障恢复

### 6.1 回滚策略

```csharp
public class MigrationRollbackService
{
    public bool RollbackToExcelMode()
    {
        // 1. 关闭 SQLite 连接
        // 2. 可选：重命名 WorkLog.db -> WorkLog.db.bak
        // 3. 清除 MigrationLog 表（或标记为已回滚）
        // 4. 应用切换回 Excel 模式
        return true;
    }
}
```

### 6.2 故障恢复级别

| 级别 | 场景 | 恢复操作 |
|---|---|---|
| Level 1 | 单文件导入失败 | 记录错误，继续处理其他文件 |
| Level 2 | 数据库损坏 | 删除 .db 文件，从 MigrationLog 重新迁移 |
| Level 3 | 需要完全回滚 | 恢复 Data/Backup/ 目录，切换回 Excel 模式 |

### 6.3 用户界面

在应用中提供：
- 迁移进度显示（处理哪个文件、成功/失败计数）
- "重新迁移" 按钮（用于修复失败的迁移）
- "回滚到 Excel 模式" 菜单（高级选项，带确认提示）

---

## 7. 实施检查清单

### 7.1 开发阶段

- [ ] 添加 EF Core 3.1.32 NuGet 包
- [ ] 创建 `AppDbContext` 和实体配置
- [ ] 重构 `ImportExportService` 支持双模式
- [ ] 实现 `ExcelToSQLiteMigrationService`
- [ ] 实现 `MigrationRollbackService`
- [ ] 更新 `MainForm` 调用新的数据访问层
- [ ] 添加迁移进度 UI

### 7.2 测试阶段

- [ ] 测试 Excel 解析兼容性（新旧文件名格式）
- [ ] 测试大批量数据迁移性能
- [ ] 测试迁移中断后的断点续传
- [ ] 测试回滚功能
- [ ] 测试数据一致性（记录数校验）

### 7.3 部署阶段

- [ ] 完整备份用户 Data 目录
- [ ] 小规模试点（1-2个月数据）
- [ ] 全量数据迁移
- [ ] 监控 MigrationLog 确保无失败

---

## 8. 风险评估

| 风险 | 概率 | 影响 | 缓解措施 |
|---|---|---|---|
| Excel 文件被锁定无法读取 | 中 | 高 | 使用临时文件复制策略 |
| 日期格式解析失败 | 低 | 中 | 复用现有解析逻辑，支持多格式 |
| 数据量大导致迁移慢 | 中 | 中 | 分批处理，显示进度 |
| 用户误删 Excel 文件 | 低 | 高 | 迁移前强制完整备份 |
| EF Core 3.1 维护状态 | 中 | 低 | 规划未来升级到 .NET 6/8 |

---

*修订记录：*
- *v1.1 (2026-03-03): 修正迁移源为 Excel 而非 JSON，补充回滚机制和双模式策略*
- *v1.2 (2026-03-04): 移除 Categories 和 WorkTemplates 表设计，保持 JSON 存储（详见 Category_Template_SQLite_Evaluation.md）*
