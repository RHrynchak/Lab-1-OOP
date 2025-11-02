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

        [TestMethod]
        public void Evaluate_SimpleAddition()
        {
            AssertExpressionEquals("3 + -5", -2);
        }


        [TestMethod]
        public void Evaluate_ParenthesesOverridesPrecedence()
        {
            AssertExpressionEquals("100 - (10 + 5)", 85);
        }

        [TestMethod]
        public void Evaluate_UnaryMinus()
        {
            AssertExpressionEquals("-10", -10);
            AssertExpressionEquals("5 + -10", -5);
            AssertExpressionEquals("- -20", 20);
        }

        [TestMethod]
        public void Evaluate_ComparisonOperators()
        {
            AssertExpressionEquals("10 > 5", 1);
            AssertExpressionEquals("10 < 5", 0);
            AssertExpressionEquals("10 = 10", 1);
            AssertExpressionEquals("10 = 5", 0);
        }

        [TestMethod]
        public void Evaluate_LogicalNot()
        {
            AssertExpressionEquals("!1", 0);  
            AssertExpressionEquals("!0", 1);  
            AssertExpressionEquals("!50", 0); 
            AssertExpressionEquals("!!1", 1); 
        }

        [TestMethod]
        public void Evaluate_LogicalAnd()
        {
            AssertExpressionEquals("1 and 1", 1);
            AssertExpressionEquals("1 && 0", 0);
            AssertExpressionEquals("50 and -10", 1);
        }

        [TestMethod]
        public void Evaluate_LogicalOr()
        {
            AssertExpressionEquals("1 or 0", 1);
            AssertExpressionEquals("0 || 0", 0);
            AssertExpressionEquals("1 or 1", 1);
        }

        [TestMethod]
        public void Evaluate_CellReferences()
        {
            var context = new Dictionary<string, object?>
        {
            { "A1", new BigInteger(100) },
            { "B2", new BigInteger(25) }
        };

            AssertExpressionEquals("A1 - B2", 75, context);
        }

        [TestMethod]
        public void Evaluate_ReferenceToEmptyCell()
        {
            var context = new Dictionary<string, object?>
        {
            { "A1", new BigInteger(10) }
        };

            AssertExpressionEquals("A1 + B2", 10, context); // 10 + 0 = 10
        }

        [TestMethod]
        public void Evaluate_ComplexExpression()
        {
            var context = new Dictionary<string, object?>
        {
            { "A1", 20 },
            { "B2", 20 },
            { "C3", 0 } 
        };

            AssertExpressionEquals("(A1 > 10 and B2=20) or !C3", 1, context);
        }

        private void AssertExpressionEquals(string expression, BigInteger expectedResult, Dictionary<string, object?>? context = null)
        {
            try
            {
                var lexer = new Lexer(expression);
                var tokens = lexer.Tokenise();
                var parser = new Parser(tokens);
                var ast = parser.Parse();

                var evaluationContext = context ?? new Dictionary<string, object?>();

                var actualResult = ast.Evaluate(evaluationContext, 100, 100);

                Assert.AreEqual(expectedResult, actualResult, $"Expression '{expression}' failed.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expression '{expression}' threw an exception: {ex.Message}");
            }
        }
    }
}
