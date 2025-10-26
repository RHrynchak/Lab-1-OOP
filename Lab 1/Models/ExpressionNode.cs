using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lab_1.Models.Enums;

namespace Lab_1.Models
{
    public abstract class ExpressionNode
    {
        public abstract BigInteger Evaluate( Dictionary<string, object?> context, int rowCount, int columnCount );
    }

    public class NumberNode : ExpressionNode
    {
        public BigInteger Value { get; }
        public NumberNode( BigInteger value )
        {
            Value = value;
        }
        public override BigInteger Evaluate( Dictionary<string, object?> context, int rowCount, int columnCount)
        {
            return Value;
        }
    }

    public class CellReferenceNode : ExpressionNode
    {
        public string Name { get; }
        public CellReferenceNode( string cellReference )
        {
            Name = cellReference.ToUpper();
        }
        public override BigInteger Evaluate( Dictionary<string, object?> context, int rowCount, int columnCount)
        {
            string columnPart = String.Concat( Name.TakeWhile( c => Char.IsLetter(c) ) );
            string rowPart = String.Concat( Name.SkipWhile( c => Char.IsLetter(c) ) );
            int columnNumber = SpreadsheetUtils.ToColumnIndex( columnPart ) + 1;
            if ( string.IsNullOrEmpty(columnPart) || string.IsNullOrEmpty(rowPart) || !int.TryParse(rowPart, out int rowNumber) || rowNumber < 1 || rowNumber > rowCount || columnNumber < 1 || columnNumber > columnCount )
            {
                throw new Exception( "#REF!" );
            }
            if ( context.TryGetValue( Name, out var value ) )
            {
                if (value is string strValue && strValue.StartsWith("#"))
                {
                    throw new Exception("Propagating an existing error: " + strValue);
                }
                if (value is BigInteger numValue)
                {
                    return numValue;
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }
    }

    public class UnaryOpNode : ExpressionNode
    {
        public TokenType Operator { get; }
        public ExpressionNode Operand { get; }
        public UnaryOpNode( TokenType op, ExpressionNode operand )
        {
            Operator = op;
            Operand = operand;
        }
        public override BigInteger Evaluate( Dictionary<string, object?> context, int rowCount, int columnCount)
        {
            var OperandValue = Operand.Evaluate( context, rowCount, columnCount);
            return Operator switch
            {
                TokenType.Plus => OperandValue,
                TokenType.Minus => -OperandValue,
                TokenType.Not => OperandValue == 0 ? 1 : 0,
                _ => throw new Exception( $"Unknown unary operator: {Operator}" ),
            };
        }
    }

    public class BinaryOpNode : ExpressionNode
    {
        public TokenType Operator { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }
        public BinaryOpNode( TokenType op, ExpressionNode left, ExpressionNode right )
        {
            Operator = op;
            Left = left;
            Right = right;
        }
        public override BigInteger Evaluate( Dictionary<string, object?> context, int rowCount, int columnCount)
        {
            var LeftValue = Left.Evaluate( context, rowCount, columnCount );
            var RightValue = Right.Evaluate( context, rowCount, columnCount);
            return Operator switch
            {
                TokenType.Plus => LeftValue + RightValue,
                TokenType.Minus => LeftValue - RightValue,
                TokenType.And => (LeftValue != 0 && RightValue != 0) ? 1 : 0,
                TokenType.Or => (LeftValue != 0 || RightValue != 0) ? 1 : 0,
                TokenType.Equals => (LeftValue == RightValue ) ? 1 : 0,
                TokenType.Less => (LeftValue < RightValue ) ? 1 : 0,
                TokenType.Greater => (LeftValue > RightValue ) ? 1 : 0,
                _ => throw new Exception( $"Unknown binary operator: {Operator}" ),
            };
        }
    }
}
