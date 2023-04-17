using ConversionMealyMoore.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversionMealyMoore.Extensions
{
    public static class ArgumentsExtension
    {
        private const string MealyMinimizationName = "mealy";
        private const string MooreMinimizationName = "moore";

        public static List<string> GetFilesNames(this string[] arguments)
        {
            CheckArguments(arguments);

            return new List<string>()
            {
                arguments[1],
                arguments[2]
            };
        }

        public static MinimizationType GetConversionType(this string[] arguments)
        {
            CheckArguments(arguments);

            return arguments[0] switch
            {
                MealyMinimizationName => MinimizationType.Mealy,
                MooreMinimizationName => MinimizationType.Moore,
                _ => throw new ArgumentOutOfRangeException($"Invalid minimization type: {arguments[0]}"),
            };
        }

        private static void CheckArguments(string[] arguments)
        {
            if (arguments.Length != 3)
            {
                throw new ArgumentException("Invalid count of arguments");
            }
            if (arguments.Any(x => string.IsNullOrWhiteSpace(x)))
            {
                throw new ArgumentException("Arguments can't be null or empty");
            }
        }
    }
}
