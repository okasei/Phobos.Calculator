using Phobos.Shared.Class;
using Phobos.Shared.Interface;
using Phobos.Shared.Manager;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;

namespace Phobos.Calculator
{
    /// <summary>
    /// Phobos 计算器插件
    /// </summary>
    public class Calculator : PCPluginBase
    {
        #region 元数据定义

        private static readonly PluginMetadata _metadata = new()
        {
            PackageName = "com.phobos.calculator",
            Name = "Calculator",
            Manufacturer = "Phobos Team",
            Version = "1.0.0",
            Secret = "phobos_calculator_sercet_jf092la0do1g",
            DatabaseKey = "PCal",
            Description = "A scientific calculator with basic and advanced functions",
            Icon = "Assets/icon.png",
            Entry = "show()",
            MinPhobosVersion = "1.0.0",
            Dependencies = new List<PluginDependency>(),
            LocalizedNames = new Dictionary<string, string>
            {
                { "en-US", "Calculator" },
                { "zh-CN", "计算器" },
                { "zh-TW", "計算機" },
                { "ja-JP", "電卓" }
            },
            LocalizedDescriptions = new Dictionary<string, string>
            {
                { "en-US", "A scientific calculator with basic and advanced functions" },
                { "zh-CN", "支持基本运算和科学计算的计算器" },
                { "zh-TW", "支援基本運算和科學計算的計算機" },
                { "ja-JP", "基本演算と科学計算をサポートする電卓" }
            },
            // 插件文件列表 - 安装时复制，卸载时删除
            FileList = new List<PluginFileInfo>
            {
                new PluginFileInfo
                {
                    RelativePath = "Phobos.Calculator.dll",
                    FileType = PluginFileType.MainAssembly,
                    IsMainAssembly = true,
                    IsRequired = true
                },
                new PluginFileInfo
                {
                    RelativePath = "Assets/icon.png",
                    FileType = PluginFileType.Resource,
                    IsRequired = false
                }
            }
        };

        public override PluginMetadata Metadata => _metadata;

        #endregion

        #region UI 组件

        private CalculatorUI? _calculatorUI;
        private readonly PluginResourceManager _resourceManager = new();

        public override FrameworkElement? ContentArea => _calculatorUI;

        #endregion

        #region 生命周期方法

        public override async Task<RequestResult> OnInstall(params object[] args)
        {
            try
            {
                LogInfo("Installing Calculator plugin...");

                // 注册协议
                await Link(new LinkAssociation
                {
                    Protocol = "calc",
                    Name = "Calculator.Evaluate",
                    Description = "Evaluate expression with Calculator",
                    Command = "Run:evaluate:%0",
                    LocalizedDescriptions = new Dictionary<string, string>
                    {
                        { "en-US", "Evaluate expression with Calculator" },
                        { "zh-CN", "使用计算器计算表达式" }
                    }
                });

                // 保存默认设置
                await WriteConfig("AngleMode", "Deg"); // Deg, Rad, Grad
                await WriteConfig("Precision", "10");

                LogInfo("Calculator plugin installed successfully");

                return new RequestResult
                {
                    Success = true,
                    Message = "Calculator installed successfully"
                };
            }
            catch (Exception ex)
            {
                LogError("Failed to install Calculator plugin", ex);
                return new RequestResult
                {
                    Success = false,
                    Message = $"Installation failed: {ex.Message}",
                    Error = ex
                };
            }
        }

        public override async Task<RequestResult> OnLaunch(params object[] args)
        {
            try
            {
                LogDebug("Launching Calculator plugin...");

                // 创建计算器 UI
                _calculatorUI = new CalculatorUI(this);

                // 加载设置
                var angleModeResult = await ReadConfig("AngleMode");
                if (angleModeResult.Success)
                {
                    _calculatorUI.SetAngleMode(angleModeResult.Value);
                    LogDebug($"Loaded AngleMode: {angleModeResult.Value}");
                }

                var precisionResult = await ReadConfig("Precision");
                if (precisionResult.Success && int.TryParse(precisionResult.Value, out var precision))
                {
                    _calculatorUI.SetPrecision(precision);
                    LogDebug($"Loaded Precision: {precision}");
                }

                // 订阅主题变更事件
                await Subscribe(PhobosEventIds.Theme, PhobosEventNames.ThemeChanged);

                LogInfo("Calculator plugin launched");

                return new RequestResult
                {
                    Success = true,
                    Message = "Calculator launched"
                };
            }
            catch (Exception ex)
            {
                LogError("Failed to launch Calculator plugin", ex);
                return new RequestResult
                {
                    Success = false,
                    Message = $"Launch failed: {ex.Message}",
                    Error = ex
                };
            }
        }

        public override async Task<RequestResult> OnClosing(params object[] args)
        {
            LogDebug("Closing Calculator plugin...");

            // 保存设置
            if (_calculatorUI != null)
            {
                await WriteConfig("AngleMode", _calculatorUI.GetAngleMode());
                await WriteConfig("Precision", _calculatorUI.GetPrecision().ToString());
                LogDebug("Settings saved");
            }

            LogInfo("Calculator plugin closed");

            return await base.OnClosing(args);
        }

