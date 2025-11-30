# Phobos Calculator Plugin

一个为 Phobos 设计的科学计算器插件，支持基本运算和高级数学函数。

## ✨ 功能特性

### 基本运算
- 加减乘除 (`+`, `-`, `×`, `÷`)
- 取模 (`%`)
- 幂运算 (`xʸ`, `x²`)
- 平方根 (`√`)

### 三角函数
- 正弦 / 余弦 / 正切 (`sin`, `cos`, `tan`)
- 反三角函数 (`asin`, `acos`, `atan`)
- 双曲函数 (`sinh`, `cosh`, `tanh`)
- 反双曲函数 (`asinh`, `acosh`, `atanh`)

### 对数和指数
- 常用对数 (`log` / `log10`)
- 自然对数 (`ln`)
- 以 2 为底的对数 (`log2`)
- 指数函数 (`exp`)

### 其他功能
- 阶乘 (`n!`)
- 内存功能 (`MC`, `MR`, `M+`, `M-`)
- 角度模式切换 (`Deg`, `Rad`, `Grad`)
- 数学常量 (`π`, `e`)
- 上一结果引用 (`Ans`)
- 独立窗口模式

## 📁 项目结构

```
PhobosDemo/
├── PhobosDemo.csproj           # 项目文件
├── CalculatorPlugin.cs         # 插件主类
├── CalculatorEngine.cs         # 计算引擎（表达式解析）
├── CalculatorUI.cs             # 嵌入式 UI 控件
├── CalculatorWindow.xaml       # 独立窗口 XAML
├── CalculatorWindow.xaml.cs    # 独立窗口代码
├── README.md                   # 说明文档
├── API_REFERENCE.md            # 接口速查
├── Shared/                     # Phobos 共享文件
│   ├── Interface/
│   │   └── IPhobosPlugin.cs
│   └── Class/
│       └── PCPluginBase.cs
└── Assets/
```

## 🔧 表达式语法

计算器支持完整的数学表达式输入：

```
// 基本运算
2 + 3 * 4           = 14
(2 + 3) * 4         = 20
2 ^ 10              = 1024
10 % 3              = 1

// 函数
sin(30)             = 0.5 (Deg 模式)
sqrt(16)            = 4
log(100)            = 2
ln(e)               = 1
fact(5)             = 120

// 常量
pi * 2              = 6.283...
e ^ 2               = 7.389...

// 组合表达式
sin(45)^2 + cos(45)^2 = 1
log(10^5)           = 5
```

## 🎨 主题支持

插件完全支持 Phobos 主题系统：

```csharp
// 获取主程序主题
var theme = GetMergedDictionaries();

// 应用到自定义窗口
ApplyThemeToWindow(myWindow);
```

独立窗口会自动应用主程序的主题配色。

## 📡 事件订阅

插件自动订阅主题变更事件，当主程序主题改变时会自动刷新 UI：

```csharp
// 已自动订阅
await Subscribe(PhobosEventIds.Theme, PhobosEventNames.ThemeChanged);

// 事件处理
public override async Task OnEventReceived(string eventId, string eventName, params object[] args)
{
    if (eventId == PhobosEventIds.Theme && eventName == PhobosEventNames.ThemeChanged)
    {
        _calculatorUI?.RefreshTheme();
    }
}
```

## 🔌 协议支持

插件注册了 `calc://` 协议：

```
calc://2+3*4          → 计算表达式
calc://sin(45)        → 计算三角函数
```

## 💻 命令接口

其他插件可以通过 `Run` 方法调用计算器功能：

```csharp
// 计算表达式
var result = await calculator.Run("evaluate", "2+3*4");

// 直接调用运算
await calculator.Run("add", 10, 20);       // 30
await calculator.Run("multiply", 5, 6);    // 30
await calculator.Run("sqrt", 16);          // 4
await calculator.Run("sin", 30);           // 0.5 (Deg)
await calculator.Run("log", 100);          // 2
```

## ⌨️ 快捷键

| 快捷键 | 功能 |
|-------|-----|
| `Enter` | 计算结果 |
| `Esc` | 清除全部 |
| `Backspace` | 删除字符 |

## 📝 配置

插件会自动保存以下设置：

| 配置项 | 默认值 | 说明 |
|-------|-------|-----|
| `AngleMode` | `Deg` | 角度模式 |
| `Precision` | `10` | 小数精度 |

## 🛠️ 构建

```bash
dotnet build -c Release
```

输出文件: `bin/Release/net10.0-windows/PhobosCalculator.dll`

## 📦 部署

将 DLL 复制到 Phobos 插件目录：

```
%APPDATA%\Phobos\Plugins\PhobosCalculator\PhobosCalculator.dll
```

或启用自动部署（Debug 模式）。