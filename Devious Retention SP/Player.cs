using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Color Color { get; private set; }
        public int[] Resources { get; private set; }
        protected World world;

        public Player(World world, Color color)
        {
            this.Color = color;
            this.world = world;
        }

        public void SetResource(int resourceID, int newAmount)
        {
            if (newAmount < 0)
                throw new ArgumentOutOfRangeException("Attempting to give a player a negative amount of a resource.");
            if (resourceID < 0 || resourceID >= Resources.Length)
                throw new ArgumentOutOfRangeException("Attempting to set an invalid resource ID to a player.");

            Resources[resourceID] = newAmount;
        }

        public void ChangeResource(int resourceID, int change)
        {
            if (resourceID < 0 || resourceID >= Resources.Length)
                throw new ArgumentOutOfRangeException("Attempting to set an invalid resource ID to a player.");

            SetResource(resourceID, Resources[resourceID] + change);
        }
    }
}
