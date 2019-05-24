using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GameServerExample2B
{
    public class GameClock:IMonotonicClock
    {
        public float clock;

        public GameClock()
        {
            clock = 0f;
        }

        public float GetNow()
        {
            return clock = Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency;
        }
    }
}
