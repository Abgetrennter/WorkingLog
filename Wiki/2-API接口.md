# 2. API 接口规范

本项目为桌面应用，"API" 指的是服务层 (`WorkLogApp.Services`) 暴露给 UI 层的 C# 接口。

## 2.1 模板服务 (`ITemplateService`)

负责处理 JSON 模板的加载、保存及渲染逻辑。

### 接口定义
```csharp
public interface ITemplateService
{
    // 加载模板配置文件
    bool LoadTemplates(string templatesJsonPath);
    
    // 保存当前模板到文件
    bool SaveTemplates();
    
    // 获取模板根对象
    TemplateRoot GetRoot();
    
    // 获取指定分类的合并模板（自动处理父子继承）
    CategoryTemplate GetMergedCategoryTemplate(string categoryName);
    
    // 渲染模板字符串
    string Render(string formatTemplate, Dictionary<string, object> fieldValues, WorkLogItem item);
    
    // 分类管理操作
    void AddCategory(string categoryName, CategoryTemplate template);
    void RemoveCategory(string categoryName);
    void UpdateCategory(string oldName, string newName, CategoryTemplate template);
}
```

### 关键方法说明
- **GetMergedCategoryTemplate**: 
  - **输入**: `categoryName` (e.g., "研发-后端-API")
  - **逻辑**: 按照 `研发` -> `研发-后端` -> `研发-后端-API` 的路径，依次合并 `FormatTemplate`（追加）、`Placeholders`（覆盖/新增）和 `Options`（合并去重）。
  - **输出**: 一个包含完整配置的 `CategoryTemplate` 对象。

- **Render**:
  - **输入**: 模板字符串、字段字典、当前事项对象。
  - **逻辑**: 使用正则替换 `{Key:Format}` 占位符。内置支持 `{ItemTitle}` 和 `{CategoryPath}` 变量。

## 2.2 导入导出服务 (`IImportExportService`)

负责所有 Excel 相关的 I/O 操作。

### 接口定义
```csharp
public interface IImportExportService
{
    // 导出/保存整月数据（合并现有数据）
    bool ExportMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory);
    
    // 强制重写整月数据（完全覆盖）
    bool RewriteMonth(DateTime month, IEnumerable<WorkLog> days, string outputDirectory);
    
    // 读取指定月份数据
    IEnumerable<WorkLog> ImportMonth(DateTime month, string dataDirectory);
    
    // 从外部 Excel 文件导入数据
    IEnumerable<WorkLog> ImportFromFile(string filePath);
}
```

### 关键机制
- **ExportMonth vs RewriteMonth**:
  - `ExportMonth`: 读取现有 Excel -> 内存中按日期合并数据（保留原有数据） -> 写入。用于追加日志。
  - `RewriteMonth`: 直接使用传入的数据覆盖 Excel 文件。用于编辑、删除、排序后的保存。

- **ImportFromFile**:
  - **功能**: 智能解析外部 Excel 文件。
  - **特性**: 
    - 自动识别中文表头（日期、标题、内容等）。
    - 自动识别日期块分隔行（`===== yyyy年MM月dd日...`）。
    - 自动提取 "当日总结" 行。
