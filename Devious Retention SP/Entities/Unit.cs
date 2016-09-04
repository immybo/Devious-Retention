using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Units are entities that:
    /// - Can be attacked
    /// - Can move
    /// - Can usually attack
    /// - Often have special abilities
    /// </summary>
    public abstract class Unit : Entity, Attackable
    {
        protected int maxHitpoints;
        protected int hitpoints;

        public Unit(Player player, double x, double y, double size)
            : base(player, x, y, size)
        {
            
        }
        
        public abstract void Damage(int amount, int damageType);
        public virtual void Heal(int amount)
        {
            if (hitpoints < maxHitpoints - amount)
                hitpoints += amount;
            else
                hitpoints = maxHitpoints;
        }
        public virtual bool IsDead()
        {
            return hitpoints <= 0;
        }

        public override Command[] ValidCommands()
        {
            return new Command[]
            {
                Command.MOVE
            };
        }
    }
}
