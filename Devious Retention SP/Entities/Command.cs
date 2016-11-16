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
    public abstract class Command
    {
        private bool finished = false;

        /// <summary>
        /// Sets a command as finished; that is, it will no longer execute
        /// and will be removed from its entity as soon as possible.
        /// </summary>
        public void SetFinished()
        {
            finished = true;
        }

        public abstract void Execute();

        /// <summary>
        /// A command is not obliged to take time, but if it does,
        /// it should add itself to an entity's list of pending commands.
        /// This will cause the entity to call this method every tick.
        /// The return value of this indicates whether it should still
        /// run in the next tick or not.
        /// </summary>
        public virtual bool Tick()
        {
            return !finished;
        }
    }
}
