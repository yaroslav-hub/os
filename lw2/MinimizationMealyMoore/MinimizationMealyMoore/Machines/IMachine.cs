﻿using System.Collections.Generic;
using ConversionMealyMoore.Types;

namespace ConversionMealyMoore.Machines
{
    public interface IMachine
    {
        List<string> GetParameters();
        void Minimize();
    }
}
