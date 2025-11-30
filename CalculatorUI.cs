using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Phobos.Calculator
{
    /// <summary>
    /// 计算器 UI 控件 - 嵌入主程序显示
    /// </summary>
    public class CalculatorUI : UserControl
    {
        private readonly Calculator _plugin;
        private readonly CalculatorEngine _engine;

        // UI 控件
        private TextBox? _display;
        private TextBlock? _historyDisplay;
        private ComboBox? _angleModeCombo;
        private Button[]? _numberButtons;
        private bool _isNewInput = true;
        private string _currentExpression = string.Empty;

        public CalculatorUI(Calculator plugin)
        {
            _plugin = plugin;
            _engine = new CalculatorEngine();
            InitializeUI();
        }

        private void InitializeUI()
        {
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 设置栏
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 历史显示
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 显示屏
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 按钮区

            // ===== 设置栏 =====
            var settingsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var angleModeLabel = new TextBlock
            {
                Text = "Angle:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            _angleModeCombo = new ComboBox
            {
                Width = 70,
                SelectedIndex = 0
            };
            _angleModeCombo.Items.Add("Deg");
            _angleModeCombo.Items.Add("Rad");
            _angleModeCombo.Items.Add("Grad");
            _angleModeCombo.SelectionChanged += (s, e) =>
            {
                _engine.AngleMode = _angleModeCombo.SelectedItem?.ToString() ?? "Deg";
            };

            var windowButton = new Button
            {
                Content = "⧉",
                Width = 30,
                Height = 24,
                Margin = new Thickness(10, 0, 0, 0),
                ToolTip = "Open in new window"
            };
            windowButton.Click += (s, e) => _plugin.OpenCalculatorWindow();

            settingsPanel.Children.Add(angleModeLabel);
            settingsPanel.Children.Add(_angleModeCombo);
            settingsPanel.Children.Add(windowButton);

            Grid.SetRow(settingsPanel, 0);
            mainGrid.Children.Add(settingsPanel);

            // ===== 历史显示 =====
            _historyDisplay = new TextBlock
            {
                Height = 20,
                Margin = new Thickness(10, 5, 10, 0),
                FontSize = 12,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right
            };
            Grid.SetRow(_historyDisplay, 1);
            mainGrid.Children.Add(_historyDisplay);

            // ===== 主显示屏 =====
            _display = new TextBox
            {
                Height = 50,
                Margin = new Thickness(10, 5, 10, 10),
                FontSize = 28,
                FontFamily = new FontFamily("Consolas"),
                TextAlignment = TextAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = "0",
                IsReadOnly = false
            };
            _display.KeyDown += Display_KeyDown;
            Grid.SetRow(_display, 2);
            mainGrid.Children.Add(_display);

            // ===== 按钮区 =====
            var buttonGrid = CreateButtonGrid();
            Grid.SetRow(buttonGrid, 3);
            mainGrid.Children.Add(buttonGrid);

            Content = mainGrid;
        }

        private Grid CreateButtonGrid()
        {
            var grid = new Grid { Margin = new Thickness(5) };

            // 7 列
            for (int i = 0; i < 7; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 6 行
            for (int i = 0; i < 6; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 40 });
            }

            // 按钮布局
            var buttons = new (string Text, int Row, int Col, int ColSpan, string? Tag)[]
            {
                // 第一行: 科学函数
                ("sin", 0, 0, 1, "sin("),
                ("cos", 0, 1, 1, "cos("),
                ("tan", 0, 2, 1, "tan("),
                ("log", 0, 3, 1, "log("),
                ("ln", 0, 4, 1, "ln("),
                ("(", 0, 5, 1, "("),
                (")", 0, 6, 1, ")"),

                // 第二行: 更多科学函数
                ("asin", 1, 0, 1, "asin("),
                ("acos", 1, 1, 1, "acos("),
                ("atan", 1, 2, 1, "atan("),
                ("x²", 1, 3, 1, "^2"),
                ("√", 1, 4, 1, "sqrt("),
                ("xʸ", 1, 5, 1, "^"),
                ("π", 1, 6, 1, "π"),

                // 第三行: 数字 7-9 和操作
                ("7", 2, 0, 1, null),
                ("8", 2, 1, 1, null),
                ("9", 2, 2, 1, null),
                ("÷", 2, 3, 1, "/"),
                ("exp", 2, 4, 1, "exp("),
                ("MC", 2, 5, 1, "MC"),
                ("MR", 2, 6, 1, "MR"),

                // 第四行: 数字 4-6 和操作
                ("4", 3, 0, 1, null),
                ("5", 3, 1, 1, null),
                ("6", 3, 2, 1, null),
                ("×", 3, 3, 1, "*"),
                ("n!", 3, 4, 1, "fact("),
                ("M+", 3, 5, 1, "M+"),
                ("M-", 3, 6, 1, "M-"),

                // 第五行: 数字 1-3 和操作
                ("1", 4, 0, 1, null),
                ("2", 4, 1, 1, null),
                ("3", 4, 2, 1, null),
                ("-", 4, 3, 1, null),
                ("e", 4, 4, 1, "e"),
                ("%", 4, 5, 1, null),
                ("CE", 4, 6, 1, "CE"),

                // 第六行: 0, 小数点, 等号等
                ("0", 5, 0, 1, null),
                (".", 5, 1, 1, null),
                ("±", 5, 2, 1, "NEG"),
                ("+", 5, 3, 1, null),
                ("Ans", 5, 4, 1, "ans"),
                ("C", 5, 5, 1, "C"),
                ("=", 5, 6, 1, "="),
            };

            foreach (var (text, row, col, colSpan, tag) in buttons)
            {
                var button = CreateButton(text, tag);
                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);
                if (colSpan > 1)
                    Grid.SetColumnSpan(button, colSpan);
                grid.Children.Add(button);
            }

            return grid;
        }

        private Button CreateButton(string text, string? tag)
        {
            var button = new Button
            {
                Content = text,
                Margin = new Thickness(2),
                FontSize = 16,
                Tag = tag ?? text
            };

            // 根据类型设置样式
            if (text == "=")
            {
                button.Background = new SolidColorBrush(Color.FromRgb(30, 144, 255));
                button.Foreground = Brushes.White;
            }
            else if ("+-×÷".Contains(text))
            {
                button.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                button.Foreground = Brushes.White;
            }
            else if (text == "C" || text == "CE")
            {
                button.Background = new SolidColorBrush(Color.FromRgb(200, 60, 60));
                button.Foreground = Brushes.White;
            }

            button.Click += Button_Click;

            return button;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || _display == null)
                return;

            var tag = button.Tag?.ToString() ?? button.Content?.ToString() ?? string.Empty;
            ProcessInput(tag);
        }

        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                ProcessInput("=");
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ProcessInput("C");
                e.Handled = true;
            }
        }

        private void ProcessInput(string input)
        {
            if (_display == null) return;

            switch (input)
            {
                case "=":
                    Calculate();
                    break;

                case "C":
                    _display.Text = "0";
                    _currentExpression = string.Empty;
                    _historyDisplay!.Text = string.Empty;
                    _isNewInput = true;
                    break;

                case "CE":
                    _display.Text = "0";
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
                        _display.Text = _engine.Memory.ToString();
                    }
                    else
                    {
                        _display.Text += _engine.Memory.ToString();
                    }
                    _isNewInput = false;
                    break;

                case "M+":
                    if (double.TryParse(_display.Text, out var mAdd))
                        _engine.Memory += mAdd;
                    break;

                case "M-":
                    if (double.TryParse(_display.Text, out var mSub))
                        _engine.Memory -= mSub;
                    break;

                default:
                    AppendInput(input);
                    break;
            }
        }

        private void AppendInput(string input)
        {
            if (_display == null) return;

            if (_isNewInput)
            {
                // 如果是数字或小数点，替换显示内容
                if (char.IsDigit(input[0]) || input == ".")
                {
                    _display.Text = input == "." ? "0." : input;
                }
                else
                {
                    // 操作符或函数，追加到当前值
                    _display.Text += input;
                }
                _isNewInput = false;
            }
            else
            {
                _display.Text += input;
            }
        }

        private void Calculate()
        {
            if (_display == null || _historyDisplay == null) return;

            var expression = _display.Text;
            _historyDisplay.Text = expression + " =";

            try
            {
                var result = _engine.Evaluate(expression);

                // 格式化结果
                if (Math.Abs(result) < 1e-10 && result != 0)
                {
                    _display.Text = result.ToString("E6");
                }
                else if (Math.Abs(result) > 1e10)
                {
                    _display.Text = result.ToString("E6");
                }
                else
                {
                    _display.Text = result.ToString("G10");
                }

                _isNewInput = true;
            }
            catch (Exception ex)
            {
                _display.Text = "Error";
                _historyDisplay.Text = ex.Message;
                _isNewInput = true;
            }
        }

        private void ToggleSign()
        {
            if (_display == null) return;

            if (_display.Text.StartsWith("-"))
            {
                _display.Text = _display.Text[1..];
            }
            else if (_display.Text != "0")
            {
                _display.Text = "-" + _display.Text;
            }
        }

        #region 公开方法

        public void SetAngleMode(string mode)
        {
            _engine.AngleMode = mode;
            if (_angleModeCombo != null)
            {
                _angleModeCombo.SelectedItem = mode;
            }
        }

        public string GetAngleMode()
        {
            return _engine.AngleMode;
        }

        public void SetPrecision(int precision)
        {
            _engine.Precision = precision;
        }

        public int GetPrecision()
        {
            return _engine.Precision;
        }

        public void RefreshTheme()
        {
            // 可以在这里根据新主题刷新控件样式
            var theme = _plugin.GetThemeResources();
            if (theme != null)
            {
                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(theme);
            }
        }

        public void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[Calculator] {message}");
        }

        #endregion
    }
}