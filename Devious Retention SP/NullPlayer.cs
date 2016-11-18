using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Devious_Retention_SP
{
    public class NullPlayer : Player
    {
        public NullPlayer(World world) : base(world, Color.Black, new GameConfiguration())
        {
        }
    }
}
