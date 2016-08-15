using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Defines an entity which has hitpoints.
    /// This entity can then be attacked and healed.
    /// </summary>
    interface Attackable
    {
        void Damage(int amount, int type);
        void Heal(int amount);
        bool IsDead();
    }
}
