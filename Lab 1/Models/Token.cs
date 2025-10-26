using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab_1.Models.Enums;

namespace Lab_1.Models
{
    public class Token
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
        public Token(TokenType type, string value = "")
        {
            Value = value;
            Type = type;
        }
    }
}
