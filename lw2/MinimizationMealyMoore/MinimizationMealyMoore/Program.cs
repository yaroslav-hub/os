using System;
using System.Collections.Generic;
using System.IO;
using ConversionMealyMoore.Extensions;
using ConversionMealyMoore.Handlers;
using ConversionMealyMoore.Machines;
using ConversionMealyMoore.Types;

namespace ConversionMealyMoore
{
    class Program
    {
        static void Main( string[] args )
        {
            try
            {
                List<string> filesNames = args.GetFilesNames();
                ConversionType conversionType = args.GetConversionType();

                using StreamReader inFile = new( filesNames[ 0 ] );
                using StreamWriter outFile = new( filesNames[ 1 ] );

                FileHandler fileHandler = new( inFile, outFile );

                MachineHandler machineHandler = new( fileHandler.ReadAllLines(), conversionType );
                IMachine convertedMachine = machineHandler.GetConverted();

                fileHandler.WriteLines( convertedMachine.GetParameters() );
            }
            catch ( ArgumentException e )
            {
                Console.WriteLine( $"Error! Message: {e.Message}" );
            }
        }
    }
}
