using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
 

namespace GameServerExample2B
{
    public class GameClock : IClockable
    {
        private float timeStamp;

        public GameClock(float timeStamp = 0)
        {
            this.timeStamp = timeStamp;
        }

        public float Now()
        {
            return timeStamp;
        }

        public void ClockUpdate()
        {
            timeStamp = Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency;
        }

        public void SetFakeTime(float time)
        {
            
        }

        public void IncreaseTimeStamp(float value)
        {
            if (value <= 0)
            {
                throw new Exception("Value can't back in Time");
            }

            timeStamp += value;
        }

    }

}

