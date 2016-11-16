using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devious_Retention_SP;

namespace Devious_Retention_SP_Tests
{
    public class WorldConfiguration
    {
        public World world { get; private set; }
        public Player[] players { get; private set; }

        public WorldConfiguration(World world, Player[] players)
        {
            this.world = world;
            this.players = players;
        }
    }
}