        public override async Task OnEventReceived(string eventId, string eventName, params object[] args)
        {
            await base.OnEventReceived(eventId, eventName, args);

            LogDebug($"Event received: {eventId}.{eventName}");

            if (eventId == PhobosEventIds.Theme && eventName == PhobosEventNames.ThemeChanged)
            {
                // 主题变更，可以在这里刷新 UI
                LogInfo("Theme changed, refreshing UI...");
                _calculatorUI?.RefreshTheme();
            }
        }

        #endregion

        #region 命令处理

        public override async Task<RequestResult> Run(params object[] args)
        {
            LogInfo("Welcome to Phobos.Calculator!");
            if (args.Length == 0)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "No command specified"
                };
            }

            var command = args[0]?.ToString()?.ToLowerInvariant() ?? string.Empty;
            var commandArgs = args.Length > 1 ? args[1..] : Array.Empty<object>();

            return command switch
            {
                "evaluate" => await HandleEvaluate(commandArgs),
                "add" => await HandleBinaryOp(commandArgs, (a, b) => a + b),
                "subtract" => await HandleBinaryOp(commandArgs, (a, b) => a - b),
                "multiply" => await HandleBinaryOp(commandArgs, (a, b) => a * b),
                "divide" => await HandleBinaryOp(commandArgs, (a, b) => a / b),
                "power" => await HandleBinaryOp(commandArgs, Math.Pow),
                "sqrt" => await HandleUnaryOp(commandArgs, Math.Sqrt),
                "sin" => await HandleTrigOp(commandArgs, Math.Sin),
                "cos" => await HandleTrigOp(commandArgs, Math.Cos),
                "tan" => await HandleTrigOp(commandArgs, Math.Tan),
                "log" => await HandleUnaryOp(commandArgs, Math.Log10),
                "ln" => await HandleUnaryOp(commandArgs, Math.Log),
                "exp" => await HandleUnaryOp(commandArgs, Math.Exp),
                "show" => await OpenCalculatorWindow(),
                _ => new RequestResult
                {
                    Success = false,
                    Message = $"Unknown command: {command}"
                }
            };
        }

        private async Task<RequestResult> HandleEvaluate(object[] args)
        {
            if (args.Length == 0)
            {
                return new RequestResult { Success = false, Message = "No expression provided" };
            }

            var expression = args[0]?.ToString() ?? string.Empty;

            try
            {
                var engine = new CalculatorEngine();
                engine.AngleMode = _calculatorUI?.GetAngleMode() ?? "Deg";

                var result = engine.Evaluate(expression);

                return await Task.FromResult(new RequestResult
                {
                    Success = true,
                    Message = result.ToString(),
                    Data = new List<object> { result }
                });
            }
            catch (Exception ex)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private async Task<RequestResult> HandleBinaryOp(object[] args, Func<double, double, double> op)
        {
            if (args.Length < 2)
            {
                return new RequestResult { Success = false, Message = "Need two numbers" };
            }

            if (double.TryParse(args[0]?.ToString(), out var a) &&
                double.TryParse(args[1]?.ToString(), out var b))
            {
                var result = op(a, b);
                return await Task.FromResult(new RequestResult
                {
                    Success = true,
                    Message = result.ToString(),
                    Data = new List<object> { result }
                });
            }

            return new RequestResult { Success = false, Message = "Invalid numbers" };
        }

        private async Task<RequestResult> HandleUnaryOp(object[] args, Func<double, double> op)
        {
            if (args.Length == 0)
            {
                return new RequestResult { Success = false, Message = "Need a number" };
            }

            if (double.TryParse(args[0]?.ToString(), out var a))
            {
                var result = op(a);
                return await Task.FromResult(new RequestResult
                {
                    Success = true,
                    Message = result.ToString(),
                    Data = new List<object> { result }
                });
            }

            return new RequestResult { Success = false, Message = "Invalid number" };
        }

        private async Task<RequestResult> HandleTrigOp(object[] args, Func<double, double> op)
        {
            if (args.Length == 0)
            {
                return new RequestResult { Success = false, Message = "Need a number" };
            }

            if (double.TryParse(args[0]?.ToString(), out var a))
            {
                var angleMode = _calculatorUI?.GetAngleMode() ?? "Deg";
                var radians = angleMode switch
                {
                    "Rad" => a,
                    "Grad" => a * Math.PI / 200,
                    _ => a * Math.PI / 180 // Deg
                };

                var result = op(radians);
                return await Task.FromResult(new RequestResult
                {
                    Success = true,
                    Message = result.ToString(),
                    Data = new List<object> { result }
                });
            }

            return new RequestResult { Success = false, Message = "Invalid number" };
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 打开独立计算器窗口
        /// </summary>
        public async Task<RequestResult> OpenCalculatorWindow()
        {
            Dictionary_Initialize();
            var window = new CalculatorWindow(this);
            _resourceManager.ApplyToWindow(window);
            window.InitializeComponent();
            window.Show();
            return new RequestResult { Success = true, Message = "Opening Calculator Window" };
        }

        /// <summary>
        /// 获取主题资源
        /// </summary>
        public ResourceDictionary? GetThemeResources()
        {
            return GetMergedDictionaries();
        }

        public void Dictionary_Initialize()
        {
            var hostTheme = GetMergedDictionaries();
            _resourceManager.SetHostTheme(hostTheme);

            // 调试
            _resourceManager.DebugPrint();
        }
        #endregion
    }
}