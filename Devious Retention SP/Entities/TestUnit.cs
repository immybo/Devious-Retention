using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP.Entities
{
    class TestUnit : Unit
    {
        public TestUnit(Player player, double x, double y)
            : base(player, x, y)
        {

        }

        public override void Damage(int amount, int damageType)
        {
            this.hitpoints -= amount;
        }

        public override Command[] ValidCommands()
        {
            return new Command[]
            {
                Command.ATTACK
            }.Concat(base.ValidCommands()).ToArray();
        }
    }
}
