using System.Collections.Generic;

namespace ConversionMealyMoore.Machines
{
    public interface IMachine
    {
        List<string> GetParameters();
        void Determine();
    }
}
