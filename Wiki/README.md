# 工作日志桌面应用 Wiki

- 适用版本：`.NET Framework 4.7.2`，WinForms，Excel 读写使用 `NPOI`
- 目录：
  - [项目概述](./1-项目概述.md)
  - [UI 层（WorkLogApp.UI）](./2-UI层.md)
  - [服务层（WorkLogApp.Services）](./3-服务层.md)
  - [核心层（WorkLogApp.Core）](./4-核心层.md)
  - [技术实现细节](./5-技术实现细节.md)

```text
WorkLogApp/
├── WorkLogApp.UI            # 表示层（WinForms）
├── WorkLogApp.Services      # 业务层（模板、导入导出）
└── WorkLogApp.Core          # 领域层（模型、枚举、模板结构）
```

```text
┌─────────────────────────────────────────────────────────────┐
│                         WorkLogApp.UI                        │
│  MainForm / ItemCreateForm / ItemEditForm / ImportWizard     │
│  CategoryManageForm / Controls / UIStyleManager              │
└───────────────▲───────────────────────────────▲─────────────┘
                │                               │
                │调用接口                        │引用模型
                │                               │
┌───────────────┴───────────────────────────────┴─────────────┐
│                     WorkLogApp.Services                       │
│  ITemplateService / TemplateService                           │
│  IImportExportService / ImportExportService (NPOI)           │
└───────────────▲──────────────────────────────────────────────┘
                │引用模型
┌───────────────┴──────────────────────────────────────────────┐
│                         WorkLogApp.Core                       │
│  WorkLog / WorkLogItem / StatusEnum                           │
│  TemplateRoot / TemplateCategory / CategoryTemplate            │
└───────────────────────────────────────────────────────────────┘
```

建议阅读顺序：从“项目概述”快速了解目标与架构，再按层阅读对应文档，并最后结合“技术实现细节”把握调用关系与关键算法。

