# UI现代化重构实施路线图

## 概述

本路线图将 WinUI/Fluent Design 规范转化为可执行的开发任务，按优先级分阶段实施。

---

## 阶段一：基础样式层 (预计 1-2 周)

### 任务 1.1: 建立 Fluent Design 常量库
**文件**: [`WorkLogApp.UI/UI/FluentColors.cs`](WorkLogApp.UI/UI/FluentColors.cs:1)

```csharp
namespace WorkLogApp.UI.UI
{
    public static class FluentColors
    {
        // Primary
        public static readonly Color Primary = Color.FromArgb(0, 120, 212);
        public static readonly Color PrimaryLight = Color.FromArgb(79, 161, 228);
        public static readonly Color PrimaryDark = Color.FromArgb(0, 90, 158);
        public static readonly Color PrimaryLighter = Color.FromArgb(229, 241, 250);
        
        // Neutral Grays
        public static readonly Color Gray190 = Color.FromArgb(32, 31, 30);
        public static readonly Color Gray160 = Color.FromArgb(50, 49, 48);
        public static readonly Color Gray130 = Color.FromArgb(96, 94, 92);
        public static readonly Color Gray100 = Color.FromArgb(138, 136, 134);
        public static readonly Color Gray80 = Color.FromArgb(200, 198, 196);
        public static readonly Color Gray60 = Color.FromArgb(200, 198, 196);
        public static readonly Color Gray40 = Color.FromArgb(225, 223, 221);
        public static readonly Color Gray20 = Color.FromArgb(243, 242, 241);
        public static readonly Color Gray10 = Color.FromArgb(250, 249, 248);
        public static readonly Color White = Color.White;
        
        // Semantic
        public static readonly Color Success = Color.FromArgb(16, 124, 16);
        public static readonly Color Warning = Color.FromArgb(255, 185, 0);
        public static readonly Color Error = Color.FromArgb(209, 52, 56);
    }
}
```

### 任务 1.2: 扩展字体管理
**文件**: [`WorkLogApp.UI/UI/FluentTypography.cs`](WorkLogApp.UI/UI/FluentTypography.cs:1)

```csharp
public static class FluentTypography
{
    public static Font Header => new Font("Segoe UI Variable", 21f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font TitleLarge => new Font("Segoe UI Variable", 15f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Title => new Font("Segoe UI Variable", 12f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Subtitle => new Font("Segoe UI Variable", 10.5f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font BodyLarge => new Font("Segoe UI Variable", 11.25f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font Body => new Font("Segoe UI Variable", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font Caption => new Font("Segoe UI Variable", 9f, FontStyle.Regular, GraphicsUnit.Point);
}
```

### 任务 1.3: 更新 UIStyleManager
**文件**: [`WorkLogApp.UI/UI/UIStyleManager.cs`](WorkLogApp.UI/UI/UIStyleManager.cs:1)

**修改内容**:
1. 替换现有颜色引用为 FluentColors
2. 统一按钮高度为 36px
3. 添加圆角按钮支持 (4px radius)
4. 更新列表样式

### 任务 1.4: 全局应用主题
**文件**: [`WorkLogApp.UI/Forms/MainForm.cs`](WorkLogApp.UI/Forms/MainForm.cs:1)

**修改构造函数**:
```csharp
public MainForm(ITemplateService templateService, IImportExportService importExportService)
{
    _templateService = templateService;
    _importExportService = importExportService;
    
    InitializeComponent();
    InitToolTips();
    IconHelper.ApplyIcon(this);

    // 应用 Fluent 主题
    UIStyleManager.ApplyFluentTheme(this);
    
    // 运行时事件绑定...
}
```

---

## 阶段二：核心界面重构 (预计 2-3 周)

### 任务 2.1: 重构 MainForm 顶部工具栏
**文件**: [`WorkLogApp.UI/Forms/MainForm.Designer.cs`](WorkLogApp.UI/Forms/MainForm.Designer.cs:1)

**布局调整**:
```
当前: FlowLayoutPanel (自动排列，间距不统一)
目标: TableLayoutPanel (精确控制，8pt网格)

行高: 56px (导航栏) + 48px (工具栏)
列宽: 左侧按钮组 | 中间筛选区 | 右侧操作组
间距: 所有元素遵循 8px/16px 间距
```

**按钮重组**:
- 主要操作: [+ 新建事项] (Primary 蓝色)
- 次要操作: [待办] [分类] [导入] (Secondary 白色)
- 工具操作: [刷新] [导出] (Ghost 透明)

