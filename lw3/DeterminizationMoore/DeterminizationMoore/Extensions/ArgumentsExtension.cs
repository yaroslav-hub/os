using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversionMealyMoore.Extensions
{
    public static class ArgumentsExtension
    {
        public static List<string> GetFilesNames(this string[] arguments)
        {
            CheckArguments(arguments);

            return new List<string>()
            {
                arguments[0],
                arguments[1]
            };
        }

        private static void CheckArguments(string[] arguments)
        {
            if (arguments.Length != 2)
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
