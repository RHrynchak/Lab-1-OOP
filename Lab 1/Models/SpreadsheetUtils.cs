using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab_1.Models
{
    public static class SpreadsheetUtils
    {
        public static string ToColumnName(int columnIndex)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "Index cannot be negative.");
            }
            string columnName = "";
            int tempIndex = columnIndex + 1;
            while (tempIndex > 0)
            {
                int remainder = (tempIndex - 1) % 26;
                columnName = (char)('A' + remainder) + columnName;
                tempIndex = (tempIndex - 1) / 26;
            }
            return columnName;
        }
        public static int ToColumnIndex(string columnName)
        {
            if ( string.IsNullOrEmpty(columnName) || !columnName.All(c => char.IsLetter(c)))
                throw new ArgumentException("Column name must be a non-empty string of letters.", nameof(columnName));
            int index = 0;
            int power = 1;
            foreach (char c in columnName.ToUpper().Reverse())
            {
                index += (c - 'A' + 1) * power;
                power *= 26;
            }
            return index - 1;
        }
        public static (int row, int column) NameToCoordinates( string cellName )
        {
            string columnPart = String.Concat( cellName.TakeWhile( c => Char.IsLetter(c) ) );
            string rowPart = String.Concat( cellName.SkipWhile( c => Char.IsLetter(c) ) );
            if ( string.IsNullOrEmpty(columnPart) || string.IsNullOrEmpty(rowPart) )
            {
                throw new ArgumentException("Invalid cell name format.", nameof(cellName));
            }
            int columnIndex = ToColumnIndex(columnPart);
            if ( !int.TryParse(rowPart, out int rowNumber) || rowNumber < 1 )
            {
                throw new ArgumentException("Invalid row number in cell name.", nameof(cellName));
            }
            return (row: rowNumber - 1, column : columnIndex);
        }
        public static List<string> GetDependencies( string formula )
        {
            var dependencies = new List<string>();
            var lexer = new Lexer(formula);
            try {
                var tokens = lexer.Tokenise();
                foreach (var token in tokens)
                {
                    if (token.Type == Enums.TokenType.CellReference)
                    {
                        dependencies.Add(token.Value);
                    }
                }
            }
            catch ( Exception )
            {
                return new List<string>();
            }
            return dependencies;
        }
    }
}
