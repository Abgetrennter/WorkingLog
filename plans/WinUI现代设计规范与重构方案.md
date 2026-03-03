# 工作日志应用 - WinUI/Fluent Design 现代化重构方案

## 1. 设计原则概述

### 1.1 Fluent Design System 核心要素

| 设计原则 | 说明 | 应用策略 |
|---------|------|----------|
| **Light** | 光线作为交互语言 | 使用亚克力材质和Reveal高亮效果 |
| **Depth** | 分层与深度感 | 通过Elevation阴影建立层级关系 |
| **Motion** | 流畅的动画过渡 | 所有状态变化需有150-300ms缓动动画 |
| **Material** | 类实体材质感 | 毛玻璃效果配合微妙噪点纹理 |
| **Scale** | 自适应布局 | 8pt网格系统支持100%-200%缩放 |

### 1.2 信息架构重构

```
应用层级结构:
├── 顶层导航栏 (App Bar)
│   ├── 品牌标识 + 应用名称
│   ├── 主要操作按钮 (创建/导入)
│   └── 设置/帮助入口
├── 内容区域 (Content Canvas)
│   ├── 筛选器栏 (Filter Bar)
│   ├── 主数据列表 (Data Grid)
│   └── 状态指示器
└── 侧边栏 (Side Pane) - 可选
    ├── 分类树 (Category Tree)
    └── 统计概览 (Statistics)
```

---

## 2. 色彩系统规范

### 2.1 主色调板

#### 主品牌色 (Brand Color)
```
Primary:    #0078D4  (Azure Blue)
Light:      #4FA1E4  (Hover状态)
Dark:       #005A9E  (Pressed状态)
Lighter:    #E5F1FA  (背景高亮)
```

#### 语义色 (Semantic Colors)
```
Success:    #107C10  (已完成)
Warning:    #FFB900  (进行中/警告)
Error:      #D13438  (错误/删除)
Info:       #0078D4  (信息提示)
```

### 2.2 中性色阶 (Neutral Palette)

```
Black:      #000000  (Primary Text)
Gray190:    #201F1E  (Body Text)
Gray160:    #323130  (Secondary Text)
Gray130:    #605E5C  (Tertiary Text)
Gray100:    #8A8886  (Disabled/Placeholder)
Gray80:     #B3B0AD  (Borders - Hover)
Gray60:     #C8C6C4  (Borders - Default)
Gray40:     #E1DFDD  (Dividers)
Gray20:     #F3F2F1  (Background Alt)
Gray10:     #FAF9F8  (Background)
White:      #FFFFFF  (Surface)
```

### 2.3 材质与背景

#### 亚克力材质 (Acrylic)
```csharp
// 主内容区背景
AcrylicBase:        rgba(243, 243, 243, 0.85)
AcrylicOverlay:     rgba(255, 255, 255, 0.4)
TintColor:          #F3F2F1
TintOpacity:        0.85
NoiseOpacity:       0.03

// 侧边栏/浮层面板
AcrylicChrome:      rgba(230, 230, 230, 0.8)
TintColorChrome:    #E6E6E6
```

#### Elevation 阴影系统
```
Level 1 (Card):     0 2px 4px rgba(0,0,0,0.04)
Level 2 (Menu):     0 4px 8px rgba(0,0,0,0.08)
Level 3 (Dialog):   0 8px 16px rgba(0,0,0,0.12)
Level 4 (Tooltip):  0 12px 24px rgba(0,0,0,0.16)
```

---

## 3. 字体层级规范 (Segoe UI Variable)

### 3.1 字体家族

```
Display:     Segoe UI Variable Display
Text:        Segoe UI Variable Text
Small:       Segoe UI Variable Small

备用字体栈: 'Segoe UI Variable', 'Segoe UI', 'Microsoft YaHei UI', sans-serif
```

### 3.2 字体层级表

