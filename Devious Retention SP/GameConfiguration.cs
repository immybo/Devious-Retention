using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public class GameConfiguration
    {
        public readonly int DEFAULT_TICK_TIME = 30;

        public GameConfiguration(int defaultTickTime)
        {
            this.DEFAULT_TICK_TIME = defaultTickTime;
        }
    }
}
