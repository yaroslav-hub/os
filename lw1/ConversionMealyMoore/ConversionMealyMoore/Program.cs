using ConversionMealyMoore.Extensions;
using ConversionMealyMoore.Handlers;
using ConversionMealyMoore.Types;
using System.Collections.Generic;
using System.IO;

namespace ConversionMealyMoore
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> filesNames = args.GetFilesNames();
            ConversionType conversionType = args.GetConversionType();

            using StreamReader inFile = new(filesNames[0]);
            using StreamWriter outFile = new(filesNames[1]);

            FileHandler fileHandler = new(inFile, outFile);


        }
    }
}
