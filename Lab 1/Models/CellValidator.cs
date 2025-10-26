using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lab_1.Models
{
    public static class CellValidator
    {
        private static readonly Regex CellFormatRegex = new Regex(@"^[A-Za-z]+[0-9]+$", RegexOptions.Compiled);
        public static bool IsValidCellName(string cellName)
        {
            if ( string.IsNullOrWhiteSpace(cellName) )
                return false;
            cellName = cellName.ToUpper();
            if ( !CellFormatRegex.IsMatch(cellName) )
                return false;
            return true;
        }
    }
}
