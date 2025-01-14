using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NRand
{
    public static class StaticRandomGenerator<Gen> where Gen : IRandomGenerator, new()
    {
        static ThreadLocal<Gen> _instance;

        public static Gen Get()
        {
            if (_instance == null)
                _instance = new ThreadLocal<Gen>(()=> { return new Gen(); });

            return _instance.Value;
        }
    }
}
