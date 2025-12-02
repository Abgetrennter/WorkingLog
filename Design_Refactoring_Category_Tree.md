# 分类树形结构重构设计方案 (Revision 2)

## 1. 概述
本方案旨在优化现有的扁平化分类结构，将其重构为支持无限层级的树形结构体系。通过分离分类管理与模板管理，实现更灵活的模板关联和更清晰的分类层级。

**变更记录**：
- **Rev 2**:
    - 废弃数据兼容性要求，采用全新数据结构。
    - 模板仅需单关联分类 (One-to-One / Many-to-One)。
    - 引入多标签 (Tags) 机制，模板应用时自动附带标签。
    - 移除 `TemplateCategoryRelation` 表，直接在 `WorkTemplate` 中存储 `CategoryId`。

## 2. 数据结构重构

### 2.1 核心实体设计

#### 2.1.1 分类节点 (Category)
树形结构的骨架。

```csharp
public class Category
{
    public string Id { get; set; }          // 唯一标识 (GUID)
    public string Name { get; set; }        // 分类名称
    public string ParentId { get; set; }    // 父节点ID (根节点为null)
    public int SortOrder { get; set; }      // 同级排序权重
    
    // 运行时属性
    [JsonIgnore]
    public List<Category> Children { get; set; } = new List<Category>();
}
```

#### 2.1.2 模板定义 (WorkTemplate)
模板实体，包含分类引用和标签。

```csharp
public class WorkTemplate
{
    public string Id { get; set; }          // 唯一标识 (GUID)
    public string Name { get; set; }        // 模板名称
    public string CategoryId { get; set; }  // 所属分类ID
    
    public string Content { get; set; }     // 模板内容 (原 FormatTemplate)
    
    // 标签集合
    public List<string> Tags { get; set; } = new List<string>();
    
    // 其它辅助字段
    public Dictionary<string, string> Placeholders { get; set; }
    public Dictionary<string, List<string>> Options { get; set; }
}
```

#### 2.1.3 根存储对象 (TemplateStore)
用于 JSON 序列化的根对象。

```csharp
public class TemplateStore
{
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<WorkTemplate> Templates { get; set; } = new List<WorkTemplate>();
}
```

## 3. 功能模块设计

### 3.1 分类管理模块
- **CRUD操作**：
  - 维护 `TemplateStore.Categories` 列表。
  - 删除分类时，若该分类下有子分类或模板，应提示并阻止，或提供级联删除。
- **树形展示**：
  - 仅依赖 `ParentId` 构建内存树。
- **排序**：
  - `SortOrder` 字段用于同级排序。

### 3.2 模板管理模块
- **关联**：
  - 每个模板必须属于一个 `Category` (叶子节点或非叶子节点均可，但建议仅限叶子节点以保持逻辑清晰，根据用户原需求是“仅允许关联到叶子节点”，此处保持该限制)。
- **标签 (Tags)**：
  - 编辑模板时可输入多个标签（逗号分隔或标签控件）。
  - `WorkLogItem` 需要增加 `Tags` 字段（如尚未存在，需检查）。
  - 应用模板时，将 `WorkTemplate.Tags` 复制到 `WorkLogItem.Tags`。

## 4. 接口/服务层调整 (ITemplateService)

### 4.1 接口定义
```csharp
public interface ITemplateService
{
    // 初始化
    void Load(string path);
    void Save();

    // Category Operations
    List<Category> GetAllCategories();
    Category GetCategory(string id);
    Category CreateCategory(string name, string parentId);
    bool UpdateCategory(Category category);
    bool DeleteCategory(string id);
    bool MoveCategory(string id, string newParentId);

    // Template Operations
    List<WorkTemplate> GetTemplatesByCategory(string categoryId);
    WorkTemplate GetTemplate(string id);
    WorkTemplate CreateTemplate(WorkTemplate template);
    bool UpdateTemplate(WorkTemplate template);
    bool DeleteTemplate(string id);
    
    // Rendering
    string Render(string content, Dictionary<string, object> values);
}
```

## 5. UI 改造

### 5.1 CategoryManageForm
- 左侧：`TreeView`。
  - 根节点：“所有分类”。
  - 支持右键菜单：新增、删除、重命名。
  - 支持拖拽移动节点。
- 右侧：
  - 选中分类后，显示该分类下的模板列表 (`DataGridView` 或 `ListView`)。
  - 模板列表包含列：名称、标签、预览。
  - 选中模板后可进行 编辑/删除。
  - 顶部按钮：“新建模板”（仅当选中有效分类时可用）。

### 5.2 模板编辑对话框
- 输入：名称、内容、标签（TextBox, 逗号分隔）、占位符配置。

### 5.3 主界面 (MainForm / ItemCreateForm)
- 分类选择器：使用 `CategoryTreeComboBox` (需改造适配新 ID 结构)。
- 模板选择：选择分类后，下拉显示该分类下的模板。
- 应用效果：填充 Content，自动填入 Tags。

## 6. 数据迁移
- **不兼容旧数据**。系统启动时若发现旧格式或无法解析，将初始化为空库（或备份旧文件后新建）。

