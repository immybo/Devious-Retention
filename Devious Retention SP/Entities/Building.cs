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
    /// Buildings are entities that:
    /// - Can be attacked
    /// - Can't move
    /// - Can sometimes attack
    /// - Can sometimes train units
    /// - Can sometimes research technologies
    /// </summary>
    public abstract class Building : Entity, Attackable
    {
        public int MaxHitpoints { get; protected set; }
        public int Hitpoints { get; protected set; }

        public Building(Player player, double x, double y, double size, string name)
            : base(player, x, y, size, name)
        {
        }

        public abstract void Damage(int amount, int damageType);
        public virtual void Heal(int amount)
        {
            if (Hitpoints < MaxHitpoints - amount)
                Hitpoints += amount;
            else
                Hitpoints = MaxHitpoints;
        }
        public virtual bool IsDead()
        {
            return Hitpoints <= 0;
        }
    }
}