### 任务 2.2: 现代化 ListView
**文件**: [`WorkLogApp.UI/Forms/MainForm.cs`](WorkLogApp.UI/Forms/MainForm.cs:1)

**样式改进**:
```csharp
private void ApplyFluentListView()
{
    _listView.BorderStyle = BorderStyle.None;
    _listView.FullRowSelect = true;
    _listView.GridLines = false;
    _listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
    _listView.BackColor = FluentColors.Gray10;
    _listView.ForeColor = FluentColors.Gray190;
    
    // 自定义绘制
    _listView.OwnerDraw = true;
    _listView.DrawItem += OnListViewDrawItem;
    _listView.DrawSubItem += OnListViewDrawSubItem;
    _listView.DrawColumnHeader += OnListViewDrawHeader;
}
```

### 任务 2.3: 状态徽章组件
**新文件**: [`WorkLogApp.UI/Controls/FluentBadge.cs`](WorkLogApp.UI/Controls/FluentBadge.cs:1)

**功能**:
- 显示待办/进行中/完成状态
- 颜色编码: 待办(Gray), 进行(Warning), 完成(Success)
- 圆角胶囊样式
- 尺寸: 高度 24px, 内边距 8px

### 任务 2.4: 重构 ItemCreateForm 布局
**文件**: [`WorkLogApp.UI/Forms/ItemCreateForm.Designer.cs`](WorkLogApp.UI/Forms/ItemCreateForm.Designer.cs:1)

**布局重构要点**:
1. 使用 TableLayoutPanel 替代现有布局
2. 标签右对齐，输入框左对齐
3. 输入框高度统一 36px
4. 表单分组使用分隔线
5. 底部操作栏固定高度 64px

**表单结构**:
```
┌─────────────────────────────────────────┐
│ 基本信息                                │ ← Group
├─────────────────────────────────────────┤
│  日期 *    [2026-03-03]                 │
│  分类 *    [研发/API设计        ▼]      │
│  标题 *    [输入框                      ]│
│  标签      [输入框                      ]│
├─────────────────────────────────────────┤
│ 模板字段                                │ ← Group (动态)
├─────────────────────────────────────────┤
│                              [取消][保存]│ ← Action Bar
└─────────────────────────────────────────┘
```

### 任务 2.5: 重构 CategoryManageForm 布局
**文件**: [`WorkLogApp.UI/Forms/CategoryManageForm.cs`](WorkLogApp.UI/Forms/CategoryManageForm.cs:1)

**布局改进**:
1. 左侧树面板宽度固定 280px
2. 树节点高度 40px
3. 添加搜索框在树上方
4. 右侧面板使用卡片式分组
5. 占位符表格行高 44px

---

## 阶段三：交互优化 (预计 1-2 周)

### 任务 3.1: 按钮交互动画
**新文件**: [`WorkLogApp.UI/UI/AnimationHelper.cs`](WorkLogApp.UI/UI/AnimationHelper.cs:1)

**实现功能**:
```csharp
public static class AnimationHelper
{
    public static void AnimateHover(Control control, Color targetColor, int duration = 150)
    {
        // 背景色过渡动画
    }
    
    public static void AnimatePress(Button button, int duration = 100)
    {
        // 缩放 0.98 效果
    }
    
    public static void AnimateFocus(Control control)
    {
        // 焦点环动画
    }
}
```

### 任务 3.2: 列表项悬停效果
**修改**: [`WorkLogApp.UI/Forms/MainForm.cs`](WorkLogApp.UI/Forms/MainForm.cs:1)

**效果**:
- 悬停时背景变为 Gray20
- 左侧显示拖拽手柄图标
- 过渡时间 100ms

### 任务 3.3: 输入框焦点效果
**修改**: [`WorkLogApp.UI/UI/UIStyleManager.cs`](WorkLogApp.UI/UI/UIStyleManager.cs:1)

**效果**:
- 获得焦点时边框变为 Primary 蓝色
- 添加外发光效果 (2px Primary 透明度 30%)
- 过渡时间 150ms

### 任务 3.4: 对话框过渡动画
**修改**: 所有 Form 类

**实现**:
```csharp
protected override void OnShown(EventArgs e)
{
    base.OnShown(e);
    
    // 淡入动画
    Opacity = 0;
    var timer = new Timer { Interval = 16 };
    float progress = 0;
    
    timer.Tick += (s, ev) =>
    {
        progress += 0.1f;
        Opacity = Math.Min(progress, 1);
        if (progress >= 1) timer.Stop();
    };
    timer.Start();
}
```

---

