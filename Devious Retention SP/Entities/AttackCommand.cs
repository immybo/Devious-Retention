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
                if (movement == null)
                {
                    PerformAttackTick();
                }
            }
            else
            {
                PerformAttackTick();
            }

            return !defender.IsDead();
        }

        private void MoveWithinRange(Unit uAttacker)
        {
            // Move to an applicable point if we have to
            if (!WithinRangeBuffer(attacker, defender, 0.2f))
            {
                // Only redo movement every 10 ticks
                if (currentTick == 10)
                {
                    if(movement != null)
                    {
                        uAttacker.RemovePendingCommand(movement);
                    }
                    PointF attackPoint = GetAttackPoint(attacker, defender, world);
                    this.movement = new MoveCommand(uAttacker, attackPoint, world);
                    movement.Execute();

                    currentTick = 0;
                }

                currentTick++;
            }
            else
            {
                movement = null;
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
            PointF defenderPoint = defender.GetCenterPosition();
            PointF attackerPoint = attacker.GetCenterPosition();

            PointF vector = new PointF(defenderPoint.X - attackerPoint.X, defenderPoint.Y - attackerPoint.Y);
            double vectorLength = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

            if (vectorLength <= attacker.GetRange() - 0.5)
                return attackerPoint;

            PointF inRangeVector = new PointF((float)(vector.X * ((attacker.GetRange()-0.5)/vectorLength)), (float)(vector.Y * ((attacker.GetRange()-0.5)/vectorLength)));

            PointF newPoint = new PointF(defenderPoint.X - inRangeVector.X, defenderPoint.Y - inRangeVector.Y);
            return newPoint;
        }
    }
}
