# Phobos 插件接口速查表

## 生命周期方法

| 方法 | 触发时机 | 用途 |
|-----|---------|------|
| `OnInstall` | 首次安装 | 注册协议、创建默认配置 |
| `OnLaunch` | 每次打开 | 初始化 UI、加载配置 |
| `OnClosing` | 窗口关闭 | 保存状态、释放资源 |
| `OnUninstall` | 用户卸载 | 清理数据、注销协议 |
| `OnUpdate` | 版本更新 | 数据迁移、配置升级 |
| `Run` | 命令调用 | 处理命令、插件间通信 |

## 配置方法

```csharp
// 本插件配置
Task<ConfigResult> ReadConfig(string key)
Task<ConfigResult> WriteConfig(string key, string value)

// 其他插件配置（需授权）
Task<ConfigResult> ReadConfig(string key, string targetPackageName)
Task<ConfigResult> WriteConfig(string key, string value, string targetPackageName)

// 系统配置
Task<ConfigResult> ReadSysConfig(string key)
Task<ConfigResult> WriteSysConfig(string key, string value)
```

### ConfigResult 结构

```csharp
class ConfigResult {
    bool Success;
    string Key;
    string Value;
    string Message;
}
```

## 协议方法

```csharp
// 注册协议
Task<RequestResult> Link(LinkAssociation association)

// 设置默认打开方式
Task<RequestResult> LinkDefault(string protocol)
```

### LinkAssociation 结构

```csharp
class LinkAssociation {
    string Protocol;      // 协议名（如 "demo"）
    string Name;          // 处理器名（如 "MyPlugin.Open"）
    string Description;   // 描述
    string Command;       // 命令（%1=完整链接, %0=无协议头）
    Dictionary<string, string> LocalizedDescriptions;
}
```

## 启动项方法

```csharp
// 注册启动项
Task<BootResult> BootWithPhobos(string command, int priority = 100, params object[] args)

// 移除启动项
Task<BootResult> RemoveBootWithPhobos(string? uuid = null)

// 获取启动项列表
Task<List<object>> GetBootItems()
```

### BootResult 结构

```csharp
class BootResult {
    bool Success;
    string UUID;
    string Message;
}
```

## 事件订阅方法

```csharp
// 订阅事件
Task<RequestResult> Subscribe(string eventId, string eventName, params object[] args)

// 取消订阅
Task<RequestResult> Unsubscribe(string eventId, string eventName, params object[] args)

// 事件接收回调（重写此方法处理事件）
Task OnEventReceived(string eventId, string eventName, params object[] args)
```

### 预定义事件 ID (PhobosEventIds)

| 常量 | 值 | 说明 |
|-----|---|-----|
| `Theme` | "Theme" | 主题相关事件 |
| `Language` | "Language" | 语言相关事件 |
| `Plugin` | "Plugin" | 插件相关事件 |
| `System` | "System" | 系统相关事件 |
| `Window` | "Window" | 窗口相关事件 |

### 预定义事件名称 (PhobosEventNames)

| 事件 ID | 事件名称 | 说明 |
|--------|---------|-----|
| Theme | `ThemeChanged` | 主题已变更 |
| Theme | `ThemeLoaded` | 主题已加载 |
| Language | `LanguageChanged` | 语言已变更 |
| Plugin | `PluginInstalled` | 插件已安装 |
| Plugin | `PluginUninstalled` | 插件已卸载 |
| Plugin | `PluginEnabled` | 插件已启用 |
| Plugin | `PluginDisabled` | 插件已禁用 |
| System | `SystemShutdown` | 系统关闭 |
| System | `SystemSuspend` | 系统休眠 |
| System | `SystemResume` | 系统恢复 |
| Window | `WindowActivated` | 窗口激活 |
| Window | `WindowDeactivated` | 窗口失焦 |
| Window | `WindowMinimized` | 窗口最小化 |
| Window | `WindowRestored` | 窗口恢复 |

### 订阅示例

```csharp
// 订阅主题变更
await Subscribe(PhobosEventIds.Theme, PhobosEventNames.ThemeChanged);

// 处理事件
public override async Task OnEventReceived(string eventId, string eventName, params object[] args)
{
    if (eventId == PhobosEventIds.Theme && eventName == PhobosEventNames.ThemeChanged)
    {
        var newThemeId = args.Length > 0 ? args[0]?.ToString() : null;
        // 刷新 UI...
    }
}
```

