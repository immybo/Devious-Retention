using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public interface Attacker : IEntity
    {
        float GetRange();
        int GetDamage();
        int GetDamageType();
        int GetAttackTime();
    }
}
