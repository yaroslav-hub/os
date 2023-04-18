using System;
using System.Collections.Generic;
using System.IO;
using Lexer.Extensions;

namespace Lexer
{
    internal class Program
    {
        static void Main( string[] args )
        {
            List<string> filesNames = args.GetFilesNames();

            using StreamReader inFile = new( filesNames[ 0 ] );

            Lexer lex = new Lexer( inputF );

            try
            {
                while ( true )
                {
                    inputF = lex.getNextLexerm();
                }
            }
            catch ( Exception e )
            {
                lex.EndWork();
                //Console.WriteLine(e);
            }
        }
    }
}
