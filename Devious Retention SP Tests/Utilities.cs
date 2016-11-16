using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devious_Retention_SP;

namespace Devious_Retention_SP_Tests
{
    public static class Utilities
    {
        /// <summary>
        /// Executes the given command and ticks the given entity
        /// until the command removes itself from the given entity's
        /// command list.
        /// </summary>
        public static void ApplyCommandSynchronous(Command command, IEntity entity)
        {
            command.Execute();
            while (entity.GetPendingCommands().Contains(command))
                entity.Tick();
        }

        public static WorldConfiguration BuildWorldFromTemplate(WorldTemplate template)
        {
            Player[] players = new Player[template.numPlayers];
            World world = new World();
            for (int i = 0; i < players.Length; i++)
                players[i] = new NullPlayer(world);
            return new WorldConfiguration(world, players);
        }
    }
}
