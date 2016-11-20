using Devious_Retention_SP.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public class GatherCommand : Command
    {
        private Gatherer gatherer;
        private Gatherable gathered;

        public GatherCommand(Gatherer gatherer, Gatherable gathered)
        {
            this.gatherer = gatherer;
            this.gathered = gathered;
        }

        public override void Execute()
        {
        }

        public override bool Tick()
        {
            return false;
        }
    }
}
