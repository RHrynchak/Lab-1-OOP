using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_1.Models.Enums
{
    public enum TokenType
    {
        Number,
        CellReference,
        Plus,
        Minus,
        Equals,
        Less,
        Greater,
        Not,
        Or,
        And,
        LeftParen,
        RightParen,
        End
    }
}
