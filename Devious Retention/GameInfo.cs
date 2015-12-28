using System.Collections.Generic;

namespace Devious_Retention
{
    /// <summary>
    /// Provides constants about the game, and also provides
    /// lists of entity types, technologies and factions for
    /// each client.
    /// </summary>
    class GameInfo
    {
        // In milliseconds
        public static int TICK_TIME { get; } = 100;
        // Melee, ranged, bombard
        public static int DAMAGE_TYPES { get; } = 3;
    }
}
