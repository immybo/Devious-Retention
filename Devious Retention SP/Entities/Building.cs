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

        private int builtHitpoints;
        public bool IsFullyBuilt { get; private set; }
        // A higher value will cause this building to be built slower.
        public float BuildResistance { get; private set; }

        public Building(Player player, double x, double y, double size, string name, float buildResistance)
            : base(player, x, y, size, name)
        {
            IsFullyBuilt = false;
            builtHitpoints = 0;
            this.BuildResistance = buildResistance;
        }

        /// <summary>
        /// Adds up to the given amount of hitpoints to this building
        /// through building it. No more hitpoints may be added if
        /// IsFullyBuilt.
        /// </summary>
        /// <param name="amount">The amount of hitpoints to build.</param>
        public void Build(int amount)
        {
            if (IsFullyBuilt)
            {
                return;
            }
            else if (builtHitpoints + amount < MaxHitpoints)
            { 
                builtHitpoints += amount;
                Heal(amount);
            }
            else
            {
                Heal(MaxHitpoints - builtHitpoints);
                builtHitpoints = MaxHitpoints;
                IsFullyBuilt = true;
            }
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
