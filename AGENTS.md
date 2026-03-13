# AGENTS.md - v2rayN Project Guide

This document provides essential information for AI coding agents working in the v2rayN repository.

## Project Overview

v2rayN is a Windows Forms application for V2Ray/Xray proxy client management, built on .NET Framework 4.8.

## Build Commands

### Prerequisites
- Visual Studio 2015+ or MSBuild
- .NET Framework 4.8 SDK
- NuGet for package restoration

### Build Solution
```bash
# Restore NuGet packages
nuget restore v2rayN/v2rayN.sln

# Build (Debug)
msbuild v2rayN/v2rayN.sln /p:Configuration=Debug

# Build (Release)
msbuild v2rayN/v2rayN.sln /p:Configuration=Release

# Build specific project only
msbuild v2rayN/v2rayN/v2rayN.csproj /p:Configuration=Release
```

### Build with Visual Studio
1. Open `v2rayN/v2rayN.sln`
2. Select configuration (Debug/Release)
3. Build → Build Solution (Ctrl+Shift+B)

## Testing

**No automated tests exist in this project.** Testing is manual:
- Run the application: `v2rayN/bin/Debug/net48/v2rayN.exe` (or Release)
- Verify proxy functionality manually
- Check log output in application logs

## Project Structure

```
v2rayN/
├── v2rayN.sln              # Visual Studio solution
├── v2rayN/                  # Main application
│   ├── Base/               # Base classes (HttpClientHelper, extensions)
│   ├── Forms/              # Windows Forms UI (30+ forms)
│   ├── Handler/            # Business logic handlers
│   ├── Mode/               # Data models and enums
│   ├── Protos/             # Protocol Buffer definitions
│   ├── Resx/               # Localization resources
│   ├── Sample/             # Sample configuration files
│   ├── Tool/               # Utility classes (Utils, Logging, UI)
│   ├── Global.cs           # Global constants and variables
│   ├── Program.cs          # Application entry point
│   └── v2rayN.csproj       # Project file
└── v2rayUpgrade/           # Updater utility project
```

## Code Style Guidelines

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `ConfigHandler`, `V2rayHandler` |
| Methods | PascalCase | `LoadConfig()`, `SaveConfig()` |
| Properties | PascalCase | `logEnabled`, `loglevel` |
| Fields (private) | camelCase | `configRes`, `objLock` |
| Local variables | camelCase | `result`, `inItem` |
| Constants | PascalCase | `ConfigFileName`, `DefaultSecurity` |
| Enums | PascalCase + 'E' prefix | `EConfigType`, `ECoreType`, `ESysProxyType` |
| Namespaces | v2rayN.{Module} | `v2rayN.Handler`, `v2rayN.Mode` |

### Import Organization

```csharp
// 1. System namespaces first (alphabetically)
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

// 2. Third-party libraries
using Newtonsoft.Json;
using log4net;

// 3. Project-specific namespaces (alphabetically)
using v2rayN.Base;
using v2rayN.Handler;
using v2rayN.Mode;
using v2rayN.Tool;
```

### Code Organization

```csharp
namespace v2rayN.Handler
{
    /// <summary>
    /// XML documentation comments (Chinese preferred for this project)
    /// </summary>
    class ConfigHandler
    {
        #region Fields
        private static string configRes = Global.ConfigFileName;
        private static readonly object objLock = new object();
        #endregion

        #region Public Methods
        /// <summary>
        /// 方法说明
        /// </summary>
        public static int LoadConfig(ref Config config)
        {
            // Implementation
        }
        #endregion

        #region Private Methods
        // ...
        #endregion
    }
}
```

### Error Handling Pattern

```csharp
// Standard error handling with logging
public static int SomeMethod()
{
    try
    {
        // Operation
        return 0; // Success
    }
    catch (Exception ex)
    {
        Utils.SaveLog("SomeMethod", ex);
        return -1; // Failure
    }
}

// Silent catch for non-critical UI operations
try
{
    // Non-critical operation
}
catch { } // Acceptable in UI contexts

// Null-safe operations
if (!Utils.IsNullOrEmpty(result))
{
    // Process result
}
```

### Return Codes

Methods typically return `int`:
- `0` = Success
- `-1` = Failure

### Singleton Pattern

```csharp
// Lazy singleton implementation
public class LazyConfig
{
    private static Lazy<LazyConfig> instance = new Lazy<LazyConfig>(() => new LazyConfig());
    public static LazyConfig Instance => instance.Value;
}
```

### Static Handler Classes

Handler classes use static methods:
```csharp
ConfigHandler.LoadConfig(ref config);
V2rayHandler.ReloadV2ray();
Utils.SaveLog("message", ex);
```

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Newtonsoft.Json | 13.0.1 | JSON serialization |
| Google.Protobuf | 3.21.5 | Protocol Buffers |
| Grpc.Core | 2.47.0 | gRPC communication |
| log4net | 2.0.15 | Logging |
| ZXing.Net | 0.16.8 | QR code generation |
| NHotkey | 2.1.0 | Global hotkeys |

## Important Files

- `Global.cs` - Application constants and global state
- `Tool/Utils.cs` - Utility functions (logging, JSON, file I/O)
- `Tool/Logging.cs` - Log4net configuration
- `Handler/ConfigHandler.cs` - Configuration management
- `Handler/V2rayHandler.cs` - V2Ray process management
- `Mode/Config.cs` - Main configuration model

## Logging

```csharp
// Log informational message
Utils.SaveLog($"v2rayN start up | {Utils.GetVersion()}");

// Log exception
Utils.SaveLog("MethodName", ex);

// Logging is configured via Logging.Setup() in Program.cs
```

## Localization

- Resource files in `Resx/` directory
- Primary language: Chinese (zh-Hans)
- Access via `ResUI.ResourceName`

## Windows Forms Patterns

```csharp
// Form inheritance
public partial class MainForm : BaseForm

// Designer separation
// MainForm.cs - Logic
// MainForm.Designer.cs - UI definition (auto-generated)

// Show dialog
using (var form = new OptionSettingForm())
{
    if (form.ShowDialog() == DialogResult.OK)
    {
        // Handle result
    }
}
```

## Notes for Agents

1. **Language**: Code comments and documentation are primarily in Chinese
2. **No Tests**: Manual testing required; no unit test infrastructure
3. **Legacy Framework**: Uses .NET Framework 4.8 (not .NET Core/.NET 5+)
4. **Windows Only**: Windows Forms application, Windows-specific APIs used
5. **Static Architecture**: Heavy use of static classes and methods
6. **Configuration**: JSON-based config stored in `guiNConfig.json`
