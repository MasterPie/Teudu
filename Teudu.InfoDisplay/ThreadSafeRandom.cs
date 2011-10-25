using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    class ThreadSafeRandom
    {
        private static Random random = new Random();

        public static int Next(int max)
        {
            lock (random)
            {
                return random.Next(max);
            }
        }
    }
}
