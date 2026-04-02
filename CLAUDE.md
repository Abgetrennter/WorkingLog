# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Restore NuGet packages
nuget restore WorkLogApp.sln

# Build (Release)
msbuild WorkLogApp.sln /p:Configuration=Release /p:Platform="Any CPU"

# Build (Debug)
msbuild WorkLogApp.sln /p:Configuration=Debug /p:Platform="Any CPU"

# Run tests
vstest WorkLogApp.Tests\bin\Release\WorkLogApp.Tests.dll
```

CI runs via GitHub Actions on Windows (`build.yml`) — triggers on push/PR to `main` and `develop`.

## Architecture

This is a .NET Framework 4.7.2 WinForms desktop app for managing daily work logs. It uses a layered architecture with SimpleInjector DI.

**Three projects:**
- **WorkLogApp.Core** — Domain models (`WorkLog`, `WorkLogItem`, `Category`, `WorkTemplate`, `TemplateField`), enums, constants, and `Logger`
- **WorkLogApp.Services** — Business logic behind interfaces. Key services: `ITemplateService` (CQRS — inherits `ITemplateQueryService` + `ITemplateCommandService`), `IPdfExportService`, `IWordExportService`, `IImportExportService`
- **WorkLogApp.UI** — WinForms entry point (`Program.cs`). Contains forms, custom controls, Fluent-style UI theming (`UIStyleManager`, `FluentColors`, `FluentTypography`), and `ResourceManager` for extracting embedded resources at runtime

**WorkLogApp.Tests** — xUnit test project

## Key Patterns

- **Embedded resources**: Config files (`dev.config.json`, `prod.config.json`) and templates (`templates.json`) are embedded in the assembly and extracted to disk on startup via `ResourceManager`
- **Template-driven forms**: `DynamicFormPanel` generates UI from `TemplateField` definitions (types: text, textarea, select, checkbox, datetime, number). Placeholders use `{key}`, `{key:prefix}`, `{key|default:value}` syntax
- **Category hierarchy**: Tree structure with template inheritance — child categories inherit parent templates, merged via dash-separated naming
- **File-based storage**: Monthly Excel files (`Data/工作日志_yyyyMM.xlsx`) using NPOI. No database

## DI Registration (Program.cs)

Services are registered as Singleton. Forms are registered as Transient with factory delegates that resolve dependencies. `DailySummaryForm` is created directly in `MainForm` (not registered in the container).

## Dependencies

- **SimpleInjector 5.5** — DI container
- **NPOI 2.6** — Excel read/write (no Office dependency)
- **PdfSharp** — PDF export
- **DocX** — Word export
- **Newtonsoft.Json** — JSON serialization
- **Costura.Fody** — Embeds DLLs as resources for single-file deployment

## Language

UI text, comments, and template content are in Chinese (zh-CN).
