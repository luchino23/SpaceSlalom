using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GameServerExample2B.Test
{
    public class FakeClock : IMonotonicClock
    {
        private float timeStap;

        public FakeClock(float timeStamp = 0)
        {
            this.timeStap = timeStamp;
        }

        public float GetNow()
        {
            return timeStap;
        }

        public void IncreaseTimeStamp(float delta)
        {
            if(delta <= 0)
            {
                throw new Exception("invalid delta value");
            }
            timeStap += delta;
        }

    }
}
