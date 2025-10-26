using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using Lab_1.Models; // Замініть на правильний простір імен вашого проєкту
using Lab_1.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Lab_1_UnitTests
{
    [TestClass]
    public class LexerTests
    {
        // =================================================================
        // ПОЗИТИВНІ ТЕСТИ (перевірка правильного розпізнавання)
        // =================================================================

        [TestMethod]
        public void Tokenise_EmptyString_ReturnsOnlyEndToken()
        {
            // Arrange
            var lexer = new Lexer("");
            var expected = new List<(TokenType type, string value)> { (TokenType.End, "") };

            // Act
            var actual = lexer.Tokenise();

            // Assert
            AssertTokensMatch(expected, actual);
        }

        [TestMethod]
        public void Tokenise_NumberAndCellReference_ReturnsCorrectTokens()
        {
            // Arrange
            var lexer = new Lexer("123 B45");
            var expected = new List<(TokenType type, string value)>
        {
            (TokenType.Number, "123"),
            (TokenType.CellReference, "B45"),
            (TokenType.End, "")
        };

            // Act
            var actual = lexer.Tokenise();

            // Assert
            AssertTokensMatch(expected, actual);
        }

        [TestMethod]
        public void Tokenise_AllOperatorsAndParentheses_ReturnsCorrectTokens()
        {
            // Arrange
            var lexer = new Lexer("+-=<> () ! && ||");
            var expected = new List<(TokenType type, string value)>
        {
            (TokenType.Plus, "+"),
            (TokenType.Minus, "-"),
            (TokenType.Equals, "="),
            (TokenType.Less, "<"),
            (TokenType.Greater, ">"),
            (TokenType.LeftParen, "("),
            (TokenType.RightParen, ")"),
            (TokenType.Not, "!"),
            (TokenType.And, "&&"),
            (TokenType.Or, "||"),
            (TokenType.End, "")
        };

            // Act
            var actual = lexer.Tokenise();

            // Assert
            AssertTokensMatch(expected, actual);
        }

        [TestMethod]
        public void Tokenise_KeywordsAreCaseInsensitive_ReturnsCorrectTokens()
        {
            // Arrange
            var lexer = new Lexer("nOt AnD oR eQv");
            var expected = new List<(TokenType type, string value)>
        {
            (TokenType.Not, "not"),
            (TokenType.And, "and"),
            (TokenType.Or, "or"),
            (TokenType.Equals, "eqv"),
            (TokenType.End, "")
        };

            // Act
            var actual = lexer.Tokenise();

            // Assert
            AssertTokensMatch(expected, actual);
        }

        [TestMethod]
        public void Tokenise_ComplexRealExpression_ReturnsCorrectSequence()
        {
            // Arrange
            var lexer = new Lexer("(A1 > 10 and B2=0) || !C3");
            var expected = new List<(TokenType type, string value)>
        {
            (TokenType.LeftParen, "("),
            (TokenType.CellReference, "A1"),
            (TokenType.Greater, ">"),
            (TokenType.Number, "10"),
            (TokenType.And, "and"),
            (TokenType.CellReference, "B2"),
            (TokenType.Equals, "="),
            (TokenType.Number, "0"),
            (TokenType.RightParen, ")"),
            (TokenType.Or, "||"),
            (TokenType.Not, "!"),
            (TokenType.CellReference, "C3"),
            (TokenType.End, "")
        };

            // Act
            var actual = lexer.Tokenise();

            // Assert
            AssertTokensMatch(expected, actual);
        }

        // =================================================================
        // НЕГАТИВНІ ТЕСТИ (перевірка правильної обробки помилок)
        // =================================================================

        [TestMethod]
        public void Tokenise_UnknownCharacter_ThrowsException()
        {
            // Arrange
            var lexer = new Lexer("A1 # B2");

            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => lexer.Tokenise());
            Assert.AreEqual("Unknown character: #", ex.Message);
        }

        [TestMethod]
        public void Tokenise_UnknownIdentifier_ThrowsException()
        {
            // Arrange
            var lexer = new Lexer("A1 someWord B2");

            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => lexer.Tokenise());
            Assert.AreEqual("#REF!", ex.Message);
        }

        [TestMethod]
        public void Tokenise_InvalidSingleAmpersand_ThrowsException()
        {
            // Arrange
            var lexer = new Lexer("A1 & B2");

            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => lexer.Tokenise());
            Assert.AreEqual("Unexpected character after &", ex.Message);
        }

        // =================================================================
        // ДОПОМІЖНИЙ МЕТОД
        // =================================================================
        private void AssertTokensMatch(List<(TokenType type, string value)> expected, List<Token> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Wrong number of tokens.");

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].type, actual[i].Type, $"Token #{i + 1} has wrong type.");
                Assert.AreEqual(expected[i].value.ToUpper(), actual[i].Value.ToUpper(), $"Token #{i + 1} has wrong value.");
            }
        }
    }
}