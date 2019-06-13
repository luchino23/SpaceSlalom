using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public interface IMonotonicClock
    {
        float GetNow();
        
    }
}
