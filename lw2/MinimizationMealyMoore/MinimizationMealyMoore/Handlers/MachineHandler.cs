using System;
using System.Collections.Generic;
using ConversionMealyMoore.Machines;
using ConversionMealyMoore.Types;

namespace ConversionMealyMoore.Handlers
{
    public sealed class MachineHandler
    {
        private readonly ConversionType _conversionType;
        private readonly IMachine _machine;

        public MachineHandler( List<string> parameters, ConversionType conversionType )
        {
            _conversionType = conversionType;
            _machine = InitMachine( parameters );
        }

        public IMachine GetConverted()
        {
            return _machine.Convert( _conversionType );
        }

        private IMachine InitMachine( List<string> parameters )
        {
            return _conversionType switch
            {
                ConversionType.ToMoore => new MealyMachine( parameters ),
                ConversionType.ToMealy => new MooreMachine( parameters ),
                _ => throw new ArgumentOutOfRangeException( nameof( _conversionType ) ),
            };
        }
    }
}
