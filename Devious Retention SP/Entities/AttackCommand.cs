using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Devious_Retention_SP
{
    public class AttackCommand : Command
    {
        private Attacker attacker;
        private Attackable defender;
        private World world;

        private MoveCommand movement = null;

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

                // We've gotten to the end location and can now attack
                if (movement != null && !unit.GetPendingCommands().Contains(movement))
                {
                    movement = null;
                }

                // Only attack if we're not moving; i.e., we're already within range of the enemy
                if (movement == null)
                {
                    PerformAttackTick();
                }
            }
            else
            {
                PerformAttackTick();
            }

            return true;
        }

        private void MoveWithinRange(Unit uAttacker)
        {
            // Move to an applicable point if we have to
            if (!WithinRangeBuffer(attacker, defender, 0.5f))
            {
                PointF attackPoint = GetAttackPoint(attacker, defender, world);
                this.movement = new MoveCommand(uAttacker, attackPoint, world);
                movement.Execute();
            }
        }

        private void PerformAttackTick()
        {
            defender.Damage(attacker.GetDamage(), attacker.GetDamageType());
        }

        /// <summary>
        /// Returns whether or not the defender is within
        /// the attacker's range.
        /// within 
        /// </summary>
        public static bool WithinRange(Attacker attacker, Attackable defender)
        {
            return WithinRange(attacker, defender, attacker.GetRange());
        }

        /// <summary>
        /// Returns whether or not the defender is
        /// within the attacker's range - buffer squares
        /// of the defender.
        /// </summary>
        public static bool WithinRangeBuffer(Attacker attacker, Attackable defender, float buffer)
        {
            return WithinRange(attacker, defender, attacker.GetRange() + buffer);
        }

        /// <summary>
        /// Returns whether or not the two given entities
        /// are within the given range of each other.
        /// </summary>
        public static bool WithinRange(IEntity e1, IEntity e2, float range)
        {
            double xDiff = e1.X - e2.X;
            double yDiff = e1.Y - e2.Y;
            double totalDiff = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            return totalDiff <= range;
        }

        /// <summary>
        /// Returns the point which
        /// - Is within this entity's range of the defender
        /// - The attacker can move to
        /// - Has the shortest path from the attacker's
        ///   current position in the given world
        /// </summary>
        /// <returns></returns>
        public static PointF GetAttackPoint(Attacker attacker, Attackable defender, World world)
        {
            PointF defenderPoint = defender.GetPosition();
            PointF attackerPoint = defender.GetPosition();
            PointF vector = new PointF(defenderPoint.X - attackerPoint.X, defenderPoint.Y - attackerPoint.Y);
            PointF attackPoint = new PointF(0, 0);

            float toGo = attacker.GetRange();

            if(vector.X != 0)
            {
                if(Math.Abs(vector.X) <= attacker.GetRange() / 2)
                {
                    toGo -= Math.Abs(vector.X);
                }
                else if (vector.X < 0) {
                    attackPoint.X = defender.GetPosition().X + attacker.GetRange()/2;
                }
                else
                {
                    attackPoint.X = defender.GetPosition().X - attacker.GetRange()/2;
                }
            }
            
            if(vector.Y != 0)
            {
                if(Math.Abs(vector.Y) <= toGo)
                {
                    toGo -= Math.Abs(vector.Y);
                }
                else if(vector.Y < 0)
                {
                    attackPoint.Y = defender.GetPosition().Y + attacker.GetRange() / 2;
                }
                else
                {
                    attackPoint.Y = defender.GetPosition().Y - attacker.GetRange() / 2;
                }
            }

            return attackPoint;
        }
    }
}
