using ConversionMealyMoore.Machines;
using System.Collections.Generic;

namespace ConversionMealyMoore.Handlers
{
    public sealed class MachineHandler
    {
        private readonly IMachine _machine;

        public MachineHandler(List<string> parameters)
        {
            _machine = InitMachine(parameters);
        }

        public IMachine GetDetermined()
        {
            _machine.Determine();

            return _machine;
        }

        private IMachine InitMachine(List<string> parameters)
        {
            return new MooreMachine(parameters);
        }
    }
}
