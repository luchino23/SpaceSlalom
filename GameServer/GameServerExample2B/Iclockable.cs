using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public interface IClockable
    {
        float Now();
        void ClockUpdate();
        void SetFakeTime(float time);
        void IncreaseTimeStamp(float value);
        

    }
}
