using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// A player holds responsibility for getting input from one specific player
    /// and updating the world as a result. The player is terminated externally
    /// on game end, but otherwise does not answer to any class except possibly
    /// ones that are defined in relation to a specific type of player.
    /// </summary>
    public abstract class Player
    {
        // TODO add player colors
        protected World world;

        public Player(World world)
        {
            this.world = world;
        }
    }
}
