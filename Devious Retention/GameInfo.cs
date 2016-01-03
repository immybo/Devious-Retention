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
        public const int TICK_TIME = 100;
        // Melee, ranged, bombard
        public const int DAMAGE_TYPES  = 3;
        // Metal, oil, energy, science
        public const int RESOURCE_TYPES = 4;
    }
}
