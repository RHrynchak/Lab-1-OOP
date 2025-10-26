using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Lab_1.Models.Enums;

namespace Lab_1.Models
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position = 0;
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }
        private Token CurrentToken => _tokens[_position];
        private void MoveNext() => _position++;
        public ExpressionNode Parse()
        {
            var expr = ParseOr();
            if (CurrentToken.Type != TokenType.End)
            {
                throw new Exception("Unexpected token at the end of expression");
            }
            return expr;
        }
        private ExpressionNode ParsePrimary()
        {
            var token = CurrentToken;
            if (token.Type == TokenType.Number)
            {
                MoveNext();
                return new NumberNode(BigInteger.Parse(token.Value));
            }
            if (token.Type == TokenType.CellReference)
            {
                MoveNext();
                return new CellReferenceNode(token.Value);
            }
            if (token.Type == TokenType.LeftParen)
            {
                MoveNext();
                var expr = ParseOr();
                if (CurrentToken.Type != TokenType.RightParen)
                {
                    throw new Exception("Expected closing parenthesis");
                }
                MoveNext();
                return expr;
            }
            throw new Exception($"Unexpected token: {token.Type}");
        }

        private ExpressionNode ParseUnary()
        {
            var token = CurrentToken;
            if (token.Type == TokenType.Plus || token.Type == TokenType.Minus || token.Type == TokenType.Not)
            {
                MoveNext();
                var operand = ParseUnary();
                return new UnaryOpNode(token.Type, operand);
            }
            return ParsePrimary();
        }

        private ExpressionNode ParseAddition()
        {
            var left = ParseUnary();
            while ( CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus )
            {
                var op = CurrentToken;
                MoveNext();
                var right = ParseUnary();
                left = new BinaryOpNode(op.Type, left, right);
            }
            return left;
        }

        private ExpressionNode ParseComparison()
        {
            var left = ParseAddition();
            while ( CurrentToken.Type == TokenType.Equals || CurrentToken.Type == TokenType.Less || CurrentToken.Type == TokenType.Greater )
            {
                var op = CurrentToken;
                MoveNext();
                var right = ParseAddition();
                left = new BinaryOpNode(op.Type, left, right);
            }
            return left;
        }

        private ExpressionNode ParseAnd()
        {
            var left = ParseComparison();
            while ( CurrentToken.Type == TokenType.And )
            {
                var op = CurrentToken;
                MoveNext();
                var right = ParseComparison();
                left = new BinaryOpNode(op.Type, left, right);
            }
            return left;
        }

        private ExpressionNode ParseOr()
        {
            var left = ParseAnd();
            while ( CurrentToken.Type == TokenType.Or )
            {
                var op = CurrentToken;
                MoveNext();
                var right = ParseAnd();
                left = new BinaryOpNode(op.Type, left, right);
            }
            return left;
        }
    }
}
