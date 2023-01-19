using ConversionMealyMoore.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversionMealyMoore.Extensions
{
    public static class ArgumentsExtension
    {
        private const string MealyToMooreConversionName = "mealy-to-moore";
        private const string MooreToMealyConversionName = "moore-to-mealy";

        public static List<string> GetFilesNames(this string[] arguments)
        {
            CheckArguments(arguments);

            return new List<string>()
            {
                arguments[1],
                arguments[2]
            };
        }

        public static ConversionType GetConversionType(this string[] arguments)
        {
            CheckArguments(arguments);

            return arguments[0] switch
            {
                MealyToMooreConversionName => ConversionType.MealyToMoore,
                MooreToMealyConversionName => ConversionType.MooreToMealy,
                _ => throw new ArgumentOutOfRangeException($"Invalid conversion type: {arguments[0]}"),
            };
        }

        private static void CheckArguments(string[] arguments)
        {
            if (arguments.Length != 3)
            {
                throw new ArgumentException("Invalid count of arguments");
            }
            if (arguments.Any(x => string.IsNullOrEmpty(x)))
            {
                throw new ArgumentException("Arguments can't be null or empty");
            }
        }
    }
}