| 层级 | 用途 | 字号 | 字重 | 行高 | 字间距 |
|-----|------|------|------|------|--------|
| **Header** | 页面标题 | 28px | 600 (SemiBold) | 36px | -0.02em |
| **Title Large** | 区块标题 | 20px | 600 | 28px | -0.01em |
| **Title** | 卡片/对话框标题 | 16px | 600 | 24px | 0 |
| **Subtitle** | 副标题/分组标签 | 14px | 600 | 20px | 0 |
| **Body Large** | 主要正文 | 15px | 400 (Regular) | 24px | 0 |
| **Body** | 次要正文 | 14px | 400 | 20px | 0 |
| **Caption** | 辅助说明 | 12px | 400 | 16px | 0.01em |
| **Overline** | 标签/时间戳 | 10px | 600 | 12px | 0.04em |

### 3.3 WinForms字体映射实现

```csharp
// UIStyleManager.cs 扩展
public static class FluentTypography
{
    // 主字体 - 使用Segoe UI Variable Text作为默认
    public static Font Header => new Font("Segoe UI Variable", 21f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font TitleLarge => new Font("Segoe UI Variable", 15f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Title => new Font("Segoe UI Variable", 12f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Subtitle => new Font("Segoe UI Variable", 10.5f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font BodyLarge => new Font("Segoe UI Variable", 11.25f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font Body => new Font("Segoe UI Variable", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font Caption => new Font("Segoe UI Variable", 9f, FontStyle.Regular, GraphicsUnit.Point);
    
    // 中文回退
    public static Font BodyChinese => new Font("Microsoft YaHei UI", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
}
```

---

## 4. 8pt网格间距系统

### 4.1 间距令牌

```
Base Unit: 4px

Spacing:
  xs:   4px   (0.5u)   - 紧凑间距
  sm:   8px   (1u)     - 默认元素间距
  md:   16px  (2u)     - 组件内边距
  lg:   24px  (3u)     - 区块间距
  xl:   32px  (4u)     - 页面边距
  2xl:  48px  (6u)     - 大面积分隔
  3xl:  64px  (8u)     - 区域分隔

Layout:
  Page Padding:     24px (xl)
  Section Gap:      32px (2xl)
  Card Padding:     16px (md)
  List Item Height: 48px (6u)
  Button Height:    36px (4.5u) / 32px (4u) compact
  Input Height:     36px (4.5u)
```

### 4.2 布局网格

```
应用窗口最小宽度: 1024px
最大内容宽度:     1440px

网格配置:
  Columns: 12
  Gutter:  24px
  Margin:  24px (desktop) / 16px (tablet)
  
响应式断点:
  Compact:   0 - 640px
  Medium:    641 - 1007px
  Expanded:  1008px+
```

---

## 5. 界面布局线框图

### 5.1 主界面 (MainForm) 布局重构

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ [Logo] 工作日志                                    [设置 ▼] [帮助 ▼]         │ ← 顶部导航栏 (56px)
├─────────────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ [+ 新建]  [筛选 ▼]  [日历选择]  [按月 □]        [搜索...        ] [🔍] │ │ ← 工具栏 (48px)
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░│ │
│ │ ░  日期    │ 标题              │ 状态   │ 内容摘要        │ 标签       ░│ │
│ │ ░──────────────────────────────────────────────────────────────────────░│ │
│ │ ░  03-01   │ 需求评审会议      │ ● 待办 │ 讨论Q2版本规划...│ 会议       ░│ │ ← 数据列表
│ │ ░  03-01   │ 接口开发          │ ● 进行 │ 用户模块API设计  │ 研发       ░│ │
│ │ ░  02-28   │ Bug修复 #2341     │ ✓ 完成 │ 修复登录异常问题 │ 缺陷       ░│ │
│ │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░│ │
│ │                                                                       │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ 共 23 项  │  ◀  1 2 3 ... 5  ▶  │                    选择: 3项  [删除] [导出] │ ← 底部状态栏 (40px)
└─────────────────────────────────────────────────────────────────────────────┘

