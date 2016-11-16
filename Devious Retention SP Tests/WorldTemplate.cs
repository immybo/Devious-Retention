using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP_Tests
{
    public class WorldTemplate
    {
        public int numPlayers { get; private set; }

        public WorldTemplate(int numPlayers)
        {
            this.numPlayers = numPlayers;
        }
    }
}
