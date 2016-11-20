using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Defines an entity which can be gathered from.
    /// </summary>
    public interface Gatherable
    {
        int MaxResourceCount();
        int CurrentResourceCount();
        void Gather(Player player, int amount);
    }
}