尺寸规格:
- 窗口: 最小 1200 x 768px, 推荐 1440 x 900px
- 顶部导航栏: 高度 56px, 背景 White, 底部 1px Gray40
- 工具栏: 高度 48px, 内边距 16px, 背景 White
- 数据列表: 填充剩余空间, 行高 48px
- 底部栏: 高度 40px, 背景 Gray10
```

#### 主界面控件规范

```csharp
// 顶部导航栏样式
public static void ApplyFluentAppBar(Form form)
{
    var navBar = new Panel
    {
        Height = 56,
        Dock = DockStyle.Top,
        BackColor = Color.White,
        Padding = new Padding(24, 0, 24, 0)
    };
    
    // 底部边框
    var bottomBorder = new Panel
    {
        Height = 1,
        Dock = DockStyle.Bottom,
        BackColor = Color.FromArgb(225, 223, 221) // Gray40
    };
    navBar.Controls.Add(bottomBorder);
    
    // Logo与标题
    var titleLabel = new Label
    {
        Text = "工作日志",
        Font = FluentTypography.TitleLarge,
        ForeColor = Color.FromArgb(32, 31, 30), // Gray190
        AutoSize = true,
        Location = new Point(24, 16)
    };
    navBar.Controls.Add(titleLabel);
}

// 主要操作按钮 (Primary Button)
public static void ApplyFluentPrimaryButton(Button btn)
{
    btn.Height = 36;
    btn.FlatStyle = FlatStyle.Flat;
    btn.FlatAppearance.BorderSize = 0;
    btn.BackColor = Color.FromArgb(0, 120, 212); // Primary
    btn.ForeColor = Color.White;
    btn.Font = FluentTypography.Body;
    btn.Padding = new Padding(16, 0, 16, 0);
    btn.Cursor = Cursors.Hand;
    
    // 圆角 4px
    btn.Region = CreateRoundedRegion(btn.Width, btn.Height, 4);
}

// 次要按钮 (Secondary Button)
public static void ApplyFluentSecondaryButton(Button btn)
{
    btn.Height = 36;
    btn.FlatStyle = FlatStyle.Flat;
    btn.FlatAppearance.BorderSize = 1;
    btn.FlatAppearance.BorderColor = Color.FromArgb(200, 198, 196); // Gray80
    btn.BackColor = Color.White;
    btn.ForeColor = Color.FromArgb(32, 31, 30); // Gray190
    btn.Font = FluentTypography.Body;
    btn.Padding = new Padding(16, 0, 16, 0);
    btn.Cursor = Cursors.Hand;
    btn.Region = CreateRoundedRegion(btn.Width, btn.Height, 4);
}
```

### 5.2 创建/编辑表单 (ItemCreateForm) 布局重构

```
┌─────────────────────────────────────────────────────────────┐
│ 创建日志事项                                    [×]          │ ← 对话框标题栏 (48px)
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  基本信息                                                    │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                             │
│  日期 *                        状态 *                       │
│  ┌──────────────────────┐      ┌──────────────────────┐    │
│  │  2026-03-03          │      │  ● 待办      ▼      │    │
│  └──────────────────────┘      └──────────────────────┘    │
│                                                             │
│  分类 *                                                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  研发 / 后端开发 / API设计                    ▼      │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  标题 *                                                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  用户认证接口开发                                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  标签                                                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  后端, API, 高优先级                                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  模板字段 (根据所选分类动态生成)                             │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                             │
│  开发内容                                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                                                      │  │
│  │  1. 设计JWT认证流程                                   │  │
│  │  2. 实现Token生成与验证                               │  │
│  │                                                      │  │
│  │                                                      │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  相关模块         [☑] 需要代码审查                         │
│  ┌────────────┐                                            │
│  │ Auth       │                                            │
│  └────────────┘                                            │
│                                                             │
│  时间范围                                                   │
│  ┌──────────────┐  ───────  ┌──────────────┐              │
│  │ 09:00   ▼   │            │ 17:30   ▼   │              │
│  └──────────────┘            └──────────────┘              │
│                                                             │
│                                                             │
│                              [取消]  [      保存      ]     │ ← 底部操作栏 (64px)
│                                                             │
└─────────────────────────────────────────────────────────────┘

