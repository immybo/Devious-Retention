using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Devious_Retention_SP
{
    public class AttackCommand : Command, ICallback
    {
        private Attacker attacker;
        private Attackable defender;
        private World world;
        
        private int currentTick = 10;

        public AttackCommand(Attacker attacker, Attackable defender, World world)
        {
            this.attacker = attacker;
            this.defender = defender;
            this.world = world;
        }

        public override void Execute()
        {
            attacker.OverrideExecutingCommand(this);
        }

        public override bool Tick()
        {
            if (!base.Tick()) return false;

            if(Entity.WithinRange(attacker, defender, attacker.GetRange()))
            {
                PerformAttackTick();
                return !defender.IsDead();
            }
            else if(attacker is Unit)
            {
                Unit uAttacker = (Unit)attacker;
                MoveWithinRange(uAttacker);
                return false; // We re-add ourselves on the callback
            }
            else
            {
                return false;
            }
        }

        private void MoveWithinRange(Unit uAttacker)
        {
            // Only redo movement every 10 ticks
            if (currentTick == 10)
            {
                uAttacker.MoveWithinRange(defender, attacker.GetRange() - 0.2f, this, world);
                currentTick = 0;
            }

            currentTick++;
        }

        private void PerformAttackTick()
        {
            defender.Damage(attacker.GetDamage(), attacker.GetDamageType());
        }

        public void Callback()
        {
            // We're within range now
            attacker.OverrideExecutingCommand(this);
        }
    }
}
