using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// There is one GameServer per game, whether the game is
    /// single- or multi-player. The GameServer relays actions
    /// to different clients, and receives actions from them
    /// to process.
    /// </summary>
    class GameServer
    {
        private List<STCConnection> connections;

        // Contains only generic base lists; i.e. they are not edited by whatever technologies clients have researched
        private GameInfo info;
        // One map for every player, contains only technologies that they have researched
        private List<Dictionary<String, Technology>> researched;

        // This map is used to determine what can and can't happen; e.g. whether or not a building can be placed at the given location
        private Map map;

        // Lists of all entities in the game
        private List<Unit> units;
        private List<Building> buildings;
        private List<Resource> resources;

        /// <summary>
        /// To create a GameServer, all clients (STCConnections) must be provided,
        /// as well as a generic base GameInfo. Note that the GameServer treats every
        /// game as multiplayer, and so computer controlled players will merely
        /// spoof having a connection.
        /// </summary>
        public GameServer(List<STCConnection> connections, GameInfo info)
        {

        }

        /// <summary>
        /// Attempts to place a foundation of the given building type for the given player at (x,y).
        /// Returns whether or not it was successful.
        /// </summary>
        public bool CreateBuilding(int player, BuildingType building, double x, double y)
        {
            return false;
        }

        /// <summary>
        /// Attempts to create a unit of the given type from the given building.
        /// Returns false if the given type can't be created from that building with
        /// the technologies of the player that owns the building. Otherwise 
        /// returns true.
        /// </summary>
        public bool CreateUnit(Building sourceBuilding, UnitType unit)
        {
            return false;
        }

        /// <summary>
        /// Adds the given technology to the list of technologies that the
        /// given player has researched. This can't fail.
        /// </summary>
        public void ResearchTechnology(int player, Technology technology)
        {

        }

        /// <summary>
        /// Removes the appropriate amount of health from the target,
        /// given that the source attacks the target once.
        /// Does nothing if either entity is a resource, or if either
        /// entity does not exist in the server's list of entities.
        /// </summary>
        public void DamageEntity(Entity source, Entity target)
        {

        }

        /// <summary>
        /// Moves the given unit by (dX, dY).
        /// Does NO CHECKING as to whether the unit would actually be
        /// able to move by this much; this is assumed when the command
        /// is sent.
        /// </summary>
        public void MoveUnit(Unit unit, double dX, double dY)
        {

        }

        /// <summary>
        /// Removes the given entity from the list of entities.
        /// Does nothing if the given entity does not exist in the server's
        /// lists of entities.
        /// </summary>
        public void DeleteEntity(Entity entity)
        {

        }
    }
}
