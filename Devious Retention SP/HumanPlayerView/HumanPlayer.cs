using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Represents a human player on the local machine.
    /// </summary>
    public class HumanPlayer : Player, HumanPlayerListener
    {
        private HumanPlayerWindow window;

        public HumanPlayer(World world)
            : base(world)
        {
            window = new HumanPlayerWindow(this);
        }
    }
}
