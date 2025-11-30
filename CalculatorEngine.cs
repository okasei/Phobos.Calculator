using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Phobos.Calculator
{
    /// <summary>
    /// 计算器引擎 - 支持表达式解析和计算
    /// </summary>
    public class CalculatorEngine
    {
        /// <summary>
        /// 角度模式: Deg, Rad, Grad
        /// </summary>
        public string AngleMode { get; set; } = "Deg";

        /// <summary>
        /// 小数精度
        /// </summary>
        public int Precision { get; set; } = 10;

        /// <summary>
        /// 上一次计算结果
        /// </summary>
        public double LastAnswer { get; private set; } = 0;

        /// <summary>
        /// 内存值
        /// </summary>
        public double Memory { get; set; } = 0;

        private int _position;
        private string _expression = string.Empty;

        #region 公开方法

        /// <summary>
        /// 计算表达式
        /// </summary>
        public double Evaluate(string expression)
        {
            _expression = expression.Replace(" ", "").Replace("ans", LastAnswer.ToString(CultureInfo.InvariantCulture));
            _position = 0;

            var result = ParseExpression();

            if (_position < _expression.Length)
            {
                throw new Exception($"Unexpected character: {_expression[_position]}");
            }

            LastAnswer = result;
            return Math.Round(result, Precision);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public double ToRadians(double angle)
        {
            return AngleMode switch
            {
                "Rad" => angle,
                "Grad" => angle * Math.PI / 200,
                _ => angle * Math.PI / 180 // Deg
            };
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public double FromRadians(double radians)
        {
            return AngleMode switch
            {
                "Rad" => radians,
                "Grad" => radians * 200 / Math.PI,
                _ => radians * 180 / Math.PI // Deg
            };
        }

        #endregion

        #region 表达式解析

        private double ParseExpression()
        {
            var result = ParseTerm();

            while (_position < _expression.Length)
            {
                var op = _expression[_position];
                if (op == '+')
                {
                    _position++;
                    result += ParseTerm();
                }
                else if (op == '-')
                {
                    _position++;
                    result -= ParseTerm();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private double ParseTerm()
        {
            var result = ParsePower();

            while (_position < _expression.Length)
            {
                var op = _expression[_position];
                if (op == '*' || op == '×')
                {
                    _position++;
                    result *= ParsePower();
                }
                else if (op == '/' || op == '÷')
                {
                    _position++;
                    var divisor = ParsePower();
                    if (divisor == 0)
                        throw new DivideByZeroException("Division by zero");
                    result /= divisor;
                }
                else if (op == '%')
                {
                    _position++;
                    var divisor = ParsePower();
                    if (divisor == 0)
                        throw new DivideByZeroException("Modulo by zero");
                    result %= divisor;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private double ParsePower()
        {
            var result = ParseUnary();

            if (_position < _expression.Length && (_expression[_position] == '^' || _expression[_position] == '²'))
            {
                if (_expression[_position] == '²')
                {
                    _position++;
                    result = result * result;
                }
                else
                {
                    _position++;
                    var exponent = ParseUnary();
                    result = Math.Pow(result, exponent);
                }
            }

            return result;
        }

        private double ParseUnary()
        {
            if (_position < _expression.Length)
            {
                if (_expression[_position] == '-')
                {
                    _position++;
                    return -ParseUnary();
                }
                if (_expression[_position] == '+')
                {
                    _position++;
                    return ParseUnary();
                }
            }

            return ParseFactor();
        }

        private double ParseFactor()
        {
            if (_position >= _expression.Length)
                throw new Exception("Unexpected end of expression");

            // 括号
            if (_expression[_position] == '(')
            {
                _position++;
                var result = ParseExpression();
                if (_position >= _expression.Length || _expression[_position] != ')')
                    throw new Exception("Missing closing parenthesis");
                _position++;
                return result;
            }

            // 函数
            var function = ParseFunction();
            if (function != null)
            {
                return EvaluateFunction(function);
            }

            // 常量
            var constant = ParseConstant();
            if (constant.HasValue)
            {
                return constant.Value;
            }

            // 数字
            return ParseNumber();
        }

        private string? ParseFunction()
        {
            var functions = new[]
            {
                "sinh", "cosh", "tanh", "asinh", "acosh", "atanh",
                "asin", "acos", "atan", "sin", "cos", "tan",
                "log10", "log2", "log", "ln", "exp",
                "sqrt", "cbrt", "abs", "floor", "ceil", "round",
                "fact", "!"
            };

            foreach (var func in functions)
            {
                if (_position + func.Length <= _expression.Length &&
                    _expression.Substring(_position, func.Length).ToLower() == func.ToLower())
                {
                    _position += func.Length;
                    return func.ToLower();
                }
            }

            return null;
        }

        private double? ParseConstant()
        {
            var constants = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "pi", Math.PI },
                { "π", Math.PI },
                { "e", Math.E },
                { "phi", (1 + Math.Sqrt(5)) / 2 }, // 黄金比例
                { "φ", (1 + Math.Sqrt(5)) / 2 }
            };

            foreach (var kvp in constants)
            {
                if (_position + kvp.Key.Length <= _expression.Length &&
                    _expression.Substring(_position, kvp.Key.Length).ToLower() == kvp.Key.ToLower())
                {
                    _position += kvp.Key.Length;
                    return kvp.Value;
                }
            }

            return null;
        }

        private double ParseNumber()
        {
            var start = _position;

            // 整数部分
            while (_position < _expression.Length && char.IsDigit(_expression[_position]))
            {
                _position++;
            }

            // 小数部分
            if (_position < _expression.Length && _expression[_position] == '.')
            {
                _position++;
                while (_position < _expression.Length && char.IsDigit(_expression[_position]))
                {
                    _position++;
                }
            }

            // 科学计数法
            if (_position < _expression.Length && (_expression[_position] == 'e' || _expression[_position] == 'E'))
            {
                _position++;
                if (_position < _expression.Length && (_expression[_position] == '+' || _expression[_position] == '-'))
                {
                    _position++;
                }
                while (_position < _expression.Length && char.IsDigit(_expression[_position]))
                {
                    _position++;
                }
            }

            if (start == _position)
                throw new Exception($"Expected number at position {_position}");

            var numberStr = _expression.Substring(start, _position - start);
            if (!double.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                throw new Exception($"Invalid number: {numberStr}");

            return number;
        }

        private double EvaluateFunction(string function)
        {
            // 处理阶乘特殊语法: 5! 
            if (function == "!")
            {
                // 回退获取前面的数字
                throw new Exception("Use fact(n) for factorial");
            }

            // 获取参数
            if (_position >= _expression.Length || _expression[_position] != '(')
                throw new Exception($"Expected '(' after function {function}");

            _position++;
            var arg = ParseExpression();

            if (_position >= _expression.Length || _expression[_position] != ')')
                throw new Exception($"Expected ')' after function argument");

            _position++;

            return function switch
            {
                // 三角函数
                "sin" => Math.Sin(ToRadians(arg)),
                "cos" => Math.Cos(ToRadians(arg)),
                "tan" => Math.Tan(ToRadians(arg)),
                "asin" => FromRadians(Math.Asin(arg)),
                "acos" => FromRadians(Math.Acos(arg)),
                "atan" => FromRadians(Math.Atan(arg)),

                // 双曲函数
                "sinh" => Math.Sinh(arg),
                "cosh" => Math.Cosh(arg),
                "tanh" => Math.Tanh(arg),
                "asinh" => Math.Asinh(arg),
                "acosh" => Math.Acosh(arg),
                "atanh" => Math.Atanh(arg),

                // 对数和指数
                "log" => Math.Log10(arg),
                "log10" => Math.Log10(arg),
                "log2" => Math.Log2(arg),
                "ln" => Math.Log(arg),
                "exp" => Math.Exp(arg),

                // 根号和幂
                "sqrt" => Math.Sqrt(arg),
                "cbrt" => Math.Cbrt(arg),

                // 其他
                "abs" => Math.Abs(arg),
                "floor" => Math.Floor(arg),
                "ceil" => Math.Ceiling(arg),
                "round" => Math.Round(arg),
                "fact" => Factorial((int)arg),

                _ => throw new Exception($"Unknown function: {function}")
            };
        }

        private double Factorial(int n)
        {
            if (n < 0)
                throw new Exception("Factorial of negative number");
            if (n > 170)
                throw new Exception("Factorial overflow");

            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        #endregion

        #region 便捷方法

        public double Add(double a, double b) => a + b;
        public double Subtract(double a, double b) => a - b;
        public double Multiply(double a, double b) => a * b;
        public double Divide(double a, double b)
        {
            if (b == 0) throw new DivideByZeroException();
            return a / b;
        }
        public double Power(double a, double b) => Math.Pow(a, b);
        public double Sqrt(double a) => Math.Sqrt(a);
        public double Sin(double a) => Math.Sin(ToRadians(a));
        public double Cos(double a) => Math.Cos(ToRadians(a));
        public double Tan(double a) => Math.Tan(ToRadians(a));
        public double Log(double a) => Math.Log10(a);
        public double Ln(double a) => Math.Log(a);
        public double Exp(double a) => Math.Exp(a);

        #endregion
    }
}