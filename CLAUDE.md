# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

v2rayN is a Windows Forms GUI client for V2Ray/Xray proxy management, built on .NET Framework 4.8 (C# with `LangVersion=preview`). Current version: 5.39.

## Build Commands

```bash
# Restore NuGet packages
nuget restore v2rayN/v2rayN.sln

# Build Debug
msbuild v2rayN/v2rayN.sln /p:Configuration=Debug

# Build Release
msbuild v2rayN/v2rayN.sln /p:Configuration=Release

# Build main project only
msbuild v2rayN/v2rayN/v2rayN.csproj /p:Configuration=Release
```

Or open `v2rayN/v2rayN.sln` in Visual Studio and build with Ctrl+Shift+B.

**No automated tests exist.** Testing is manual — run the built executable from `v2rayN/bin/Debug/net48/v2rayN.exe`.

## Architecture

Two projects in the solution:
- **v2rayN** — Main WinForms application
- **v2rayUpgrade** — Standalone updater that replaces v2rayN binaries from a ZIP file

### Layer Separation

| Layer | Directory | Role |
|-------|-----------|------|
| UI | `Forms/` | 18 WinForms classes inheriting from `BaseForm` |
| Business Logic | `Handler/` | 13 static handler classes for config, core process, networking |
| Data Models | `Mode/` | 20+ model/enum classes, JSON-serialized configuration |
| Utilities | `Tool/` + `Base/` | Logging (log4net), HTTP helpers, file I/O utilities |
| Resources | `Resx/` + `Sample/` | Localization (zh-Hans primary) and embedded sample configs |

### Key Handlers

- **ConfigHandler** — Loads/saves `guiNConfig.json`, manages server list CRUD
- **V2rayHandler** — Starts/stops V2ray/Xray core processes
- **V2rayConfigHandler** — Generates V2ray JSON config for each protocol type
- **LazyConfig** — Singleton config wrapper (`LazyConfig.Instance`)
- **ShareHandler** — Generates/parses protocol share URLs (`vmess://`, `vless://`, `trojan://`, `ss://`)
- **StatisticsHandler** — gRPC-based real-time traffic stats
- **UpdateHandle/DownloadHandle** — GitHub release-based auto-updates
- **SysProxyHandle** — Sets Windows system proxy via Registry

### Data Flow

1. User config stored in `guiNConfig.json` (loaded into `Config` model)
2. `ConfigHandler` manages the config; `LazyConfig` provides singleton access
3. `V2rayConfigHandler` generates V2ray-format `config.json` from user config
4. `V2rayHandler` spawns the core process with that config
5. `StatisticsHandler` connects via gRPC to collect traffic stats

## Conventions

### Naming
- Enums prefixed with `E`: `EConfigType`, `ECoreType`, `ESysProxyType`, `EMove`
- Handler classes are static with static methods
- Namespaces: `v2rayN.{Module}` (e.g., `v2rayN.Handler`, `v2rayN.Mode`)

### Return Codes
Methods return `int`: `0` = success, `-1` = failure.

### Error Handling
```csharp
try { /* ... */ return 0; }
catch (Exception ex) { Utils.SaveLog("MethodName", ex); return -1; }
```
Silent `catch { }` is acceptable for non-critical UI operations.

### Localization
Access strings via `ResUI.ResourceName`. Primary language is Chinese (zh-Hans).

### Comments
Code comments and XML documentation are primarily in Chinese.

## Key Constants (Global.cs)

- Config file: `guiNConfig.json`
- V2ray config: `config.json`
- Default SOCKS port: `10808`
- Supported protocols: VMess, Shadowsocks, SOCKS, VLESS, Trojan
- Supported transports: tcp, kcp, ws, h2, quic, grpc
- Supported cores: v2fly, Xray, SagerNet, clash, clash_meta, hysteria, naiveproxy, tuic, sing_box
- Registry key: `Software\v2rayNGUI`

## Key Dependencies

| Package | Purpose |
|---------|---------|
| Newtonsoft.Json 13.0.1 | JSON serialization |
| Google.Protobuf 3.21.5 | Protocol Buffers for gRPC stats |
| Grpc.Core 2.46.3 | gRPC communication with V2ray core |
| log4net 2.0.15 | Logging |
| ZXing.Net 0.16.8 | QR code generation |
| NHotkey 2.1.0 | Global hotkeys |
