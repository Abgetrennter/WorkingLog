# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Build/Test Commands

- **Build solution**: `dotnet build WorkLogApp.sln` or use Visual Studio
- **Run tests**: `dotnet test WorkLogApp.Tests/WorkLogApp.Tests.csproj`
- **Run single test**: `dotnet test --filter "FullyQualifiedName~TestMethodName"`

## Non-Obvious Project Patterns

### Resource Management
- Config files (dev.config.json, prod.config.json, templates.json) are embedded resources and extracted at runtime via `ResourceManager.ExtractResourceIfNotExists()` (only extracts if file doesn't exist)
- Resources are embedded with namespace `WorkLogApp.UI.Configs.filename` format
- Costura.Fody merges dependency DLLs into the EXE (configured in FodyWeavers.xml)

### Dependency Injection
- SimpleInjector container configured in `Program.ConfigureServices()`
- Services registered as `Lifestyle.Singleton` (ITemplateService, IImportExportService)
- Forms registered as transient with factory methods
- Container verification is disabled (`EnableAutoVerification = false`) to avoid disposable transient warnings

### UI/Design-Time Support
- `UIStyleManager.IsDesignMode` detects design-time vs runtime (checks LicenseManager and process name)
- Forms need parameterless constructors for Visual Studio designer
- `MainForm` has dual constructors: parameterless for design-time, parameterized for DI
- Design-time data is populated in `MainForm` constructor when `IsDesignMode` is true

### Excel Export (ImportExportService)
- Monthly exports group data by weeks into separate sheets
- Sheet naming format: "StartDay-EndDay" (e.g., "1日-5日", "8日-8日")
- `ExportMonth()` merges with existing file; `RewriteMonth()` overwrites completely
- Header mapping supports both Chinese and English column names for backward compatibility

### Template System
- Templates use `Placeholders` (Dictionary<string, string>) for form field types (text, textarea, datetime, select, checkbox)
- `Options` (Dictionary<string, List<string>>) provides dropdown/checkbox choices
- `DynamicFormPanel.BuildForm()` creates controls dynamically based on placeholder types
- `TemplateService` uses lock (_lock) for thread-safe file operations

### Status Handling
- `StatusHelper.ToChinese()` converts enum to Chinese display text
- `StatusHelper.Parse()` accepts Chinese, English, or numeric strings (e.g., "待办", "Todo", "0" all parse to Todo)

### Data Storage
- Runtime data stored in `Data/` directory (configurable via App.config DataPath)
- Logs written to `Logs/` directory (thread_exception.log, unhandled_exception.log, startup_error.log)
- Category.Children property has `[ScriptIgnore]` attribute to exclude from JSON serialization

### Project Structure
- WorkLogApp.UI: WinForms entry point with DI container setup
- WorkLogApp.Core: Domain models, enums, helpers (no dependencies)
- WorkLogApp.Services: Business logic with Newtonsoft.Json and NPOI dependencies
- WorkLogApp.Tests: xUnit tests (LangVersion 10.0, Nullable enabled)

### Gotchas
- SimpleInjector container verification is intentionally disabled - don't enable it
- Config files are only extracted if missing - manual edits persist
- Excel export merges existing data - use RewriteMonth() for complete overwrite
- Category.Children is runtime-only property, not persisted to JSON
