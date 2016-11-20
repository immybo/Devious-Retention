using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP.Entities
{
    public abstract class Gatherer : Unit
    {
        public int GatherAmount { get; private set; }
        public int GatherTicks { get; private set; }

        public Gatherer(Player player, double x, double y, double size, float movementSpeed, string name, int gatherAmount, int gatherTicks)
            : base(player, x, y, size, movementSpeed, name)
        {
            this.GatherAmount = gatherAmount;
            this.GatherTicks = gatherTicks;
        }
    }
}
