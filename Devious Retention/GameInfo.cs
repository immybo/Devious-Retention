using System.Collections.Generic;

namespace Devious_Retention
{
    /// <summary>
    /// Provides constants about the game, and also provides
    /// lists of entity types, technologies and factions for
    /// each client.
    /// </summary>
    public class GameInfo
    {
        // In milliseconds
        public const int TICK_TIME = 100;
        // Melee, ranged, bombard
        public const int DAMAGE_TYPES  = 3;
        // Metal, oil, energy, science
        public const int RESOURCE_TYPES = 4;

        public List<UnitType> unitTypes { get; internal set; }
        public List<BuildingType> buildingTypes { get; internal set; }
        public List<ResourceType> resourceTypes { get; internal set; }
        public List<Technology> technologies { get; internal set; }
        public List<Faction> factions { get; internal set; }

        public GameInfo(string unitfname, string buildingfname, string resourcefname, string technologyfname, string factionfname)
        {

        }
    }
}
