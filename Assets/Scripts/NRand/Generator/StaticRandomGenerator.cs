using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public static class StaticRandomGenerator<Gen> where Gen : IRandomGenerator, new()
    {
        static Gen _instance;

        public static Gen Get()
        {
            if (_instance == null)
                _instance = new Gen();

            return _instance;
        }
    }
}
