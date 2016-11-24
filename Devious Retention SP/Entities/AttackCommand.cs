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
            attacker.AddPendingCommand(this);
        }

        public override bool Tick()
        {
            if (!base.Tick()) return false;

            if (attacker is Unit)
            {
                Unit unit = (Unit)attacker;

                // Make sure we actually can attack it
                MoveWithinRange(unit);

                // Only attack if we're not moving; i.e., we're already within range of the enemy
                bool ok = true;
                foreach (Command c in attacker.GetPendingCommands())
                {
                    if(c is MoveCommand)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    PerformAttackTick();
                }
            }
            else
            {
                // TODO don't attack if we're not within range
                if(Unit.WithinRange(attacker, defender, attacker.GetRange()))
                {
                    PerformAttackTick();
                }
                else
                {
                    return false;
                }
            }

            return !defender.IsDead();
        }

        private void MoveWithinRange(Unit uAttacker)
        {
            // Move to an applicable point if we have to
            if (!Entity.WithinRange(attacker, defender, (float)attacker.GetRange()-0.2f))
            {
                // Only redo movement every 10 ticks
                if (currentTick == 10)
                {
                    uAttacker.MoveWithinRange(defender, (float)attacker.GetRange() - 0.2f, this, world);
                    currentTick = 0;
                }

                currentTick++;
            }
        }

        private void PerformAttackTick()
        {
            // TODO: Check within range every tick
            defender.Damage(attacker.GetDamage(), attacker.GetDamageType());
        }

        public void Callback()
        {
            // We're within range now
        }
    }
}
