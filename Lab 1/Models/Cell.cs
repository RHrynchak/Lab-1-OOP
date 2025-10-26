using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Lab_1.Models
{
    public class Cell
    {
        public string _input = string.Empty;
        public string Input
        {
            get => _input;
            set
            {
                _input = value;
                _astCache = null;
            }
        }
        private ExpressionNode? _astCache;
        public object? CalculatedValue { get; private set; }
        public CellFormat Format { get; set; } = new CellFormat();
        public bool HasFormula => !string.IsNullOrEmpty(Input) && Input.StartsWith('=');
        public void SetErrorValue(string message)
        {
            CalculatedValue = message;
            _astCache = null;
        }
        public void Evaluate( Dictionary<string, object?> context, int RowCount, int ColumnCount)
        {
            if (string.IsNullOrEmpty(Input))
            {
                CalculatedValue = string.Empty;
                return;
            }
            if (Input.StartsWith('='))
            {
                try
                {
                    if ( _astCache == null )
                    {
                        Lexer lexer = new Lexer(Input.Substring(1));
                        var tokens = lexer.Tokenise();
                        Parser parser = new Parser(tokens);
                        _astCache = parser.Parse();
                    }
                    CalculatedValue = _astCache.Evaluate(context, RowCount, ColumnCount );
                }
                catch (Exception ex)
                {
                    if ( ex.Message == "#REF!" )
                    {
                        CalculatedValue = "#REF!";
                        return;
                    }
                    CalculatedValue = $"#ERROR!";
                }
            }
            else
            {
                if ( BigInteger.TryParse(Input, out BigInteger intValue) )
                {
                    CalculatedValue = intValue;
                }
                else
                {
                    CalculatedValue = Input;
                }
            }
        }
    }
}
