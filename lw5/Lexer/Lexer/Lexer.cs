using System;
using System.Collections.Generic;
using System.IO;
using Lexer.Types;

namespace Lexer
{
    internal class Lexer
    {
        public Dictionary<TokenType, string> tokenTypeToString = new Dictionary<TokenType, string>()
        {
            { TokenType.Keyword, "KEYWORD"},
            { TokenType.Identifier, "IDENTIFIER"},
            { TokenType.Number, "NUMBER"},
            { TokenType.Separator, "SEPARATOR"},

            { TokenType.Addition, "ADD"},
            { TokenType.Subtraction, "SUB"},
            { TokenType.Multiplication, "MUL"},
            { TokenType.Division, "DIV"},
            { TokenType.Equating, "ASG"},

            { TokenType.LogicEquivalent, "EQV"},
            { TokenType.LogicNotEquivalent, "NEQV"},
            { TokenType.LogicLess, "LES"},
            { TokenType.LogicMore, "GRT"},
            { TokenType.LogicLessOrEquivalent, "LES_OR_EQV"},
            { TokenType.LogicMoreOrEquivalent, "LES_OR_EQV"},
            { TokenType.LogicAnd, "LES_OR_EQV"},
            { TokenType.LogicOr, "LES_OR_EQV"},
            { TokenType.QuotationMark, "LES_OR_EQV"},
            { TokenType.Comment, "LES_OR_EQV"}

        };

        public Dictionary<string, TokenType> stringToTokenType = new Dictionary<string, TokenType>()
        {
            { "BEGIN", TokenType.Keyword},
            { "END", TokenType.Keyword},
            { "READ", TokenType.Keyword},
            { "WRITE", TokenType.Keyword},
            { "GET", TokenType.Keyword},
            { "CONST", TokenType.Keyword},
            { "LET", TokenType.Keyword},
            { "VAR", TokenType.Keyword},
            { "IF", TokenType.Keyword},
            { "THEN", TokenType.Keyword},
            { "ELSE", TokenType.Keyword},
            { "WHILE", TokenType.Keyword},
            { "DO", TokenType.Keyword},
            { "FOR", TokenType.Keyword},
            { "TRUE", TokenType.Keyword},
            { "FALSE", TokenType.Keyword},
            { "INTEGER", TokenType.Keyword},
            { "STRING", TokenType.Keyword},

            { "==", TokenType.LogicEquivalent},
            { "!=", TokenType.LogicNotEquivalent},
            { "<=", TokenType.LogicLessOrEquivalent},
            { ">=", TokenType.LogicMoreOrEquivalent},
            { "&&", TokenType.LogicAnd},
            { "||", TokenType.LogicOr},

            { "//", TokenType.Comment}
        };

        public Dictionary<char, TokenType> charToTokenType = new Dictionary<char, TokenType>()
        {
            { ' ', TokenType.Separator},
            { '(', TokenType.Separator},
            { ')', TokenType.Separator},
            { ';', TokenType.Separator},
            { ':', TokenType.Separator},
            { ',', TokenType.Separator},

            { '+', TokenType.Addition},
            { '-', TokenType.Subtraction},
            { '*', TokenType.Multiplication},
            { '/', TokenType.Division},
            { '=', TokenType.Equating},
            { '<', TokenType.LogicLess},
            { '>', TokenType.LogicMore},

            { '"', TokenType.QuotationMark},

            //some part of oter that can be seporator
            { '!', TokenType.LogicNotEquivalent},
            { '&', TokenType.LogicAnd},
            { '|', TokenType.LogicOr},

        };

        public Dictionary<char, TokenType> charToTokenSecondPart = new Dictionary<char, TokenType>()
        {

            { '/', TokenType.Comment},
            //some part of oter that can be seporator
            { '=', TokenType.LogicNotEquivalent},
            { '&', TokenType.LogicAnd},
            { '|', TokenType.LogicOr},

        };

        private int _lineIndex = 0;
        private int _lineCount = 0;
        private int _lineFromFileSize = -1;

        private string _buffer = "";
        private string _linefromFile = "";
        private StreamReader _rs;


        public Lexer( string inputF )
        {
            _rs = new StreamReader( inputF );
        }

        public void EndWork()
        {
            _rs.Close();
        }


        public string getNextLexerm()
        {
            if ( _lineIndex >= ( _lineFromFileSize - 1 ) )
            {
                GetNexLineFromFile();
            }

            DeleteStartSpases();
            GetConnectedSimbolsBeforSeporator();

            if ( _buffer.Length == 1 && charToTokenSecondPart.ContainsKey( _linefromFile[ _lineIndex ] ) )
            {
                _buffer += _linefromFile[ _lineIndex ];
                _lineIndex += 1;

                if ( stringToTokenType[ _buffer ] == TokenType.Comment )
                {
                    _lineIndex = _lineFromFileSize - 1;
                }

                Console.WriteLine( $"in line {_lineCount} pos {_lineIndex - _buffer.Length + 1} :  {_buffer} - {tokenTypeToString[ stringToTokenType[ _buffer ] ]}" );
                return _buffer;
            }

            if ( _buffer.Length == 1 && charToTokenType.ContainsKey( _buffer[ 0 ] ) )
            {
                Console.WriteLine( $"in line {_lineCount} pos {_lineIndex - _buffer.Length + 1} : {_buffer} - {tokenTypeToString[ charToTokenType[ _buffer[ 0 ] ] ]}" );
                return _buffer;
            }

            if ( stringToTokenType.ContainsKey( _buffer ) )
            {
                Console.WriteLine( $"in line {_lineCount} pos {_lineIndex - _buffer.Length + 1} : {_buffer} - {tokenTypeToString[ stringToTokenType[ _buffer ] ]}" );
                return _buffer;
            }

            try
            {
                int numVal = Int32.Parse( _buffer );
                Console.WriteLine( $"in line {_lineCount} pos {_lineIndex - _buffer.Length + 1} : {_buffer} - {tokenTypeToString[ TokenType.Number ]}" );
                return _buffer;

            }
            catch ( FormatException e )
            {
                Console.WriteLine( $"in line {_lineCount} pos {_lineIndex - _buffer.Length + 1} : {_buffer} - {tokenTypeToString[ TokenType.Identifier ]}" );
                return _buffer;
            }
        }

        private void GetNexLineFromFile()
        {
            if ( _rs.EndOfStream )
            {
                Console.WriteLine( "Incorrect operation, no more lexem in file" );
                throw new Exception( "Incorrect operation, no more lexem in file" );
            }

            _linefromFile = _rs.ReadLine();
            _lineIndex = 0;
            _lineCount += 1;
            _lineFromFileSize = _linefromFile.Length;


        }

        private void DeleteStartSpases()
        {
            while ( _linefromFile[ _lineIndex ] == ' ' && _lineIndex < _lineFromFileSize )
            {
                _lineIndex += 1;
            }
        }

        private void GetConnectedSimbolsBeforSeporator()
        {
            _buffer = "";

            do
            {
                _buffer += _linefromFile[ _lineIndex ];
                _lineIndex += 1;
            }
            while ( ( _lineIndex < _lineFromFileSize )
            && !charToTokenType.ContainsKey( _linefromFile[ _lineIndex ] )
            && !charToTokenType.ContainsKey( _buffer[ 0 ] ) );
        }
    }
}