## 阶段四：高级效果 (可选，持续优化)

### 任务 4.1: Acrylic 材质背景
**说明**: WinForms 实现 Acrylic 需要 P/Invoke 调用 Windows API
**复杂度**: 高
**优先级**: 低

### 任务 4.2: Reveal 高亮效果
**说明**: 鼠标悬停时按钮的高光跟随效果
**实现**: 自定义 Button 控件，处理 MouseMove 事件

### 任务 4.3: Elevation 阴影系统
**说明**: 使用分层阴影表达深度
**实现**: 自定义 Panel 控件，绘制多层阴影

---

## 快速启动检查清单

### 立即可以开始 (阶段一)

- [ ] 创建 `FluentColors.cs` 常量文件
- [ ] 创建 `FluentTypography.cs` 字体常量
- [ ] 修改 `UIStyleManager.cs` 应用新颜色和字体
- [ ] 将所有按钮高度改为 36px
- [ ] 为所有按钮添加 4px 圆角
- [ ] 更新 `MainForm` 背景色为 Gray10

### 短期目标 (阶段二)

- [ ] 重构 `MainForm` 工具栏布局
- [ ] 现代化 `ListView` 样式
- [ ] 创建 `FluentBadge` 状态徽章
- [ ] 重构 `ItemCreateForm` 表单布局
- [ ] 重构 `CategoryManageForm` 布局

### 中期目标 (阶段三)

- [ ] 实现按钮悬停动画
- [ ] 实现列表项悬停效果
- [ ] 实现输入框焦点效果
- [ ] 添加对话框过渡动画

---

## 代码实现优先级示例

### 优先级 1: 颜色常量 (30分钟)

```bash
# 创建文件
touch WorkLogApp.UI/UI/FluentColors.cs

# 复制规范中的颜色定义
# 更新 UIStyleManager 引用新常量
```

### 优先级 2: 按钮样式 (1小时)

```csharp
// 在 UIStyleManager 中添加
public static void ApplyFluentButton(Button btn, bool isPrimary = false)
{
    btn.FlatStyle = FlatStyle.Flat;
    btn.Height = 36;
    btn.Padding = new Padding(16, 0, 16, 0);
    
    if (isPrimary)
    {
        btn.BackColor = FluentColors.Primary;
        btn.ForeColor = Color.White;
        btn.FlatAppearance.BorderSize = 0;
    }
    else
    {
        btn.BackColor = Color.White;
        btn.ForeColor = FluentColors.Gray190;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = FluentColors.Gray80;
    }
    
    // 圆角
    ApplyRoundedCorners(btn, 4);
}
```

### 优先级 3: 列表样式 (2小时)

```csharp
// 在 MainForm 中更新 BindListView
private void BindListView(List<WorkLogItem> items)
{
    _listView.BeginUpdate();
    _listView.Items.Clear();
    
    foreach (var item in items)
    {
        var lv = new ListViewItem(item.LogDate.ToString("yyyy-MM-dd")) { Tag = item };
        lv.SubItems.Add(item.ItemTitle ?? "");
        lv.SubItems.Add(item.Status.ToChinese());
        lv.SubItems.Add(item.ItemContent ?? "");
        lv.SubItems.Add(item.Tags ?? "");
        lv.SubItems.Add(item.StartTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
        lv.SubItems.Add(item.EndTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
        
        // 根据状态设置颜色
        switch (item.Status)
        {
            case StatusEnum.Done:
                lv.ForeColor = FluentColors.Success;
                break;
            case StatusEnum.Doing:
                lv.ForeColor = FluentColors.Warning;
                break;
            default:
                lv.ForeColor = FluentColors.Gray130;
                break;
        }
        
        _listView.Items.Add(lv);
    }
    _listView.EndUpdate();
}
```

---

## 验收标准

### 阶段一验收

1. 所有颜色引用 `FluentColors` 常量
2. 所有按钮高度 36px，圆角 4px
3. 字体使用 Segoe UI Variable 系列
4. 应用背景色为 Gray10

### 阶段二验收

1. MainForm 工具栏布局整齐，间距统一
2. ListView 无边框，无网格线，选中态明显
3. 状态使用徽章组件显示，颜色正确
4. 所有表单布局符合 8pt 网格

### 阶段三验收

1. 所有按钮有悬停/按下视觉反馈
2. 列表项悬停时有背景变化
3. 输入框焦点状态明显
4. 对话框有平滑的显示/隐藏动画

---

**路线图书写**: 2026-03-03
**版本**: 1.0
**维护者**: UI设计团队
