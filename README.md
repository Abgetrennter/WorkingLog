# 工作日志桌面应用 (WorkLog Desktop App)

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

**工作日志桌面应用** 是一款专为开发者和职场人士设计的轻量级日志管理工具。它结合了**模板化生成**与**Excel 结构化存储**的优势，帮助用户高效记录、管理和回顾每日工作内容，彻底告别散乱的文本记录和繁琐的手动表格维护。

---

## ✨ 核心特性

- **📝 模板驱动记录**：通过预设模板（支持文本、下拉框、日期等）快速生成标准化日志。
- **📊 Excel 无感同步**：数据自动按月存储为标准 `.xlsx` 文件，无需安装 Office，数据完全掌控。
- **🌲 多级分类管理**：支持 "父-子-孙" 级联分类，子分类自动继承模板，灵活适应复杂业务。
- **📅 智能视图切换**：
  - **日视图**：专注当日事项，支持拖拽排序。
  - **月视图**：概览整月工作，自动按日期聚合。
- **📝 每日总结生成**：一键提取当日事项标题生成日报草稿。
- **🔄 历史数据迁移**：内置强大的导入向导，支持从外部 Excel 批量迁移数据。
- **🎨 现代化 UI**：支持 High DPI 缩放，清晰舒适的阅读体验。

---

## � 下载与安装

### 环境要求
- **操作系统**：Windows 7 SP1 / Windows 10 / Windows 11
- **运行环境**：[.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net472) (Win10/11 通常自带)

### 安装步骤
1. 访问 [Releases](../../releases) 页面下载最新版本的压缩包 `WorkLogApp_vX.X.zip`。
2. 解压到任意目录（推荐 `D:\Tools\WorkLogApp`，避免放在 C 盘受权限限制）。
3. 双击 `WorkLogApp.UI.exe` 即可启动。
   > **注意**：首次运行会自动在根目录创建 `Data`（数据）和 `Templates`（模板）文件夹。

---

## 📖 详细使用指南

### 1. 主界面概览
界面主要分为三个区域：
- **顶部工具栏**：包含创建、视图切换、分类管理、导入向导等核心功能。
- **左侧日历/列表**：
  - **日历模式**：点击日期查看当日日志。
  - **月份选择器**：在月视图下切换月份。
- **中央内容区**：以列表形式展示日志条目，支持富文本预览。

### 2. 创建第一条日志
1. 点击左上角的 **➕ 创建事项** 按钮。
2. 在弹出的窗口中：
   - **选择分类**：从树形下拉框中选择（如 `研发-后端`）。
   - **填写表单**：根据所选分类，界面会自动生成对应的输入框（如 `模块`、`进度`）。
   - **调整时间**：默认填充当前时间，可手动修改开始/结束时间。
3. 点击 **确定**，日志即刻保存并显示在列表中。

### 3. 日志管理与编辑
- **快速编辑**：双击列表中的任意条目，进入纯文本编辑模式，可直接修改内容并保存。
- **拖拽排序**：
  - 切换到 **日视图**（取消勾选“按月显示”）。
  - 按住列表项拖动，调整其在当日的顺序。
  - 点击 **💾 保存** 按钮，顺序将写入 Excel。
- **每日总结**：
  - 在日视图下点击 **📝 每日总结**。
  - 系统会自动聚合当日所有事项的标题作为草稿。
  - 修改确认后，总结将作为特殊行（灰色背景）追加到当日数据末尾。

### 4. 模板配置详解 (高级)
本应用最强大的功能是**自定义模板**。点击 **📂 分类管理** 进行配置。

#### 4.1 界面操作
- **新增分类**：选中父节点（或根节点），点击“新增”。
- **模板编辑**：在右侧编辑区配置生成格式和表单字段。

#### 4.2 模板 JSON 原理
配置保存在 `Templates/templates.json` 中。以下是一个“会议记录”的配置示例：

```json
"会议": {
  "CategoryTemplate": {
    "FormatTemplate": "【会议】{Theme}\r\n参与人：{Attendees}\r\n结论：{Conclusion}",
    "Placeholders": {
      "Theme": "text",       // 单行文本框
      "Attendees": "text",
      "Conclusion": "textarea" // 多行文本域
    }
  }
}
```

#### 4.3 常用占位符类型
| 类型 | 说明 | 示例 |
| :--- | :--- | :--- |
| `text` | 单行文本框 | 任务名称 |
| `textarea` | 多行文本域 | 详细描述 |
| `select` | 下拉选择框 | 需配合 `Options` 字段定义选项 |
| `datetime` | 日期时间选择 | 截止时间 |
| `checkbox` | 复选框 | 是否紧急 |

### 5. 数据导出与查看
- **自动保存**：所有操作实时写入 `Data/worklog_yyyyMM.xlsx`。
- **一键打开**：点击主界面的 **📂 打开EXCEL** 按钮，直接定位到当月文件。
- **Excel 结构**：
  - 文件按月分表（如 `worklog_202501.xlsx`）。
  - 包含列：`日期`、`标题`、`内容`、`分类ID`、`开始时间`、`结束时间`、`标签`、`排序`。
  - **日期分隔行**：系统会自动插入带背景色的日期行，便于阅读。

---

## 🛠️ 开发与构建

如果您希望自行构建或贡献代码：

### 项目结构
```text
WorkingLog/
├── WorkLogApp.Core/       # 领域模型、枚举
├── WorkLogApp.Services/   # 业务逻辑 (Excel处理、模板引擎)
├── WorkLogApp.UI/         # WinForms 界面实现
├── Templates/             # 默认配置文件
└── Wiki/                  # 详细技术文档
```

### 构建命令
```powershell
# 1. 克隆仓库
git clone https://github.com/your-repo/WorkingLog.git

# 2. 还原依赖
nuget restore WorkLogApp.sln

# 3. 构建发布版
msbuild WorkLogApp.sln /p:Configuration=Release
```

---

## 📚 更多文档

- [技术架构设计](Wiki/1-技术架构.md)
- [API 接口规范](Wiki/2-API接口.md)
- [数据库与存储设计](Wiki/3-数据库设计.md)
- [常见问题 (FAQ)](Wiki/6-常见问题.md)

---

## 📄 许可证

本项目采用 [MIT License](LICENSE) 开源，欢迎自由使用和修改。
