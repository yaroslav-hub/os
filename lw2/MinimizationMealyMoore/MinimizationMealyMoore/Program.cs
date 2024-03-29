﻿using System;
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
                MinimizationType minimizationType = args.GetConversionType();

                using StreamReader inFile = new( filesNames[ 0 ] );
                using StreamWriter outFile = new( filesNames[ 1 ] );

                FileHandler fileHandler = new( inFile, outFile );

                MachineHandler machineHandler = new( fileHandler.ReadAllLines(), minimizationType );
                IMachine minimizedMachine = machineHandler.GetMinimized();

                fileHandler.WriteLines( minimizedMachine.GetParameters() );
            }
            catch ( ArgumentException e )
            {
                Console.WriteLine( $"Error! Message: {e.Message}" );
            }
        }
    }
}
