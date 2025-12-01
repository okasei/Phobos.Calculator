using Phobos.Shared.Class;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Phobos.Calculator
{
    /// <summary>
    /// 计算器独立窗口
    /// </summary>
    public partial class CalculatorWindow : Window
    {
        private readonly Calculator _plugin;
        private readonly CalculatorEngine _engine;
        private bool _isNewInput = true;

        public CalculatorWindow(Calculator plugin)
        {
            _plugin = plugin;
            _engine = new CalculatorEngine();
            
            // 设置焦点到显示框
            Loaded += (s, e) => Display.Focus();
        }


        private void AngleModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AngleModeCombo.SelectedItem is ComboBoxItem item)
            {
                _engine.AngleMode = item.Content?.ToString() ?? "Deg";
            }
        }

        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ProcessInput("=");
                    e.Handled = true;
                    break;

                case Key.Escape:
                    ProcessInput("C");
                    e.Handled = true;
                    break;

                case Key.Back:
                    // 允许正常删除
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            var tag = button.Tag?.ToString() ?? button.Content?.ToString() ?? string.Empty;
            ProcessInput(tag);

            // 保持焦点在显示框
            Display.Focus();
            Display.CaretIndex = Display.Text.Length;
        }

        private void ProcessInput(string input)
        {
            switch (input)
            {
                case "=":
                    Calculate();
                    break;

                case "C":
                    Display.Text = "0";
                    HistoryDisplay.Text = string.Empty;
                    _isNewInput = true;
                    break;

                case "CE":
                    Display.Text = "0";
                    _isNewInput = true;
                    break;

                case "NEG":
                    ToggleSign();
                    break;

                case "MC":
                    _engine.Memory = 0;
                    break;

                case "MR":
                    if (_isNewInput)
                    {
                        Display.Text = FormatNumber(_engine.Memory);
                    }
                    else
                    {
                        Display.Text += FormatNumber(_engine.Memory);
                    }
                    _isNewInput = false;
                    break;

                case "M+":
                    if (double.TryParse(Display.Text, out var mAdd))
                        _engine.Memory += mAdd;
                    break;

                case "M-":
                    if (double.TryParse(Display.Text, out var mSub))
                        _engine.Memory -= mSub;
                    break;

                default:
                    AppendInput(input);
                    break;
            }
        }

        private void AppendInput(string input)
        {
            if (_isNewInput)
            {
                // 如果是数字或小数点，替换显示内容
                if (input.Length == 1 && (char.IsDigit(input[0]) || input == "."))
                {
                    Display.Text = input == "." ? "0." : input;
                }
                else
                {
                    // 操作符或函数，追加到当前值
                    Display.Text += input;
                }
                _isNewInput = false;
            }
            else
            {
                Display.Text += input;
            }
        }

        private void Calculate()
        {
            var expression = Display.Text;
            HistoryDisplay.Text = expression + " =";

            try
            {
                var result = _engine.Evaluate(expression);
                Display.Text = FormatResult(result);
                _isNewInput = true;
            }
            catch (DivideByZeroException)
            {
                Display.Text = "Error: Div/0";
                _isNewInput = true;
            }
            catch (Exception ex)
            {
                Display.Text = "Error";
                HistoryDisplay.Text = ex.Message;
                _isNewInput = true;
            }
        }

        private void ToggleSign()
        {
            if (Display.Text.StartsWith("-"))
            {
                Display.Text = Display.Text[1..];
            }
            else if (Display.Text != "0" && Display.Text != "Error")
            {
                Display.Text = "-" + Display.Text;
            }
        }

        private static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
            {
                return ((long)value).ToString();
            }
            return value.ToString("G10");
        }

        private static string FormatResult(double result)
        {
            // 处理特殊值
            if (double.IsNaN(result))
                return "NaN";
            if (double.IsPositiveInfinity(result))
                return "∞";
            if (double.IsNegativeInfinity(result))
                return "-∞";

            // 非常小的数使用科学计数法
            if (result != 0 && Math.Abs(result) < 1e-10)
            {
                return result.ToString("E6");
            }

            // 非常大的数使用科学计数法
            if (Math.Abs(result) > 1e10)
            {
                return result.ToString("E6");
            }

            // 整数
            if (result == Math.Floor(result) && Math.Abs(result) < 1e15)
            {
                return ((long)result).ToString();
            }

            // 一般数值
            return result.ToString("G10");
        }

        /// <summary>
        /// 设置角度模式
        /// </summary>
        public void SetAngleMode(string mode)
        {
            _engine.AngleMode = mode;
            foreach (ComboBoxItem item in AngleModeCombo.Items)
            {
                if (item.Content?.ToString() == mode)
                {
                    AngleModeCombo.SelectedItem = item;
                    break;
                }
            }
        }

        /// <summary>
        /// 获取角度模式
        /// </summary>
        public string GetAngleMode()
        {
            return _engine.AngleMode;
        }
    }
}