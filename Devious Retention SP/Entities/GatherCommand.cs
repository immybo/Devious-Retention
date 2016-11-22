using Devious_Retention_SP.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public class GatherCommand : Command, ICallback
    {
        private Gatherer gatherer;
        private Gatherable gathered;
        private World world;

        private bool inRange;
        private int currentTick;

        public GatherCommand(Gatherer gatherer, Gatherable gathered, World world)
        {
            this.gatherer = gatherer;
            this.gathered = gathered;
            this.world = world;
        }

        public override void Execute()
        {
            inRange = false;
            gatherer.MoveWithinRange(gathered, 1, this, world);
            currentTick = 1;
            gatherer.AddPendingCommand(this);
        }

        public void Callback()
        {
            inRange = true;
        }

        public override bool Tick()
        {
            if (!inRange) return true;

            if (currentTick % gatherer.GatherTicks == 0 && gathered.CurrentResourceCount() > 0)
            {
                gathered.Gather(gatherer.Player, gatherer.GatherAmount);
            }

            currentTick++;
            return gathered.CurrentResourceCount() > 0;
        }
    }
}
