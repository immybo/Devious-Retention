using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// A command is something that can be executed with relation to an entity.
    /// </summary>
    public interface Command
    {
        void Execute();
        /// <summary>
        /// A command is not obliged to take time, but if it does,
        /// it should add itself to an entity's list of pending commands.
        /// This will cause the entity to call this method every tick.
        /// The return value of this indicates whether it should still
        /// run in the next tick or not.
        /// </summary>
        bool Tick();
    }
}
