# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Build/Test Commands

- **Build solution**: `dotnet build WorkLogApp.sln`
- **Run tests**: `dotnet test WorkLogApp.Tests/WorkLogApp.Tests.csproj`
- **Run single test**: `dotnet test --filter "FullyQualifiedName~TestMethodName"`

## Project-Specific Patterns

See mode-specific files for detailed rules:
- `.roo/rules-code/AGENTS.md` - Coding rules
- `.roo/rules-debug/AGENTS.md` - Debugging rules
- `.roo/rules-ask/AGENTS.md` - Documentation context
- `.roo/rules-architect/AGENTS.md` - Architecture rules

## Critical Gotchas

- **SimpleInjector verification disabled** - `EnableAutoVerification = false` in [`Program.ConfigureServices()`](WorkLogApp.UI/Program.cs:129), do not enable it
- **Config files are embedded resources** - Extracted via [`ResourceManager.ExtractResourceIfNotExists()`](WorkLogApp.UI/Helpers/ResourceManager.cs:34) only if missing (preserves user edits)
- **Category.Children not persisted** - `[JsonIgnore]` attribute on [`Category.Children`](WorkLogApp.Core/Models/Category.cs:15) means tree structure is runtime-only
- **Excel export behavior** - [`ExportMonth()`](WorkLogApp.Services/Implementations/ImportExportService.cs) merges, [`RewriteMonth()`](WorkLogApp.Services/Implementations/ImportExportService.cs) overwrites completely
