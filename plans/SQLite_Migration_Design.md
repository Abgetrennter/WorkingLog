# SQLite 数据库与数据访问层设计方案

## 1. 目标
将现有基于 JSON 的文件存储方式改造为 SQLite 关系型数据库存储，同时保持现有的 Excel 导出/导入能力。

## 2. 数据库表结构设计

我们使用 Entity Framework Core (SQLite) 作为 ORM。根据现有的领域模型 `WorkLog`, `WorkLogItem`, `Category` 设计如下表结构：

### 表：Categories (分类表)
| 字段名 | 类型 | 约束 | 描述 | 对应模型属性 |
|---|---|---|---|---|
| Id | TEXT | PRIMARY KEY | 唯一标识(GUID字符串) | `Id` |
| Name | TEXT | NOT NULL | 分类名称 | `Name` |
| ParentId | TEXT | NULLABLE | 父节点ID | `ParentId` |
| SortOrder | INTEGER | NOT NULL DEFAULT 0 | 排序权重 | `SortOrder` |

*索引建议：在 `ParentId` 上创建索引以优化树形结构查询。在 `Name` 上创建唯一索引（如果在同级或全局要求名称唯一）。*

### 表：WorkLogs (每日日志主表)
*注：目前系统中 `WorkLog` 是按天存储的，使用 `LogDate` 作为唯一标识更为合理。*
| 字段名 | 类型 | 约束 | 描述 | 对应模型属性 |
|---|---|---|---|---|
| LogDate | TEXT | PRIMARY KEY | 日期(格式: yyyy-MM-dd) | `LogDate` |
| DailySummary | TEXT | NULLABLE | 每日总结/感想 | `DailySummary` |

### 表：WorkLogItems (工作日志明细表)
| 字段名 | 类型 | 约束 | 描述 | 对应模型属性 |
|---|---|---|---|---|
| Id | TEXT | PRIMARY KEY | 唯一标识(GUID字符串) | `Id` |
| LogDate | TEXT | NOT NULL | 归属日期(外键->WorkLogs) | `LogDate` |
| ItemTitle | TEXT | NOT NULL | 标题 | `ItemTitle` |
| ItemContent | TEXT | NULLABLE | 详细内容 | `ItemContent` |
| CategoryName| TEXT | NULLABLE | 分类名称(当前为冗余/解耦设计)| `CategoryName` |
| Status | INTEGER| NOT NULL | 状态枚举(0:Todo,1:Doing等)| `Status` |
| StartTime | TEXT | NULLABLE | 开始时间(ISO 8601字符串)| `StartTime` |
| EndTime | TEXT | NULLABLE | 结束时间(ISO 8601字符串)| `EndTime` |
| Tags | TEXT | NULLABLE | 标签(逗号分隔) | `Tags` |
| SortOrder | INTEGER| NULLABLE | 排序 | `SortOrder` |
| TrackingId | TEXT | NULLABLE | 外部系统追踪ID | `TrackingId` |

*外键关系：`WorkLogItems.LogDate` -> `WorkLogs.LogDate` (ON DELETE CASCADE)*
*索引建议：在 `LogDate`、`Status` 和 `CategoryName` 上创建索引，以加速日常列表查询和统计。*

## 3. 架构改造方案

### 3.1 引入 NuGet 包
在 `WorkLogApp.Core` 或 `WorkLogApp.Services`（建议在 Services 层或者新建 Data 访问层）中引入：
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`

### 3.2 DbContext 设计
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<WorkLog> WorkLogs { get; set; }
    public DbSet<WorkLogItem> WorkLogItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "WorkLog.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkLog>()
            .HasKey(w => w.LogDate); // 使用 LogDate 作为主键

        modelBuilder.Entity<WorkLog>()
            .HasMany(w => w.Items)
            .WithOne()
            .HasForeignKey(i => i.LogDate)
            .HasPrincipalKey(w => w.LogDate)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## 4. 兼容 Excel 导入/导出

### 导出 (Export)
- **旧逻辑**：读取 `Data/yyyy-MM.json` 文件。
- **新逻辑**：通过 Entity Framework Core 查询 `WorkLogItems` 表中 `LogDate` 在指定月份范围内的所有数据，`Include` 或者关联相关的 `WorkLog`，然后在内存中组装成与以前相同的 DTO，再交给 NPOI 生成 Excel（按周分 Sheet，按原有列名导出）。完全不需要改动基于 NPOI 的生成逻辑。

### 导入 (Import)
- **旧逻辑**：解析 Excel 到模型，然后追加/覆写 JSON。
- **新逻辑**：解析 Excel 生成 `List<WorkLogItem>`。
  - 对于已经存在的 `Id`（如果有 ID 列），执行 Update 操作。
  - 对于没有 `Id` 的新记录，生成 GUID 并执行 Insert 操作。
  - 导入完成后，调用 `SaveChanges()` 将事务一次性提交，保证数据的完整性。这比写文件安全得多。

## 5. 数据迁移计划 (JSON to SQLite)

在应用启动时（`Program.cs`），加入自动迁移逻辑：
1. 检查是否存在 `Data/WorkLog.db` 且是否已包含数据。
2. 如果为空或不存在，执行初始化脚本：遍历 `Data/` 目录下的所有 `.json` 文件（例如 `categories.json`, `2024-03.json`）。
3. 使用现有的 JSON 反序列化方法将数据读入内存。
4. 将数据批量 `AddRange` 插入到 SQLite 并保存。
5. 记录一个迁移成功标记（如在配置或新建的系统配置表中写入标记），后续启动直接使用 SQLite。
