using ConversionMealyMoore.Machines;
using ConversionMealyMoore.Types;
using System;
using System.Collections.Generic;

namespace ConversionMealyMoore.Handlers
{
    public sealed class MachineHandler
    {
        private readonly MinimizationType _minimizationType;
        private readonly IMachine _machine;

        public MachineHandler(List<string> parameters, MinimizationType conversionType)
        {
            _minimizationType = conversionType;
            _machine = InitMachine(parameters);
        }

        public IMachine GetMinimized()
        {
            _machine.Minimize();

            return _machine;
        }

        private IMachine InitMachine(List<string> parameters)
        {
            return _minimizationType switch
            {
                MinimizationType.Moore => new MooreMachine(parameters),
                MinimizationType.Mealy => new MealyMachine(parameters),
                _ => throw new ArgumentOutOfRangeException(nameof(_minimizationType)),
            };
        }
    }
}