尺寸规格:
- 对话框宽度: 600px (标准) / 800px (宽屏)
- 标题栏: 高度 48px, 字体 Title, 背景 White
- 内容区: 内边距 24px, 背景 White
- 表单字段间距: 16px (垂直)
- 输入框高度: 36px
- 文本域最小高度: 80px
- 底部栏: 高度 64px, 背景 Gray10, 内边距 16px 24px
```

#### 表单控件规范

```csharp
// Fluent TextBox
public static void ApplyFluentTextBox(TextBox tb)
{
    tb.Height = 36;
    tb.BorderStyle = BorderStyle.None;
    tb.Font = FluentTypography.Body;
    tb.BackColor = Color.FromArgb(250, 249, 248); // Gray10
    tb.Padding = new Padding(12, 8, 12, 8);
    
    // 自定义边框绘制
    tb.Paint += (s, e) => 
    {
        var g = e.Graphics;
        var rect = tb.ClientRectangle;
        
        // 背景
        g.FillRectangle(new SolidBrush(tb.BackColor), rect);
        
        // 边框 - 根据焦点状态变化
        var borderColor = tb.Focused 
            ? Color.FromArgb(0, 120, 212) // Primary
            : Color.FromArgb(225, 223, 221); // Gray40
            
        using (var pen = new Pen(borderColor, 1))
        {
            g.DrawRoundedRectangle(pen, rect, 4);
        }
    };
}

// Fluent ComboBox
public static void ApplyFluentComboBox(ComboBox cb)
{
    cb.DropDownStyle = ComboBoxStyle.DropDownList;
    cb.FlatStyle = FlatStyle.Flat;
    cb.Height = 36;
    cb.Font = FluentTypography.Body;
    cb.BackColor = Color.White;
    
    // 自定义下拉箭头
    cb.DrawMode = DrawMode.OwnerDrawFixed;
    cb.DrawItem += (s, e) =>
    {
        e.DrawBackground();
        
        if (e.Index >= 0)
        {
            var item = cb.Items[e.Index];
            var text = item?.ToString() ?? "";
            
            using (var brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds.Left + 12, e.Bounds.Top + 8);
            }
        }
        
        e.DrawFocusRectangle();
    };
}
```

### 5.3 分类管理界面 (CategoryManageForm) 布局重构

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ 分类与模板管理                                               [×]           │
├──────────────────────────┬──────────────────────────────────────────────────┤
│                          │                                                  │
│  ┌────────────────────┐  │  模板详情                                        │
│  │ 🔍 搜索分类...     │  │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│  ├────────────────────┤  │                                                  │
│  │ 📁 会议             │  │  名称                                            │
│  │   ├─ 需求评审      │  │  ┌────────────────────────────────────────────┐│
│  │   ├─ 站会          │  │  │  需求评审会议模板                            ││
│  │   └─ 回顾会议      │  │  └────────────────────────────────────────────┘│
│  │ 📁 研发            │  │                                                  │
│  │   ├─ 前端开发      │  │  标签                                            │
│  │   │   └─ 📝组件开发│  │  ┌────────────────────────────────────────────┐│
│  │   ├─ 后端开发      │  │  │  会议, 需求, 评审                            ││
│  │   │   └─ 📝API设计 │  │  └────────────────────────────────────────────┘│
│  │   └─ 📝Bug修复     │  │                                                  │
│  │ 📁 文档            │  │  模板内容                                        │
│  │   └─ 📝技术文档    │  │  ┌────────────────────────────────────────────┐│
│  │                    │  │  │                                            ││
│  │                    │  │  │  ## {会议主题}                             ││
│  │                    │  │  │                                            ││
│  │                    │  │  │  **参会人员**: {参会人员}                  ││
│  │                    │  │  │  **会议结论**: {结论}                      ││
│  │                    │  │  │                                            ││
│  └────────────────────┘  │  └────────────────────────────────────────────┘│
│                          │                                                  │
│  [+ 新建分类] [+ 新建模板]│  占位符配置                                      │
│                          │  ┌────────────────────────────────────────────┐│
│                          │  │ 名称      │ 类型      │ 选项               ││
│                          │  ├───────────┼───────────┼────────────────────┤│
│                          │  │ 会议主题  │ 文本      │                    ││
│                          │  │ 参会人员  │ 多行文本  │                    ││
│                          │  │ 会议类型  │ 下拉选择  │ 线上, 线下        ││
│                          │  │ 结论      │ 多行文本  │                    ││
│                          │  └────────────────────────────────────────────┘│
│                          │                                                  │
│                          │  [+ 添加占位符]                                  │
│                          │                                                  │
│                          │                              [取消]  [保存]      │
├──────────────────────────┴──────────────────────────────────────────────────┤
│ 提示: 拖拽可调整分类顺序                                                      │
└─────────────────────────────────────────────────────────────────────────────┘

尺寸规格:
- 窗口: 1000 x 700px
- 左侧边栏: 宽度 280px, 背景 Gray10
- 搜索框: 高度 36px, 内边距 12px
- 树节点高度: 40px
- 右侧内容区: 内边距 24px
- 模板编辑器: 最小高度 200px
- 占位符表格: 行高 44px
```

