using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lab_1.Models;

namespace Lab_1_UnitTests
{
    [TestClass]
    public class EvaluationTests
    {
        // =================================================================
        // Арифметика та пріоритет операцій
        // =================================================================

        [TestMethod]
        public void Evaluate_SimpleAddition_ReturnsCorrectResult()
        {
            AssertExpressionEquals("3 + -5", -2);
        }

        [TestMethod]
        public void Evaluate_OperatorPrecedence_MultiplicationBeforeAddition()
        {
            // Цей тест буде працювати, коли ви додасте множення.
            // Зараз він показує, що додавання/віднімання мають однаковий пріоритет.
            AssertExpressionEquals("100 - 10 + 5", 95); // (100 - 10) + 5
        }

        [TestMethod]
        public void Evaluate_Parentheses_OverridesPrecedence()
        {
            // Цей тест також буде більш показовим з множенням,
            // наприклад, "(10 + 5) * 2" = 30
            AssertExpressionEquals("100 - (10 + 5)", 85); // 100 - 15
        }

        [TestMethod]
        public void Evaluate_UnaryMinus_CorrectlyNegatesValue()
        {
            AssertExpressionEquals("-10", -10);
            AssertExpressionEquals("5 + -10", -5);
            AssertExpressionEquals("- -20", 20); // Подвійне заперечення
        }

        // =================================================================
        // Логічні та порівняльні операції
        // =================================================================

        [TestMethod]
        public void Evaluate_ComparisonOperators_ReturnsOneForTrueZeroForFalse()
        {
            AssertExpressionEquals("10 > 5", 1);
            AssertExpressionEquals("10 < 5", 0);
            AssertExpressionEquals("10 = 10", 1);
            AssertExpressionEquals("10 = 5", 0);
        }

        [TestMethod]
        public void Evaluate_LogicalNot_InvertsBooleanValue()
        {
            AssertExpressionEquals("!1", 0);  // not true -> false (0)
            AssertExpressionEquals("!0", 1);  // not false -> true (1)
            AssertExpressionEquals("!50", 0); // Будь-яке не-нульове число - це true
            AssertExpressionEquals("!!1", 1); // Подвійне заперечення
        }

        [TestMethod]
        public void Evaluate_LogicalAnd_ReturnsCorrectResult()
        {
            AssertExpressionEquals("1 and 1", 1);
            AssertExpressionEquals("1 && 0", 0);
            AssertExpressionEquals("50 and -10", 1); // true and true -> true
        }

        [TestMethod]
        public void Evaluate_LogicalOr_ReturnsCorrectResult()
        {
            AssertExpressionEquals("1 or 0", 1);
            AssertExpressionEquals("0 || 0", 0);
            AssertExpressionEquals("1 or 1", 1);
        }

        // =================================================================
        // Робота з клітинками та контекстом
        // =================================================================

        [TestMethod]
        public void Evaluate_WithCellReferences_UsesContextValues()
        {
            // Arrange
            var context = new Dictionary<string, object?>
        {
            { "A1", new BigInteger(100) },
            { "B2", new BigInteger(25) }
        };

            // Act & Assert
            AssertExpressionEquals("A1 - B2", 75, context);
        }

        [TestMethod]
        public void Evaluate_ReferenceToEmptyCell_TreatsAsZero()
        {
            // Arrange
            var context = new Dictionary<string, object?>
        {
            { "A1", new BigInteger(10) } // B2 відсутня в контексті
        };

            // Act & Assert
            AssertExpressionEquals("A1 + B2", 10, context); // 10 + 0 = 10
        }

        [TestMethod]
        public void Evaluate_ComplexExpression_ReturnsCorrectResult()
        {
            // Arrange
            var context = new Dictionary<string, object?>
        {
            { "A1", 20 },
            { "B2", 20 },
            { "C3", 0 } // false
        };

            // (20 > 10 and 20=20) or !(0)  ->  (true and true) or true  ->  true or true  ->  true (1)
            AssertExpressionEquals("(A1 > 10 and B2=20) or !C3", 1, context);
        }

        // =================================================================
        // МЕТОД-ПОМІЧНИК
        // =================================================================

        /// <summary>
        /// Допоміжний метод, який виконує весь ланцюжок (Лексер->Парсер->Evaluate)
        /// і перевіряє результат.
        /// </summary>
        private void AssertExpressionEquals(string expression, BigInteger expectedResult, Dictionary<string, object?>? context = null)
        {
            try
            {
                // Arrange
                var lexer = new Lexer(expression);
                var tokens = lexer.Tokenise();
                var parser = new Parser(tokens);
                var ast = parser.Parse();

                // Якщо контекст не надано, використовуємо порожній
                var evaluationContext = context ?? new Dictionary<string, object?>();

                // Act
                var actualResult = ast.Evaluate(evaluationContext, 100, 100);

                // Assert
                Assert.AreEqual(expectedResult, actualResult, $"Expression '{expression}' failed.");
            }
            catch (Exception ex)
            {
                // Якщо тест не очікував помилки, а вона виникла, він провалиться.
                Assert.Fail($"Expression '{expression}' threw an exception: {ex.Message}");
            }
        }
    }
}
