using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lab_1.Models.Enums;

namespace Lab_1.Models
{
    public class Lexer
    {
        private string _input;
        private int _position;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
        }

        private Token ReadNumber()
        {
            int start = _position;
            while ( _position < _input.Length && char.IsDigit(_input[_position]) )
            {
                _position++;
            }
            string number = _input.Substring(start, _position - start);
            return new Token(TokenType.Number, number);
        }

        private string ReadWord()
        {
            int start = _position;
            while ( _position < _input.Length && char.IsLetterOrDigit(_input[_position]) )
            {
                _position++;
            }
            return _input.Substring(start, _position - start);
        }

        public List<Token> Tokenise()
        {
            var tokens = new List<Token>();
            while ( _position < _input.Length )
            {
                char currentChar = _input[_position];
                
                if ( char.IsWhiteSpace(currentChar) )
                {
                    _position++;
                    continue;
                }

                if ( char.IsDigit(currentChar) )
                {
                    tokens.Add( ReadNumber() );
                    continue;
                }

                if ( char.IsLetter(currentChar) )
                {
                    string word = ReadWord();
                    
                    switch (word.ToLower())
                    {
                        case "not":
                            tokens.Add( new Token(TokenType.Not, "not") );
                            break;
                        case "and":
                            tokens.Add( new Token(TokenType.And, "and") );
                            break;
                        case "or":
                            tokens.Add( new Token(TokenType.Or, "or") );
                            break;
                        case "eqv":
                            tokens.Add( new Token(TokenType.Equals, "eqv") );
                            break;
                        default:
                            if ( CellValidator.IsValidCellName(word) )
                            {
                                tokens.Add( new Token(TokenType.CellReference, word.ToUpper()) );
                            }
                            else
                            {
                                throw new Exception("#REF!");
                            }
                            break;
                    }
                    continue;
                }

                switch (currentChar)
                {
                    case '!':
                        tokens.Add( new Token(TokenType.Not, "!") );
                        _position++;
                        break;

                    case '&':
                        if ( _position + 1 < _input.Length && _input[_position + 1] == '&' )
                        {
                            tokens.Add( new Token(TokenType.And, "&&") );
                            _position += 2;
                        }
                        else
                        {
                            throw new Exception("Unexpected character after &");
                        }
                        break;

                    case '|':
                        if ( _position + 1 < _input.Length && _input[_position + 1] == '|' )
                        {
                            tokens.Add( new Token(TokenType.Or, "||") );
                            _position += 2;
                        }
                        else
                        {
                            throw new Exception("Unexpected character after |");
                        }
                        break;

                    case '+':
                        tokens.Add( new Token(TokenType.Plus, "+") );
                        _position++;
                        break;

                    case '-':
                        tokens.Add( new Token(TokenType.Minus, "-") );
                        _position++;
                        break;

                    case '=':
                        tokens.Add( new Token(TokenType.Equals, "=") );
                        _position++;
                        break;
                    
                    case '<':
                        tokens.Add( new Token(TokenType.Less, "<") );
                        _position++;
                        break;
                    
                    case '>':
                        tokens.Add( new Token(TokenType.Greater, ">") );
                        _position++;
                        break;
                    
                    case '(':
                        tokens.Add( new Token(TokenType.LeftParen, "(") );
                        _position++;
                        break;
                    
                    case ')':
                        tokens.Add( new Token(TokenType.RightParen, ")") );
                        _position++;
                        break;
                    
                    default:
                        throw new Exception($"Unknown character: {currentChar}");
                }
            }
            tokens.Add( new Token(TokenType.End) );
            return tokens;
        }
    }
}
