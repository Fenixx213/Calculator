using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Dictionary<string, double> variables = new Dictionary<string, double>();
        private Dictionary<string, (string[] parameters, string expression)> functions = new Dictionary<string, (string[], string)>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        bool CheckParenthesesBalance(string expression)
        {
            int balance = 0;
            foreach (char c in expression)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0) return false;
            }
            return balance == 0;
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            Evaluate(label1, true);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Evaluate(label2, false);
        }
        private void Evaluate(Label label, bool KE)
        {
            string input = textBox1.Text.Trim();
            label.Text = "";

            // Проверка на определение функции: f(x,y) = x+y
            if (Regex.IsMatch(input, @"^\s*[a-zA-Z]+\s*\([^)]+\)\s*=\s*.+"))
            {
                var match = Regex.Match(input, @"^\s*([a-zA-Z]+)\s*\(([^)]+)\)\s*=\s*(.+)\s*$");
                string funcName = match.Groups[1].Value;
                string[] parameters = match.Groups[2].Value.Split(',').Select(p => p.Trim()).ToArray();
                string expression = match.Groups[3].Value.Trim();

                if (!CheckParenthesesBalance(expression))
                {
                    label.Text = "Некорректное выражение: несбалансированные скобки";
                    return;
                }
                if (KE)
                {
                    functions[funcName] = (parameters, expression);
                    string parametersString = string.Join(",", parameters);
                    label.Text = $"{funcName}({parametersString}) = {expression}";
                }
                return;
            }
            // Проверка на определение переменной: x = 1
            else if (Regex.IsMatch(input, @"^\s*[a-zA-Z]+\s*=\s*.+"))
            {
                var match = Regex.Match(input, @"^\s*([a-zA-Z]+)\s*=\s*(.+)\s*$");
                string varName = match.Groups[1].Value;
                string expression = match.Groups[2].Value.Trim();

                if (!CheckParenthesesBalance(expression))
                {
                    label.Text = "Некорректное выражение: несбалансированные скобки";
                    return;
                }

                string evalExpression = ReplaceVariablesAndFunctions(expression);

                try
                {
                    DataTable dt = new DataTable();
                    var result = dt.Compute(evalExpression, "");
                    if (KE)
                    {
                        if (result is double || result is int)
                        {
                            double value = Convert.ToDouble(result);
                            variables[varName] = value;
                            label.Text = $"{varName} = {value}";
                        }
                        else
                        {
                            label.Text = "Ошибка: результат не является числом";
                        }
                    }
                }
                catch (Exception ex)
                {
                    label.Text = $"Ошибка: {ex.Message}";
                }
            }
            // Обычное выражение или вызов функции
            else
            {
                if (!string.IsNullOrEmpty(input) && CheckParenthesesBalance(input))
                {
                    try
                    {
                        string evalExpression = ReplaceVariablesAndFunctions(input);
                        DataTable dt = new DataTable();
                        var result = dt.Compute(evalExpression, "");
                        label.Text = result.ToString();
                    }
                    catch (Exception ex)
                    {
                        label.Text = $"Ошибка: {ex.Message}";
                    }
                }
                else
                {
                    label.Text = "Некорректное выражение";
                }
            }
        }
        private string ReplaceVariablesAndFunctions(string expression)
        {
            string result = expression;

            // Замена вызовов функций
            foreach (var func in functions)
            {
                string funcName = func.Key;
                var (parameters, funcExpression) = func.Value;

                // Поиск вызова функции: f(1,2)
                string pattern = $@"\b{funcName}\s*\(([^)]+)\)";
                result = Regex.Replace(result, pattern, m =>
                {
                    string[] args = m.Groups[1].Value.Split(',').Select(a => a.Trim()).ToArray();
                    if (args.Length != parameters.Length)
                    {
                        label2.Text = $"Ошибка: Неверное количество аргументов для функции {funcName}";
                        return m.Value; // Возвращаем исходное выражение, чтобы избежать дальнейшей обработки
                    }

                    // Заменяем параметры в выражении функции на переданные аргументы
                    string evalExpression = funcExpression;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        evalExpression = Regex.Replace(evalExpression, $@"\b{parameters[i]}\b", args[i]);
                    }
                    return $"({evalExpression})";
                });

                // Если label.Text уже содержит ошибку, прерываем обработку
                if (label2.Text.StartsWith("Ошибка"))
                {
                    return result;
                }
            }

            // Замена переменных
            foreach (var variable in variables)
            {
                result = Regex.Replace(result, $@"\b{variable.Key}\b", variable.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return result;
        }
        private void buttonplus_Click(object sender, EventArgs e)
        {
           
            textBox1.Text += "+";
        }

        private void buttonminus_Click(object sender, EventArgs e)
        {
            textBox1.Text += "-";
        }

        private void buttonmult_Click(object sender, EventArgs e)
        {
            textBox1.Text += "*";
        }

        private void buttondivide_Click(object sender, EventArgs e)
        {
            textBox1.Text += "/";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text += "1";
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text += "2";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text += "3";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text += "4";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text += "5";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text += "6";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBox1.Text += "7";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox1.Text += "8";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            textBox1.Text += "9";
        }

        private void button0_Click(object sender, EventArgs e)
        {
            textBox1.Text += "0";
        }

        private void buttonequal_Click(object sender, EventArgs e)
        {
            textBox1.Text += "=";
        }

        private void buttonC_Click(object sender, EventArgs e)
        {
            variables.Clear();
            functions.Clear();
            textBox1.Text = string.Empty;
            label1.Text = string.Empty;
            label2.Text = string.Empty;
        }
    }
}
