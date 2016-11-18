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
        public readonly int RESOURCE_COUNT = 4;

        public GameConfiguration(int defaultTickTime, int resourceCount)
        {
            this.DEFAULT_TICK_TIME = defaultTickTime;
            this.RESOURCE_COUNT = resourceCount;
        }

        public GameConfiguration() { }
    }
}