---

## 6. 交互状态与动画规范

### 6.1 按钮状态

```
Primary Button States:
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  Default:     [保存]  BG: #0078D4, Text: White              │
│                                                             │
│  Hover:       [保存]  BG: #106EBE, Elevation: +2px          │
│                       Transition: 150ms ease-out            │
│                                                             │
│  Pressed:     [保存]  BG: #005A9E, Scale: 0.98              │
│                       Transition: 100ms ease-in             │
│                                                             │
│  Disabled:    [保存]  BG: #F3F2F1, Text: #A19F9D           │
│                       Cursor: not-allowed                   │
│                                                             │
│  Focus:       [保存]  Ring: 2px #0078D4 (外发光)            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Reveal 高亮效果

```csharp
// Reveal效果 - 鼠标悬停时的高光
public class RevealButton : Button
{
    private bool _isHovering;
    private Point _mousePos;
    
    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovering = true;
        base.OnMouseEnter(e);
        Invalidate();
    }
    
    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovering = false;
        base.OnMouseLeave(e);
        Invalidate();
    }
    
    protected override void OnMouseMove(MouseEventArgs e)
    {
        _mousePos = e.Location;
        base.OnMouseMove(e);
        Invalidate();
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        if (_isHovering)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 径向渐变高光
            using (var brush = new RadialGradientBrush(
                new Rectangle(_mousePos.X - 50, _mousePos.Y - 50, 100, 100),
                Color.FromArgb(40, 255, 255, 255),
                Color.Transparent))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
```

### 6.3 动画规范

```csharp
// 动画时长常量
public static class FluentDurations
{
    public const int Instant = 0;      // 即时
    public const int Fast = 150;       // 快速反馈
    public const int Normal = 250;     // 标准过渡
    public const int Slow = 350;       // 强调动画
    public const int Page = 500;       // 页面切换
}

// 缓动函数
public static class FluentEasings
{
    // 标准: ease-out
    public static float EaseOut(float t) => 1 - (1 - t) * (1 - t);
    
    // 进入: ease-out-cubic
    public static float EaseOutCubic(float t) => 1 - (float)Math.Pow(1 - t, 3);
    
    // 退出: ease-in-cubic
    public static float EaseInCubic(float t) => (float)Math.Pow(t, 3);
    
    // 强调: spring
    public static float Spring(float t) => (float)(1 - Math.Pow(2, -10 * t) * Math.Cos(t * Math.PI * 3));
}

// 按钮点击动画
public static void AnimateButtonPress(Button btn)
{
    var timer = new Timer { Interval = 16 }; // 60fps
    float progress = 0;
    const float duration = 100f; // ms
    
    timer.Tick += (s, e) =>
    {
        progress += timer.Interval;
        float t = Math.Min(progress / duration, 1);
        
        // 按下: 缩小到 0.98
        float scale = 1 - 0.02f * FluentEasings.EaseInCubic(t);
        btn.Scale(new SizeF(scale, scale));
        
        if (t >= 1)
        {
            timer.Stop();
            // 反弹动画
            AnimateButtonRelease(btn);
        }
    };
    
    timer.Start();
}
```

### 6.4 列表项交互

```
ListView Item States:
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  Default:  透明背景, Text: Gray190                          │
│                                                             │
│  Hover:    BG: #F3F2F1 (Gray20), Transition: 100ms          │
│            左侧显示拖拽手柄 (⋮⋮)                            │
│                                                             │
│  Selected: BG: #E5F1FA (Primary Lighter)                    │
│            Left Border: 3px #0078D4                         │
│            Text: #0078D4                                    │
│                                                             │
│  Pressed:  BG: #CCE3F1 (Primary Light)                      │
│            Scale: 0.995                                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 7. 图标系统

### 7.1 图标规范

```
Icon System: Segoe Fluent Icons
Size: 16px (small), 20px (default), 24px (large)
Weight: Regular

图标映射表:
新增:       \uE710  (Add)
编辑:       \uE70F  (Edit)
删除:       \uE74D  (Delete)
保存:       \uE74E  (Save)
刷新:       \uE72C  (Refresh)
搜索:       \uE721  (Search)
设置:       \uE713  (Settings)
筛选:       \uE71C  (Filter)
日历:       \uE787  (Calendar)
文件夹:     \uE8B7  (Folder)
文档:       \uE8A5  (Document)
完成:       \uE73E  (CheckMark)
取消:       \uE711  (Cancel)
更多:       \uE712  (More)
展开:       \uE70D  (ChevronDown)
收起:       \uE70E  (ChevronUp)
上一条:     \uE70A  (ChevronLeft)
下一条:     \uE76C  (ChevronRight)
拖拽:       \uE784  (GripperBarVertical)
导出:       \uE7B8  (Export)
导入:       \uE8B5  (Import)
```

### 7.2 图标按钮实现

```csharp
public class FluentIconButton : Button
{
    public string IconGlyph { get; set; }
    public int IconSize { get; set; } = 20;
    
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        
        // 背景
        using (var bgBrush = new SolidBrush(BackColor))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }
        
        // 图标
        if (!string.IsNullOrEmpty(IconGlyph))
        {
            using (var font = new Font("Segoe Fluent Icons", IconSize))
            using (var brush = new SolidBrush(ForeColor))
            {
                var size = g.MeasureString(IconGlyph, font);
                var x = (Width - size.Width) / 2;
                var y = (Height - size.Height) / 2;
                g.DrawString(IconGlyph, font, brush, x, y);
            }
        }
        
        // 文字
        if (!string.IsNullOrEmpty(Text))
        {
            using (var brush = new SolidBrush(ForeColor))
            {
                var size = g.MeasureString(Text, Font);
                var x = (Width - size.Width) / 2;
                var y = (Height - size.Height) / 2 + IconSize / 2 + 4;
                g.DrawString(Text, Font, brush, x, y);
            }
        }
    }
}
```

---

## 8. 组件库实施建议

### 8.1 创建 FluentUI 控件库

建议创建一个新的 `WorkLogApp.UI.Fluent` 命名空间，包含以下控件：

```
WorkLogApp.UI.Fluent/
├── Controls/
│   ├── FluentButton.cs          # 主/次/文本按钮
│   ├── FluentTextBox.cs         # 输入框
│   ├── FluentComboBox.cs        # 下拉选择
│   ├── FluentDatePicker.cs      # 日期选择
│   ├── FluentListView.cs        # 数据列表
│   ├── FluentTreeView.cs        # 树形控件
│   ├── FluentCard.cs            # 卡片容器
│   ├── FluentDialog.cs          # 对话框
│   ├── FluentToggleSwitch.cs    # 开关
│   ├── FluentBadge.cs           # 状态徽章
│   └── FluentNavigationView.cs  # 导航视图
├── Styling/
│   ├── FluentColors.cs          # 色彩常量
│   ├── FluentTypography.cs      # 字体常量
│   ├── FluentSpacing.cs         # 间距常量
│   └── FluentEffects.cs         # 效果工具
└── Animation/
    ├── AnimationHelper.cs       # 动画辅助
    ├── EasingFunctions.cs       # 缓动函数
    └── Transitions.cs           # 过渡效果
```

### 8.2 渐进式迁移策略

```
阶段一: 基础样式层 (1-2周)
- 更新 UIStyleManager 应用Fluent色彩
- 标准化字体和间距
- 替换所有按钮为圆角样式

阶段二: 核心组件重构 (2-3周)
- 重构 MainForm 布局
- 实现新的 ListView 样式
- 更新所有表单对话框

阶段三: 高级效果 (1-2周)
- 添加动画过渡
- 实现 Acrylic 背景效果
- 添加 Reveal 高亮

阶段四: 交互优化 (持续)
- 添加键盘导航
- 优化焦点管理
- 提升可访问性
```

---

## 9. 代码实现示例

### 9.1 更新后的 UIStyleManager

```csharp
public static class FluentStyleManager
{
    // 色彩
    public static Color Primary => Color.FromArgb(0, 120, 212);
    public static Color PrimaryLight => Color.FromArgb(79, 161, 228);
    public static Color PrimaryDark => Color.FromArgb(0, 90, 158);
    public static Color PrimaryLighter => Color.FromArgb(229, 241, 250);
    
    // 中性色
    public static Color Gray190 => Color.FromArgb(32, 31, 30);
    public static Color Gray160 => Color.FromArgb(50, 49, 48);
    public static Color Gray130 => Color.FromArgb(96, 94, 92);
    public static Color Gray100 => Color.FromArgb(138, 136, 134);
    public static Color Gray80 => Color.FromArgb(200, 198, 196);
    public static Color Gray60 => Color.FromArgb(200, 198, 196);
    public static Color Gray40 => Color.FromArgb(225, 223, 221);
    public static Color Gray20 => Color.FromArgb(243, 242, 241);
    public static Color Gray10 => Color.FromArgb(250, 249, 248);
    
    // 语义色
    public static Color Success => Color.FromArgb(16, 124, 16);
    public static Color Warning => Color.FromArgb(255, 185, 0);
    public static Color Error => Color.FromArgb(209, 52, 56);
    
    // 应用主题
    public static void ApplyFluentTheme(Form form)
    {
        form.BackColor = Gray10;
        form.Font = FluentTypography.Body;
        
        ApplyToControlTree(form);
        
        // 启用双缓冲减少闪烁
        typeof(Form).GetProperty("DoubleBuffered", 
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(form, true);
    }
    
    private static void ApplyToControlTree(Control parent)
    {
        foreach (Control ctrl in parent.Controls)
        {
            ApplyControlStyle(ctrl);
            
            if (ctrl.HasChildren)
            {
                ApplyToControlTree(ctrl);
            }
        }
    }
    
    private static void ApplyControlStyle(Control ctrl)
    {
        switch (ctrl)
        {
            case Button btn:
                ApplyButtonStyle(btn);
                break;
            case TextBox tb:
                ApplyTextBoxStyle(tb);
                break;
            case ComboBox cb:
                ApplyComboBoxStyle(cb);
                break;
            case ListView lv:
                ApplyListViewStyle(lv);
                break;
            case Label lbl:
                lbl.ForeColor = Gray190;
                break;
        }
    }
    
    private static void ApplyButtonStyle(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Height = 36;
        btn.Padding = new Padding(16, 0, 16, 0);
        btn.Font = FluentTypography.Body;
        btn.Cursor = Cursors.Hand;
        
        // 判断按钮类型
        var tag = btn.Tag as string;
        if (tag?.Contains("primary") == true)
        {
            btn.BackColor = Primary;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.MouseOverBackColor = PrimaryLight;
            btn.FlatAppearance.MouseDownBackColor = PrimaryDark;
        }
        else
        {
            btn.BackColor = Color.White;
            btn.ForeColor = Gray190;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Gray80;
            btn.FlatAppearance.MouseOverBackColor = Gray20;
            btn.FlatAppearance.MouseDownBackColor = Gray40;
        }
        
        // 圆角
        btn.Paint += (s, e) => DrawRoundedButton(e.Graphics, btn);
    }
    
    private static void DrawRoundedButton(Graphics g, Button btn)
    {
        var rect = btn.ClientRectangle;
        rect.Inflate(-1, -1);
        
        using (var path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, 8, 8, 180, 90);
            path.AddArc(rect.Right - 8, rect.Y, 8, 8, 270, 90);
            path.AddArc(rect.Right - 8, rect.Bottom - 8, 8, 8, 0, 90);
            path.AddArc(rect.X, rect.Bottom - 8, 8, 8, 90, 90);
            path.CloseFigure();
            
            btn.Region = new Region(path);
        }
    }
}
```

### 9.2 状态徽章控件

```csharp
public class FluentBadge : Label
{
    public enum BadgeStyle
    {
        Default,
        Success,
        Warning,
        Error,
        Info
    }
    
    private BadgeStyle _style = BadgeStyle.Default;
    
    public BadgeStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            UpdateAppearance();
            Invalidate();
        }
    }
    
    public FluentBadge()
    {
        AutoSize = false;
        Height = 24;
        Padding = new Padding(8, 2, 8, 2);
        TextAlign = ContentAlignment.MiddleCenter;
        Font = FluentTypography.Caption;
        UpdateAppearance();
    }
    
    private void UpdateAppearance()
    {
        switch (_style)
        {
            case BadgeStyle.Success:
                BackColor = Color.FromArgb(16, 124, 16);
                ForeColor = Color.White;
                break;
            case BadgeStyle.Warning:
                BackColor = Color.FromArgb(255, 185, 0);
                ForeColor = Color.FromArgb(50, 49, 48);
                break;
            case BadgeStyle.Error:
                BackColor = Color.FromArgb(209, 52, 56);
                ForeColor = Color.White;
                break;
            case BadgeStyle.Info:
                BackColor = Color.FromArgb(0, 120, 212);
                ForeColor = Color.White;
                break;
            default:
                BackColor = FluentStyleManager.Gray20;
                ForeColor = FluentStyleManager.Gray160;
                break;
        }
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        
        // 绘制圆角背景
        using (var brush = new SolidBrush(BackColor))
        using (var path = GetRoundedRect(ClientRectangle, 12))
        {
            g.FillPath(brush, path);
        }
        
        // 绘制文字
        TextRenderer.DrawText(g, Text, Font, 
            new Rectangle(0, 0, Width, Height), ForeColor, 
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
    
    private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;
        
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        
        return path;
    }
}
```

---

## 10. 总结与实施优先级

### 10.1 高优先级 (立即实施)

1. **色彩系统统一化**
   - 更新 `UIStyleManager` 使用 Fluent 色彩常量
   - 所有按钮应用圆角和统一高度 (36px)
   - 建立中性色阶使用规范

2. **字体层级规范化**
   - 使用 Segoe UI Variable 作为主要字体
   - 建立标题/正文/说明文字层级
   - 统一字号和行高

3. **8pt 间距系统**
   - 组件内边距统一为 16px
   - 页面边距统一为 24px
   - 列表项高度统一为 48px

### 10.2 中优先级 (短期实施)

1. **主界面布局重构**
   - 顶部导航栏 (56px)
   - 工具栏 (48px)
   - 现代化 ListView (行高 48px, 选中态)

2. **表单界面重构**
   - 输入框统一高度 36px
   - 标签右对齐，输入框左对齐
   - 分组使用分隔线和标题

3. **图标系统**
   - 引入 Segoe Fluent Icons
   - 按钮图标化改造

### 10.3 低优先级 (长期优化)

1. **动画效果**
   - 按钮点击动画
   - 页面过渡效果
   - 列表项悬停动画

2. **高级视觉效果**
   - Acrylic 材质背景
   - Reveal 高亮效果
   - Elevation 阴影系统

3. **可访问性**
   - 键盘导航优化
   - 焦点环样式
   - 高对比度模式

---

**设计文档版本**: 1.0
**适用系统**: Windows 10/11
**设计规范**: Microsoft Fluent Design System
**最后更新**: 2026-03-03
