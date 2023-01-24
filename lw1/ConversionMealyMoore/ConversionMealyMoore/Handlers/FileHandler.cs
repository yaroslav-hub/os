using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConversionMealyMoore.Handlers
{
    public sealed class FileHandler
    {
        private readonly StreamReader _input;
        private readonly StreamWriter _output;

        public FileHandler( StreamReader input, StreamWriter output )
        {
            _input = input;
            _output = output;
        }

        public string ReadLine()
        {
            return ( !_input.EndOfStream )
                ? _input.ReadLine()
                : null;
        }

        public List<string> ReadAllLines()
        {
            List<string> lines = new();
            while ( !_input.EndOfStream )
            {
                lines.Add( ReadLine() );
            }

            return lines
                .Where( s => !string.IsNullOrEmpty( s ) )
                .ToList();
        }

        public void WriteLine( string line )
        {
            if ( string.IsNullOrEmpty( line ) )
            {
                return;
            }

            _output.WriteLine( line );
        }

        public void WriteLines( List<string> lines )
        {
            lines.ForEach( x => WriteLine( x ) );
        }
    }
}