## 主题获取方法

```csharp
// 获取主程序主题资源字典
ResourceDictionary? GetMergedDictionaries()

// 辅助方法：应用主题到窗口（基类提供）
protected void ApplyThemeToWindow(Window window)
```

### 自定义窗口应用主题示例

```csharp
var window = new Window { Title = "My Window" };

// 应用主程序主题
ApplyThemeToWindow(window);

window.Show();
```

## Logger 方法

```csharp
// 调试日志
void LogDebug(string message, params object[] args)

// 信息日志
void LogInfo(string message, params object[] args)

// 警告日志
void LogWarning(string message, params object[] args)

// 错误日志
void LogError(string message, Exception? exception = null, params object[] args)

// 严重错误日志
void LogFatal(string message, Exception? exception = null, params object[] args)
```

### LogLevel 枚举

| 级别 | 值 | 说明 |
|-----|---|-----|
| `Debug` | 0 | 调试信息 |
| `Info` | 1 | 一般信息 |
| `Warning` | 2 | 警告信息 |
| `Error` | 3 | 错误信息 |
| `Fatal` | 4 | 严重错误 |

### Logger 使用示例

```csharp
// 记录调试信息
LogDebug("Loading config for key: {0}", key);

// 记录一般信息
LogInfo("Plugin initialized successfully");

// 记录警告
LogWarning("Config file not found, using defaults");

// 记录错误
try
{
    // ...
}
catch (Exception ex)
{
    LogError("Failed to process request", ex);
}

// 记录严重错误
LogFatal("Critical system failure", exception);
```

### LogEntry 结构

```csharp
class LogEntry {
    DateTime Timestamp;
    LogLevel Level;
    string Source;       // 插件包名
    string Message;
    Exception? Exception;
    object[]? Args;
}
```

## 请求方法

```csharp
// 向主程序请求信息
Task<List<object>> RequestPhobos(params object[] args)

// 向主程序发送请求
Task<RequestResult> Request(string command, Action<RequestResult>? callback, params object[] args)
```

### RequestPhobos 可用参数

| 参数 | 返回值 |
|-----|-------|
| `"username"` | 当前用户名 |
| `"machinename"` | 计算机名 |
| `"currentdirectory"` | 当前目录 |
| `"pluginsdirectory"` | 插件目录 |
| `"caller"` | [包名, DatabaseKey] |

## RequestResult 结构

```csharp
class RequestResult {
    bool Success;
    string Message;
    List<object> Data;
    Exception? Error;
}
```

## PluginMetadata 结构

```csharp
class PluginMetadata {
    string PackageName;       // 必填：包名（全局唯一）
    string Name;              // 必填：名称
    string Manufacturer;      // 必填：开发者
    string Version;           // 必填：版本号
    string Secret;            // 可选：密钥
    string DatabaseKey;       // 可选：数据库键前缀
    List<PluginDependency> Dependencies;
    Dictionary<string, string> LocalizedNames;
    Dictionary<string, string> LocalizedDescriptions;
}
```

## PluginDependency 结构

```csharp
class PluginDependency {
    string PackageName;  // 依赖的插件包名
    string MinVersion;   // 最低版本
    bool IsOptional;     // 是否可选
}
```

## Run 方法标准命令

| 命令 | 参数 | 说明 |
|-----|------|-----|
| `boot` | `[command]` | 启动项执行 |
| `pluginmessage` | `[sourcePlugin, message, ...args]` | 插件间消息 |
| `protocol` | `[url]` | 协议处理 |

## 系统配置键

| 键 | 说明 | 示例值 |
|---|------|-------|
| `Theme` | 主题 | `"dark"`, `"light"` |
| `Language` | 语言 | `"en-US"`, `"zh-CN"` |
| `StartupPlugin` | 启动插件 | `"com.phobos.plugin.manager"` |
| `Initialized` | 初始化标记 | `"true"` |

## 支持的语言代码

| 代码 | 语言 |
|-----|------|
| `en-US` | English |
| `zh-CN` | 简体中文 |
| `zh-TW` | 繁體中文 |
| `ja-JP` | 日本語 |
| `ko-KR` | 한국어 |